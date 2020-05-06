using CnMedicineServer.Models;
using OW;
using OW.Data.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CnMedicineServer.Bll
{
    /// <summary>
    /// 提取症状表的数据类。
    /// </summary>
    public class JingJianQiChuXueSigns
    {
        /*
         编号	问题	    症状	类型号	问题类型	说明
         101    疼痛时间	月经期	B	    选择	类型号：A参与计算且阈值为0.7；B不参与计算；C参与计算且没有阈值
        */

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

    public class JingJianQiChuXueAnalysisData : GaoRongrongAnalysisDataBase
    {
        public JingJianQiChuXueAnalysisData()
        {

        }

        private List<int> _AllNumbers;

        /// <summary>
        /// 添加了派生编号后的所有编号。
        /// </summary>
        public List<int> AllNumbers
        {
            get
            {
                lock (SyncLocker)
                    if (null == _AllNumbers)
                    {
                        var adding = Numbers.Union(AddingNumbers).ToArray();

                        var coll = from tmp in JingJianQiChuXueZhengZhuangGuiLei.DefaultCollection
                                   let fac = (float)tmp.Numbers.Intersect(adding).Count() / tmp.Numbers.Count
                                   where fac >= tmp.Thresholds
                                   select tmp.Number;
                        _AllNumbers = Numbers.Union(coll).ToList();
                    }
                return _AllNumbers;
            }
        }

        private Dictionary<JingJianQiChuXueFenXing, float> _Fenxing2Matching;

        /// <summary>
        /// 键是分型数据，值匹配度。
        /// </summary>
        public Dictionary<JingJianQiChuXueFenXing, float> Fenxing2Matching
        {
            get
            {
                lock (SyncLocker)
                    if (null == _Fenxing2Matching)
                    {
                        Dictionary<JingJianQiChuXueFenXing, float> result = new Dictionary<JingJianQiChuXueFenXing, float>();
                        foreach (var item in JingJianQiChuXueFenXing.DefaultCollection)
                        {
                            var count = item.Numbers.Join(AllNumbers, c => c.Key, c => c, (kv, num) => kv).GroupBy(c => c.Key).Sum(c => c.Sum(subc => subc.Value)); //AllNumbers中重複出現將有額外含義
                            result.Add(item, count / item.Numbers.Count);
                        }
                        _Fenxing2Matching = result;
                    }
                return _Fenxing2Matching;
            }
        }

        List<JingJianQiChuXueFenXing> _FenXing;

        /// <summary>
        /// 获取分型。
        /// </summary>
        public List<JingJianQiChuXueFenXing> FenXing
        {
            get
            {
                lock (SyncLocker)
                    if (null == _FenXing)
                    {
                        var collAlternative = (from tmp in Fenxing2Matching    //备选
                                               where tmp.Key.ThresholdsOfLowest <= tmp.Value //符合最低匹配度要求的才入选
                                               select tmp).ToArray();
                        var avgDic = (from tmp in collAlternative
                                      group tmp by tmp.Key.GroupNumber into g
                                      select new { g.Key, Avg = g.Average(c => c.Value) });    //求平均匹配度
                        var coll = from tmp in collAlternative
                                   join avg in avgDic on tmp.Key.GroupNumber equals avg.Key
                                   orderby avg.Avg descending, tmp.Value descending //平均匹配度 匹配度降序
                                   select new { tmp.Key, Matching = tmp.Value, AvgMating = avg.Avg };
                        var result = coll.ToArray();
                        var fenxing = coll.FirstOrDefault(c => c.Matching >= c.Key.Thresholds); //获取最佳匹配
                        if (null == fenxing)   //如果没有找到
                            fenxing = coll.FirstOrDefault(c => c.Matching >= c.Key.ThresholdsOfLowest); //降格处理
                        _FenXing = new List<JingJianQiChuXueFenXing>();
                        if (null != fenxing)
                            _FenXing.Add(fenxing.Key);
                        AddingNumbers.AddRange(_FenXing.Select(c => c.Number));    //辩证的编号加入编号集合
                        _AllNumbers = null;
                    }
                return _FenXing;
            }
        }

        List<Tuple<string, decimal>> _FenXingCnMedicine;

        /// <summary>
        /// 获取分型药物加减后的结果。
        /// </summary>
        public List<Tuple<string, decimal>> FenXingCnMedicine
        {
            get
            {
                lock (SyncLocker)
                    if (null == _FenXingCnMedicine)
                    {
                        var adds = (from tmp in JingJianQiChuXueFenXingCorrection.DefaultCollection
                                    where tmp.TypeNumber == 1 && (float)AllNumbers.Intersect(tmp.Numbers).Count() / tmp.Numbers.Count >= tmp.Thresholds
                                    select tmp).SelectMany(c => c.CnDrugOfAdd); //增加项
                        var igns = (from tmp in JingJianQiChuXueFenXingCorrection.DefaultCollection
                                    where tmp.TypeNumber == 1 && (float)AllNumbers.Intersect(tmp.Numbers).Count() / tmp.Numbers.Count >= tmp.Thresholds
                                    select tmp).SelectMany(c => c.CnDrugOfSub);   //忽略的项
                        var subc = (from tmp in JingJianQiChuXueFenXingCorrection.DefaultCollection
                                    where tmp.TypeNumber == 2 && (float)AllNumbers.Intersect(tmp.Numbers).Count() / tmp.Numbers.Count >= tmp.Thresholds
                                    select tmp).SelectMany(c => c.CnDrugOfAdd);   //减少项
                        var coll1 = FenXing.SelectMany(c => c.CnDrugs).Union(adds) //增加项
                            .Where(c => !igns.Contains(c.Item1));   //忽略药物
                        var result = coll1.GroupJoin(subc, c => c.Item1, c => c.Item1, (tp, c) => c.Any() ? c.First() : tp);
                        _FenXingCnMedicine = result.ToList();
                    }
                return _FenXingCnMedicine;
            }
        }

        List<Tuple<string, decimal>> _JingLuoCnMedicine;

        /// <summary>
        /// 获取经络辩证的药物。
        /// </summary>
        public List<Tuple<string, decimal>> JingLuoCnMedicine
        {
            get
            {
                lock (SyncLocker)
                    if (null == _JingLuoCnMedicine)
                    {
                        var list = JingJianQiChuXueJingLuoBianZheng.DefaultCollection.Where(c => c.Numbers1.Intersect(AllNumbers).Count() > 0).ToList();    //包含的行
                        if (list.Count == 0)   //若没有备选行
                            list = JingJianQiChuXueJingLuoBianZheng.DefaultCollection.ToList(); //选取所有行
                        var result = list.Tops(c => (float)c.Numbers2.Intersect(AllNumbers).Count() / c.Numbers2.Count, (c1, c2) => c1).OrderBy(c => c.Priority).FirstOrDefault();
                        _JingLuoCnMedicine = new List<Tuple<string, decimal>>();
                        if (null != result)
                            _JingLuoCnMedicine.AddRange(result.CnDrugs);
                    }
                return _JingLuoCnMedicine;
            }
        }

        List<Tuple<string, decimal>> _ResultCnMedicine;

        /// <summary>
        /// 最终药物结果列表。
        /// </summary>
        public List<Tuple<string, decimal>> Results
        {
            get
            {
                var coll = FenXingCnMedicine.Union(JingLuoCnMedicine).GroupBy(c => c.Item1);
                var tmp = coll.Select(c => Tuple.Create(c.Key, c.Max(subc => subc.Item2))).ToList();
                return tmp.RandomSequence();
            }
        }

    }

    /// <summary>
    /// 经间期出血。
    /// </summary>
    [OwAdditional(SurveysTemplateIdName, SurveysTemplateIdString)]
    public class JingJianQiChuXueAlgorithm : GaoRonrongAlgorithmBase
    {
        /// <summary>
        /// 此算法处理的调查模板Id。
        /// </summary>
        public const string SurveysTemplateIdString = "0C199BBC-009C-46EC-B3AC-CB4796F7CB2A";

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
            var path = System.Web.HttpContext.Current.Server.MapPath("~/content/经间期出血");
            List<JingJianQiChuXueSigns> signs;
            using (var tdb = new TextFileContext(path) { IgnoreQuotes = true, })
            {
                signs = tdb.GetList<JingJianQiChuXueSigns>("症状表.txt");
            }
            //初始化调查模板项
            var surveysTemplate = new SurveysTemplate()
            {
                Id = Guid.Parse(SurveysTemplateIdString),
                Name = "经间期出血",
                UserState = "支持复诊0",
                Questions = new List<SurveysQuestionTemplate>(),
                Description = "经间期出血指：凡在两次月经之间，排卵期时，有周期性出血者，称为经间期出血。",
            };
            context.Set<SurveysTemplate>().AddOrUpdate(surveysTemplate);
            //添加专病项
            InsomniaCasesItem caseItem = new InsomniaCasesItem()
            {
                Name = "经间期出血",
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
                        UserState = "",
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

        /// <summary>
        /// 构造函数。
        /// </summary>
        public JingJianQiChuXueAlgorithm()
        {

        }

        protected override SurveysConclusion GetResultCore(Surveys surveys, ApplicationDbContext db)
        {
            var result = new SurveysConclusion() { SurveysId = surveys.Id };
            result.GeneratedIdIfEmpty();
            var sy = db.SurveysTemplates.Where(c => c.Name == "经间期出血").FirstOrDefault();
            if (null == sy)
                return null;
            var data = new JingJianQiChuXueAnalysisData();
            data.SetAnswers(surveys.SurveysAnswers, db);
            result.Conclusion = string.Join(",", data.Results.Select(c => $"{c.Item1}{c.Item2}"));
            SetCnPrescriptiones(result, data.Results);

            if (string.IsNullOrWhiteSpace(result.Conclusion))
                result.Conclusion = "(您输入的症状暂无对应药方，请联系医生。)";
            return result;
        }
    }
}