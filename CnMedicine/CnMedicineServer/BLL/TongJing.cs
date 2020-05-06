
using CnMedicineServer.Models;
using OW;
using OW.Data.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web.Http;

namespace CnMedicineServer.Bll
{
    /// <summary>
    /// 提取症状表的数据类。
    /// </summary>
    public class TongJingSigns
    {
        /*
         编号	问题	    症状	类型号	问题类型	说明
         101    疼痛时间	月经期	B	    选择	类型号：A参与计算且阈值为0.7；B不参与计算；C参与计算且没有阈值
        */

        /// <summary>
        /// 构造函数。
        /// </summary>
        public TongJingSigns()
        {

        }

        /// <summary>
        /// 编号。
        /// </summary>
        [TextFieldName("编号")]
        public int Number { get; set; }

        /// <summary>
        /// 问题。
        /// </summary>
        [TextFieldName("问题")]
        public string Question { get; set; }

        /// <summary>
        /// 症状。
        /// </summary>
        [TextFieldName("症状")]
        public string ZhengZhuang { get; set; }

        /// <summary>
        /// 类型号。
        /// </summary>
        [TextFieldName("类型号")]
        public string TypeString { get; set; }

        /// <summary>
        /// 问题类型。
        /// </summary>
        [TextFieldName("问题类型")]
        public QuestionsKind QuestionsKind { get; set; }

        /// <summary>
        /// 说明。
        /// </summary>
        [TextFieldName("说明")]
        public string Description { get; set; }
    }

    /// <summary>
    /// 痛经的分析数据类。
    /// </summary>
    public class TongJingAnalysisData : GaoRongrongAnalysisDataBase
    {
        //public void SetAnswers(IEnumerable<SurveysAnswer> answers, DbContext dbContext)
        //{
        //    _Answers = answers.ToList();
        //    var tIds = _Answers.Select(c => c.TemplateId).ToArray();
        //    _Templates = dbContext.Set<SurveysAnswerTemplate>().Where(c => tIds.Contains(c.Id)).ToDictionary(c => c.Id);
        //    _Numbers = new HashSet<int>(_Templates.Values.Select(c => c.OrderNumber));
        //}

        private List<int> _AllNumbers;

        /// <summary>
        /// 包含派生编号的所有编号。
        /// </summary>
        public List<int> AllNumbers
        {
            get
            {
                lock (SyncLocker)
                    if (null == _AllNumbers)
                    {
                        var coll = Numbers.GetNumbers(TongJingZhengZhuangGuiLei.DefaultCollection, c => c.Numbers, c => c.Thresholds, c => c.Number);
                        _AllNumbers = coll.Union(Numbers).ToList();
                    }
                return _AllNumbers;
            }
        }

        public List<TongJingFenxing> _FenXing;

