
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
        //    _Numbers = new HashSet<int>(_Templates.Values.Select(c => c.OrderNum));
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
                        _FenXing = AllNumbers.GetNumbers(TongJingFenxing.DefaultCollection, c => c.Numbers, c => c.Fact, c => c).Union(collmust).ToList();
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
        /// 药物加减项。
        /// </summary>
        public List<TongJingMedicineCorrection> MedicineCorrection
        {
            get
            {
                lock (SyncLocker)
                    if (null == _MedicineCorrection)
                    {
                        var coll = TongJingMedicineCorrection.DefaultCollection.Join(FenXing, c => c.ZhengXing, c => c.Fenxing, (c1, c2) => c1);  //备选项
                        var result = coll.Where(c => (float)AllNumbers.Intersect(c.Numbers).Count() / c.Numbers.Count >= c.Thresholds);
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
                var coll = FenXing.SelectMany(c => c.YaowuList).Union(JingluoBianzhengs.SelectMany(c => c.AllYao)).Union(MedicineCorrection.SelectMany(c => c.Drugs)).GroupBy(c => c.Item1);
                var tmp = coll.Select(c => Tuple.Create(c.Key, c.Max(subc => subc.Item2))).ToList();
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
    public class TongJingAlgorithm : CnMedicineAlgorithmBase
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
            if (string.IsNullOrWhiteSpace(result.Conclusion))
                result.Conclusion = "(您输入的症状暂无对应药方，请联系医生。)";
            return result;
        }
    }
}