        /// <summary>
        /// 使用的分型数据。
        /// </summary>
        public List<TongJingFenxing> FenXing
        {
            get
            {
                lock (SyncLocker)
                    if (null == _FenXing)
                    {
                        var coll = AllNumbers.Where(c => GetTypeNumber(c) == "C");    //必有分型编号
                        var collmust = TongJingFenxing.DefaultCollection.Where(c => c.Numbers.Intersect(coll).Count() > 0); //必有分型
                        var coll1 = from tmp in TongJingFenxing.DefaultCollection
                                    let Matching = (float)AllNumbers.Intersect(tmp.Numbers).Count() / tmp.Numbers.Count   //匹配度
                                    where Matching >= tmp.ThresholdsOfLowest    //大于或等于最低匹配度
                                    select new { Matching, FenXing = tmp };
                        var coll2 = from tmp in coll1
                                    group tmp by tmp.FenXing.GroupNumber;   //按分组号分组
                        var coll3 = from tmp in coll2
                                    where tmp.Any(c => c.Matching >= c.FenXing.Thresholds)    //至少有一個可以最終選擇
                                    let AvgMatching = tmp.Average(c => c.Matching)    //每组平均匹配度
                                    orderby AvgMatching descending  //最高平均匹配度在最前
                                    select new { Group = tmp, AvgMatching };
                        var lst = coll3.ToList();
                        var maxAvgMatching = lst.FirstOrDefault()?.AvgMatching; //最大平均匹配度
                        IEnumerable<TongJingFenxing> result = Enumerable.Empty<TongJingFenxing>();
                        if (maxAvgMatching.HasValue)   //若有匹配
                        {
                            result = lst.TakeWhile(c => c.AvgMatching >= maxAvgMatching.Value - 0.1)   //获得入选组
                                .Select(c => c.Group.OrderByDescending(subc => subc.Matching).FirstOrDefault())   //入选组中匹配度最高的元素
                                .Where(c => c != null)
                                .Where(c => c.Matching >= c.FenXing.Thresholds) //大于或等于匹配度
                                .Select(c => c.FenXing);  //最终分型
                        }
                        _FenXing = result.Union(collmust).ToList();
                        if (!_FenXing.Any()) //若无分型匹配
                        {
                            var item = coll1.OrderByDescending(c => c.Matching).Where(c => c.Matching >= c.FenXing.ThresholdsOfLowest).FirstOrDefault();
                            if (null != item)
                                _FenXing.Add(item.FenXing);
                        }
                    }
                return _FenXing;
            }
        }

        private List<TongJingJingluoBianzheng> _JingluoBianzhengs;

        /// <summary>
        /// 使用的经络辩证。
        /// </summary>
        public List<TongJingJingluoBianzheng> JingluoBianzhengs
        {
            get
            {
                lock (SyncLocker)
                    if (null == _JingluoBianzhengs)
                    {
                        var list = TongJingJingluoBianzheng.DefaultCollection.Where(c => c.Numbers1.Intersect(AllNumbers).Any()).ToList();    //备选的行
                        if (list.Count == 0)   //若没有备选行
                            ; //不选，留着！
                        var coll = list
                            .GroupBy(c => c.GroupNumber)   //分组
                            .Select(c => c.OrderBy(subc => subc.Priority).FirstOrDefault()).Where(c => c != null);    //取最高优先级的一个
                        _JingluoBianzhengs = coll.ToList();
                    }
                return _JingluoBianzhengs;
            }
        }

        private List<TongJingMedicineCorrection> _MedicineCorrection;

        /// <summary>
        /// 药物加减项。包含增加和减少得药物。
        /// </summary>
        public List<TongJingMedicineCorrection> MedicineCorrection
        {
            get
            {
                lock (SyncLocker)
                    if (null == _MedicineCorrection)
                    {
                        var result = TongJingMedicineCorrection.DefaultCollection
                            .Where(c => (float)AllNumbers.Intersect(c.Numbers).Count() / c.Numbers.Count >= c.Thresholds);  //达到阈值要求
                        _MedicineCorrection = result.ToList();
                    }
                return _MedicineCorrection;
            }
        }

        /// <summary>
        /// 最终药物结果列表。
        /// </summary>
        public List<Tuple<string, decimal>> Results
        {
            get
            {
                var coll = FenXing.SelectMany(c => c.YaowuList).Union(JingluoBianzhengs.SelectMany(c => c.AllYao))  //辩证药物
                    .Union(MedicineCorrection.Where(c => c.TypeNumber == 1).SelectMany(c => c.Drugs)).GroupBy(c => c.Item1);    //增加的药物
                var tmp = coll.Select(c => Tuple.Create(c.Key, c.Max(subc => subc.Item2))).ToList();    //除最低用药外的药物
                var mins = MedicineCorrection.Where(c => c.TypeNumber == 2).SelectMany(c => c.Drugs).GroupBy(c => c.Item1)
                    .ToDictionary(c => c.Key, c => c.Min(sunc => sunc.Item2));   //最低剂量药物
                for (int i = tmp.Count - 1; i >= 0; i--)
                {
                    var item = tmp[i];
                    if (mins.TryGetValue(item.Item1, out decimal d))  //如果需要最小计量
                    {
                        tmp.RemoveAt(i);
                        tmp.Insert(i, Tuple.Create(item.Item1, d));
                    }
                }
                var result = new List<Tuple<string, decimal>>();
                //乱序
                Random rnd = new Random();
                while (tmp.Count > 0)
                {
                    var index = rnd.Next(tmp.Count);
                    result.Add(tmp[index]);
                    tmp.RemoveAt(index);
                }
                return result;
            }
        }

    }

    /// <summary>
    /// 痛经诊断算法类。
    /// </summary>
    [OwAdditional(SurveysTemplateIdName, SurveysTemplateIdString)]
    public class TongJingAlgorithm : GaoRonrongAlgorithmBase
    {
        /// <summary>
        /// 此算法处理的调查模板Id。
        /// </summary>
        public const string SurveysTemplateIdString = "45534E5F-359E-4261-93E9-3D3504843224";

        /// <summary>
        /// 初始化函数。
        /// </summary>
        /// <param name="context"></param>
        [OwAdditional(InitializationFuncName)]
        public static void Initialize(DbContext context)
        {
            var tmp = context.Set<SurveysTemplate>().Find(Guid.Parse(SurveysTemplateIdString));
            if (null != tmp)
                return;
            var path = System.Web.HttpContext.Current.Server.MapPath("~/content/痛经");
            List<TongJingSigns> signs;
            using (var tdb = new TextFileContext(path) { IgnoreQuotes = true, })
            {
                signs = tdb.GetList<TongJingSigns>("痛经病-症状表.txt");
            }
            //初始化调查模板项
            var surveysTemplate = new SurveysTemplate()
            {
                Id = Guid.Parse(SurveysTemplateIdString),
                Name = "痛经",
                UserState = "支持复诊0",
                Questions = new List<SurveysQuestionTemplate>(),
                Description= "痛经：凡在经期或经行前后,出现周期性小腹疼痛,或痛引腰骶,甚至剧痛晕厥者,称为“痛经”,亦称“经行腹痛”。",
            };
            context.Set<SurveysTemplate>().AddOrUpdate(surveysTemplate);
            //添加专病项
            InsomniaCasesItem caseItem = new InsomniaCasesItem()
            {
                Name = "痛经",
            };
            context.Set<InsomniaCasesItem>().AddOrUpdate(caseItem);
            //添加问题项
            var coll = signs.GroupBy(c => c.Question).Select(c =>
            {
                SurveysQuestionTemplate sqt = new SurveysQuestionTemplate()
                {
                    Kind = c.First().QuestionsKind,
                    OrderNum = c.First().Number,
                    QuestionTitle = c.Key,
                    UserState = string.Empty,
                };
                sqt.Answers = c.Select(subc =>
                {
                    SurveysAnswerTemplate sat = new SurveysAnswerTemplate()
                    {
                        AnswerTitle = subc.ZhengZhuang,
                        OrderNum = subc.Number,
                        UserState = $"编号{subc.Number}，类型号{subc.TypeString}1",
                    };
                    return sat;
                }).ToList();
                return sqt;
            });
            surveysTemplate.Questions.AddRange(coll);
            context.SaveChanges();
        }

        protected override SurveysConclusion GetResultCore(Surveys surveys, ApplicationDbContext db)
        {
            var sy = db.SurveysTemplates.Where(c => c.Name == "痛经").FirstOrDefault();
            if (null == sy)
                return null;
            var data = new TongJingAnalysisData();
            data.SetAnswers(surveys.SurveysAnswers, db);
            var result = new SurveysConclusion() { SurveysId = surveys.Id };
            result.Conclusion = string.Join(",", data.Results.Select(c => $"{c.Item1}{c.Item2}"));
            SetCnPrescriptiones(result, data.Results);

            if (string.IsNullOrWhiteSpace(result.Conclusion))
                result.Conclusion = "(您输入的症状暂无对应药方，请联系医生。)";
            return result;
        }
    }
}