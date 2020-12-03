/*
 * 高荣荣医师妇科经类算法
 */

using CnMedicineServer.Models;
using OW;
using OW.Data.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;

namespace CnMedicineServer.Bll
{
    #region 崩漏
    /// <summary>
    /// 崩漏算法类
    /// </summary>
    [OwAdditional(SurveysTemplateIdName, SurveysTemplateIdString)]
    public class BengLouAlgorithm : GaoRonrongAlgorithmBase
    {
        /// <summary>
        /// 此算法处理的调查模板Id。
        /// </summary>
        public const string SurveysTemplateIdString = "8FD0F4B9-6B54-4A58-AF34-64BAB4B6F12E";

        /// <summary>
        /// 病种的中文名称。
        /// </summary>
        public const string CnName = "崩漏";

        /// <summary>
        /// 病种数据文件目录路径。
        /// </summary>
        public const string DataFilePath = "Content/崩漏";

        /// <summary>
        /// 初始化函数。
        /// </summary>
        /// <param name="context"></param>
        [OwAdditional(InitializationFuncName)]
        public static void Initialize(DbContext context)
        {
            InitializeCore(context, $"~/{DataFilePath}/{CnName}-症状表.txt", typeof(BengLouAlgorithm));
            var survId = Guid.Parse(SurveysTemplateIdString);
            //初始化模板数据
            var template = context.Set<SurveysTemplate>().Find(survId);
            template.Name = CnName;
            template.UserState = "支持复诊0";
            template.Description = null;
            //添加专病项
            InsomniaCasesItem caseItem = new InsomniaCasesItem()
            {
                Name = CnName,
            };
            context.Set<InsomniaCasesItem>().AddOrUpdate(caseItem);

            context.SaveChanges();

        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        public BengLouAlgorithm()
        {

        }

        public override IEnumerable<GrrBianZhengFenXingBase> BianZhengFenXings =>
            CnMedicineLogicBase.GetOrCreateAsync<BengLouFenXing>($"~/{DataFilePath}/{CnName}-辨证分型表.txt").Result;

        public override IEnumerable<CnDrugCorrectionBase> CnDrugCorrections =>
            CnMedicineLogicBase.GetOrCreateAsync<BengLouCnDrugCorrection>($"~/{DataFilePath}/{CnName}-加减表.txt").Result;

        public override IEnumerable<GrrJingLuoBianZhengBase> JingLuoBianZhengs =>
            CnMedicineLogicBase.GetOrCreateAsync<BengLouJingLuoBian>($"~/{DataFilePath}/{CnName}-经络辨证表.txt").Result;

        public override IEnumerable<GeneratedNumeber> GeneratedNumebers =>
            CnMedicineLogicBase.GetOrCreateAsync<BengLouGeneratedNumeber>($"~/{DataFilePath}/{CnName}-症状归类表.txt").Result;


        protected override SurveysConclusion GetResultCore(Surveys surveys, ApplicationDbContext db)
        {
            var result = new SurveysConclusion() { SurveysId = surveys.Id };
            result.GeneratedIdIfEmpty();
            var sy = db.SurveysTemplates.Where(c => c.Name == CnName).FirstOrDefault();
            if (null == sy)
                return null;
            SetSigns(surveys.SurveysAnswers, db);
            result.Conclusion = string.Join(",", Results.Select(c => $"{c.Item1}{c.Item2}"));
            SetCnPrescriptiones(result, Results);

            result.ThingPropertyItems.Add(new ThingPropertyItem()
            {
                Name = "诊断",
                Value = CnName,
            });
            var coll = surveys.SurveysAnswers.Select(c =>
            {
                var answerTemplate = db.Set<SurveysAnswerTemplate>().Find(c.TemplateId);
                if (null == answerTemplate)  //若没有直接绑定到答案项
                {
                    var questionTemplate = db.Set<SurveysQuestionTemplate>().Find(c.TemplateId);
                    return Tuple.Create(questionTemplate.QuestionTitle, c.Guts);
                }
                else //直接绑定了答案项
                {
                    return Tuple.Create(answerTemplate.SurveysQuestionTemplate.QuestionTitle, answerTemplate.AnswerTitle);
                }
            }).GroupBy(c => c.Item1).Select(c => Tuple.Create(c.Key, string.Join(",", c.Select(subc => subc.Item2))));
            result.ThingPropertyItems.Add(new ThingPropertyItem()
            {
                Name = "身体症状",
                Value = string.Join("; ", coll.Select(c => $"{c.Item1}:{c.Item2}")),
            });
            result.ThingPropertyItems.Add(new ThingPropertyItem()
            {
                Name = "分型",
                Value = FenXing.FirstOrDefault()?.ZhengDuan,
            });
            result.ThingPropertyItems.Add(new ThingPropertyItem()
            {
                Name = "病因分析",
                Value = FenXing.FirstOrDefault()?.FenXi,
            });
            result.ThingPropertyItems.Add(new ThingPropertyItem()
            {
                Name = "经络情况分析",
                Value = JingLuoBianZhengs.FirstOrDefault()?.BingYin,
            });
            if (string.IsNullOrWhiteSpace(result.Conclusion))
                result.Conclusion = "(患者身体健康，无需用药。)";
            return result;
        }
    }

    #endregion 崩漏

    #region 经行乳房胀痛
    /// <summary>
    /// 经行乳房胀痛症状类。
    /// </summary>
    public class JingXingRuFangZhangTongSigns
    {
        /*
            编号	问题	症状	问题类型
            1101	经前乳房胀痛	经前乳房胀痛	"选择,多重"
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
    /// 经行乳房胀痛数据类。
    /// </summary>
    public class JingXingRuFangZhangTongAnalysisData : GaoRongrongAnalysisDataBase
    {
        public JingXingRuFangZhangTongAnalysisData()
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
                        var coll = from tmp in JingXingRuFangZhangTongZhengZhuangGuiLei.DefaultCollection
                                   let fac = (float)tmp.Numbers.Intersect(adding).Count() / tmp.Numbers.Count
                                   where fac >= tmp.Thresholds
                                   select tmp.Number;
                        _AllNumbers = adding.Union(coll).ToList();
                    }
                return _AllNumbers;
            }
        }

        private Dictionary<JingXingRuFangZhangTongFenXing, float> _Fenxing2Matching;

        /// <summary>
        /// 键是分型数据，值匹配度。
        /// </summary>
        public Dictionary<JingXingRuFangZhangTongFenXing, float> Fenxing2Matching
        {
            get
            {
                lock (SyncLocker)
                    if (null == _Fenxing2Matching)
                    {
                        Dictionary<JingXingRuFangZhangTongFenXing, float> result = new Dictionary<JingXingRuFangZhangTongFenXing, float>();
                        foreach (var item in JingXingRuFangZhangTongFenXing.DefaultCollection)
                        {
                            var count = item.Numbers.Join(AllNumbers, c => c.Key, c => c, (kv, num) => kv).GroupBy(c => c.Key, (key, kvs) => kvs.Sum(c => c.Value)).Sum(); //AllNumbers中重複出現將有額外含義
                            result.Add(item, count / item.Numbers.Count);
                        }
                        _Fenxing2Matching = result;
                    }
                return _Fenxing2Matching;
            }
        }

        List<JingXingRuFangZhangTongFenXing> _FenXing;

        /// <summary>
        /// 获取分型。
        /// </summary>
        public List<JingXingRuFangZhangTongFenXing> FenXing
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
                        _FenXing = new List<JingXingRuFangZhangTongFenXing>();
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
                        var adds = (from tmp in JingXingRuFangZhangTongCorrection.DefaultCollection
                                    where tmp.TypeNumber == 1 && (float)AllNumbers.Intersect(tmp.Numbers).Count() / tmp.Numbers.Count >= tmp.Thresholds
                                    select tmp).SelectMany(c => c.CnDrugOfAdd); //增加项
                        var igns = (from tmp in JingXingRuFangZhangTongCorrection.DefaultCollection
                                    where tmp.TypeNumber == 1 && (float)AllNumbers.Intersect(tmp.Numbers).Count() / tmp.Numbers.Count >= tmp.Thresholds
                                    select tmp).SelectMany(c => c.CnDrugOfSub);   //忽略的项
                        var subc = (from tmp in JingXingRuFangZhangTongCorrection.DefaultCollection
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

        List<JingXingRuFangZhangTongJingLuoBianZheng> _JingLuo;

        /// <summary>
        /// 获取经络辩证。
        /// </summary>
        public List<JingXingRuFangZhangTongJingLuoBianZheng> JingLuo
        {
            get
            {
                lock (SyncLocker)
                    if (null == _JingLuo)
                    {
                        var coll = JingXingRuFangZhangTongJingLuoBianZheng.DefaultCollection
                            .Select(c => new { Value = c, Matching = (float)c.Numbers.Intersect(AllNumbers).Count() / c.Numbers.Count })   //计算中间数值
                            .Where(c => c.Matching >= c.Value.Thresholds)    //匹配项
                            .GroupBy(c => c.Value.GroupNumber, (key, seq) => seq.OrderByDescending(c => c.Matching).ThenByDescending(c => c.Value.Priority).First().Value);  //每组中匹配度最高的项
                        _JingLuo = coll.ToList();
                    }
                return _JingLuo;
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
                var coll = FenXingCnMedicine.Union(JingLuo.SelectMany(c => c.CnDrugs)).GroupBy(c => c.Item1);
                var tmp = coll.Select(c => Tuple.Create(c.Key, c.Max(subc => subc.Item2))).ToList();
                return tmp.RandomSequence();
            }
        }

    }

    /// <summary>
    /// 经行乳房胀痛。
    /// </summary>
    [OwAdditional(SurveysTemplateIdName, SurveysTemplateIdString)]
    public class JingXingRuFangZhangTongAlgorithm : GaoRonrongAlgorithmBase
    {
        /// <summary>
        /// 此算法处理的调查模板Id。
        /// </summary>
        public const string SurveysTemplateIdString = "D4817276-153D-4C90-AB33-23F70C20CC33";

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
            var path = System.Web.HttpContext.Current.Server.MapPath("~/content/经行乳房胀痛");
            List<JingXingRuFangZhangTongSigns> signs;
            using (var tdb = new TextFileContext(path) { IgnoreQuotes = true, })
            {
                signs = tdb.GetList<JingXingRuFangZhangTongSigns>("乳胀-症状表.txt");
            }
            //初始化调查模板项
            var surveysTemplate = new SurveysTemplate()
            {
                Id = Guid.Parse(SurveysTemplateIdString),
                Name = "经行乳房胀痛",
                UserState = "支持复诊0",
                Questions = new List<SurveysQuestionTemplate>(),
                Description = "经行乳房痛：每值经前或经期乳房作胀,甚至胀满疼痛,或乳头痒痛者,称“经行乳房痛”。包含乳腺增生、乳腺纤维瘤等乳腺疾病的伴发症状。",
            };
            context.Set<SurveysTemplate>().AddOrUpdate(surveysTemplate);
            //添加专病项
            InsomniaCasesItem caseItem = new InsomniaCasesItem()
            {
                Name = "经行乳房胀痛",
            };
            context.Set<InsomniaCasesItem>().AddOrUpdate(caseItem);
            //添加问题项
            var coll = signs.GroupBy(c => c.Question).Select(c =>
            {
                SurveysQuestionTemplate sqt = new SurveysQuestionTemplate()
                {
                    Kind = c.First().QuestionsKind,
                    IdNumber = c.First().Number,
                    QuestionTitle = c.Key,
                    UserState = "",
                };
                sqt.Answers = c.Select(subc =>
                {
                    SurveysAnswerTemplate sat = new SurveysAnswerTemplate()
                    {
                        AnswerTitle = subc.ZhengZhuang,
                        IdNumber = subc.Number,
                        UserState = $"编号{subc.Number}",
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
            var result = new SurveysConclusion() { SurveysId = surveys.Id };
            result.GeneratedIdIfEmpty();
            var sy = db.SurveysTemplates.Where(c => c.Name == "经行乳房胀痛").FirstOrDefault();
            if (null == sy)
                return null;
            var data = new JingXingRuFangZhangTongAnalysisData();
            data.SetAnswers(surveys.SurveysAnswers, db);
            result.Conclusion = string.Join(",", data.Results.Select(c => $"{c.Item1}{c.Item2}"));
            SetCnPrescriptiones(result, data.Results);

            if (string.IsNullOrWhiteSpace(result.Conclusion))
                result.Conclusion = "(患者身体健康，无需用药。)";
            return result;
        }
    }

    #endregion 经行乳房胀痛

    #region 经间期出血
    /// <summary>
    /// 经间期出血提取症状表的数据类。
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

    /// <summary>
    /// 经间期出血数据类。
    /// </summary>
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
                    IdNumber = c.First().Number,
                    QuestionTitle = c.Key,
                    UserState = "",
                };
                sqt.Answers = c.Select(subc =>
                {
                    SurveysAnswerTemplate sat = new SurveysAnswerTemplate()
                    {
                        AnswerTitle = subc.ZhengZhuang,
                        IdNumber = subc.Number,
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
                result.Conclusion = "(患者身体健康，无需用药。)";
            return result;
        }
    }

    #endregion 经间期出血

    #region 痛经

    ///// <summary>
    ///// 提取症状表的数据类。
    ///// </summary>
    //public class TongJingSigns
    //{
    //    /*
    //     编号	问题	    症状	类型号	问题类型	说明
    //     101    疼痛时间	月经期	B	    选择	类型号：A参与计算且阈值为0.7；B不参与计算；C参与计算且没有阈值
    //    */

    //    /// <summary>
    //    /// 构造函数。
    //    /// </summary>
    //    public TongJingSigns()
    //    {

    //    }

    //    /// <summary>
    //    /// 编号。
    //    /// </summary>
    //    [TextFieldName("编号")]
    //    public int Number { get; set; }

    //    /// <summary>
    //    /// 问题。
    //    /// </summary>
    //    [TextFieldName("问题")]
    //    public string Question { get; set; }

    //    /// <summary>
    //    /// 症状。
    //    /// </summary>
    //    [TextFieldName("症状")]
    //    public string ZhengZhuang { get; set; }

    //    /// <summary>
    //    /// 类型号。
    //    /// </summary>
    //    [TextFieldName("类型号")]
    //    public string TypeString { get; set; }

    //    /// <summary>
    //    /// 问题类型。
    //    /// </summary>
    //    [TextFieldName("问题类型")]
    //    public QuestionsKind QuestionsKind { get; set; }

    //    /// <summary>
    //    /// 说明。
    //    /// </summary>
    //    [TextFieldName("说明")]
    //    public string Description { get; set; }
    //}

    ///// <summary>
    ///// 痛经的分析数据类。
    ///// </summary>
    //public class TongJingAnalysisData : GaoRongrongAnalysisDataBase
    //{
    //    //public void SetAnswers(IEnumerable<SurveysAnswer> answers, DbContext dbContext)
    //    //{
    //    //    _Answers = answers.ToList();
    //    //    var tIds = _Answers.Select(c => c.TemplateId).ToArray();
    //    //    _Templates = dbContext.Set<SurveysAnswerTemplate>().Where(c => tIds.Contains(c.Id)).ToDictionary(c => c.Id);
    //    //    _Numbers = new HashSet<int>(_Templates.Values.Select(c => c.OrderNumber));
    //    //}

    //    private List<int> _AllNumbers;

    //    /// <summary>
    //    /// 包含派生编号的所有编号。
    //    /// </summary>
    //    public List<int> AllNumbers
    //    {
    //        get
    //        {
    //            lock (SyncLocker)
    //                if (null == _AllNumbers)
    //                {
    //                    var coll = Numbers.GetNumbers(TongJingZhengZhuangGuiLei.DefaultCollection, c => c.Numbers, c => c.Thresholds, c => c.Number);
    //                    _AllNumbers = coll.Union(Numbers).ToList();
    //                }
    //            return _AllNumbers;
    //        }
    //    }

    //    public List<TongJingFenxing> _FenXing;

    //    /// <summary>
    //    /// 使用的分型数据。
    //    /// </summary>
    //    public List<TongJingFenxing> FenXing
    //    {
    //        get
    //        {
    //            lock (SyncLocker)
    //                if (null == _FenXing)
    //                {
    //                    var coll = AllNumbers.Where(c => GetTypeNumber(c) == "C");    //必有分型编号
    //                    var collmust = TongJingFenxing.DefaultCollection.Where(c => c.Numbers.Intersect(coll).Count() > 0); //必有分型
    //                    var coll1 = from tmp in TongJingFenxing.DefaultCollection
    //                                let Matching = (float)AllNumbers.Intersect(tmp.Numbers).Count() / tmp.Numbers.Count   //匹配度
    //                                where Matching >= tmp.ThresholdsOfLowest    //大于或等于最低匹配度
    //                                select new { Matching, FenXing = tmp };
    //                    var coll2 = from tmp in coll1
    //                                group tmp by tmp.FenXing.GroupNumber;   //按分组号分组
    //                    var coll3 = from tmp in coll2
    //                                where tmp.Any(c => c.Matching >= c.FenXing.Thresholds)    //至少有一個可以最終選擇
    //                                let AvgMatching = tmp.Average(c => c.Matching)    //每组平均匹配度
    //                                orderby AvgMatching descending  //最高平均匹配度在最前
    //                                select new { Group = tmp, AvgMatching };
    //                    var lst = coll3.ToList();
    //                    var maxAvgMatching = lst.FirstOrDefault()?.AvgMatching; //最大平均匹配度
    //                    IEnumerable<TongJingFenxing> result = Enumerable.Empty<TongJingFenxing>();
    //                    if (maxAvgMatching.HasValue)   //若有匹配
    //                    {
    //                        result = lst.TakeWhile(c => c.AvgMatching >= maxAvgMatching.Value - 0.1)   //获得入选组
    //                            .Select(c => c.Group.OrderByDescending(subc => subc.Matching).FirstOrDefault())   //入选组中匹配度最高的元素
    //                            .Where(c => c != null)
    //                            .Where(c => c.Matching >= c.FenXing.Thresholds) //大于或等于匹配度
    //                            .Select(c => c.FenXing);  //最终分型
    //                    }
    //                    _FenXing = result.Union(collmust).ToList();
    //                    if (!_FenXing.Any()) //若无分型匹配
    //                    {
    //                        var item = coll1.OrderByDescending(c => c.Matching).Where(c => c.Matching >= c.FenXing.ThresholdsOfLowest).FirstOrDefault();
    //                        if (null != item)
    //                            _FenXing.Add(item.FenXing);
    //                    }
    //                }
    //            return _FenXing;
    //        }
    //    }

    //    private List<TongJingJingluoBianzheng> _JingluoBianzhengs;

    //    /// <summary>
    //    /// 使用的经络辩证。
    //    /// </summary>
    //    public List<TongJingJingluoBianzheng> JingluoBianzhengs
    //    {
    //        get
    //        {
    //            lock (SyncLocker)
    //                if (null == _JingluoBianzhengs)
    //                {
    //                    var list = TongJingJingluoBianzheng.DefaultCollection.Where(c => c.Numbers1.Intersect(AllNumbers).Any()).ToList();    //备选的行
    //                    if (list.Count == 0)   //若没有备选行
    //                        ; //不选，留着！
    //                    var coll = list
    //                        .GroupBy(c => c.GroupNumber)   //分组
    //                        .Select(c => c.OrderBy(subc => subc.Priority).FirstOrDefault()).Where(c => c != null);    //取最高优先级的一个
    //                    _JingluoBianzhengs = coll.ToList();
    //                }
    //            return _JingluoBianzhengs;
    //        }
    //    }

    //    private List<TongJingMedicineCorrection> _MedicineCorrection;

    //    /// <summary>
    //    /// 药物加减项。包含增加和减少得药物。
    //    /// </summary>
    //    public List<TongJingMedicineCorrection> MedicineCorrection
    //    {
    //        get
    //        {
    //            lock (SyncLocker)
    //                if (null == _MedicineCorrection)
    //                {
    //                    var result = TongJingMedicineCorrection.DefaultCollection
    //                        .Where(c => (float)AllNumbers.Intersect(c.Numbers).Count() / c.Numbers.Count >= c.Thresholds);  //达到阈值要求
    //                    _MedicineCorrection = result.ToList();
    //                }
    //            return _MedicineCorrection;
    //        }
    //    }

    //    /// <summary>
    //    /// 最终药物结果列表。
    //    /// </summary>
    //    public List<Tuple<string, decimal>> Results
    //    {
    //        get
    //        {
    //            var coll = FenXing.SelectMany(c => c.YaowuList).Union(JingluoBianzhengs.SelectMany(c => c.AllYao))  //辩证药物
    //                .Union(MedicineCorrection.Where(c => c.TypeNumber == 1).SelectMany(c => c.Drugs)).GroupBy(c => c.Item1);    //增加的药物
    //            var tmp = coll.Select(c => Tuple.Create(c.Key, c.Max(subc => subc.Item2))).ToList();    //除最低用药外的药物
    //            var mins = MedicineCorrection.Where(c => c.TypeNumber == 2).SelectMany(c => c.Drugs).GroupBy(c => c.Item1)
    //                .ToDictionary(c => c.Key, c => c.Min(sunc => sunc.Item2));   //最低剂量药物
    //            for (int i = tmp.Count - 1; i >= 0; i--)
    //            {
    //                var item = tmp[i];
    //                if (mins.TryGetValue(item.Item1, out decimal d))  //如果需要最小计量
    //                {
    //                    tmp.RemoveAt(i);
    //                    tmp.Insert(i, Tuple.Create(item.Item1, d));
    //                }
    //            }
    //            var result = new List<Tuple<string, decimal>>();
    //            //乱序
    //            Random rnd = new Random();
    //            while (tmp.Count > 0)
    //            {
    //                var index = rnd.Next(tmp.Count);
    //                result.Add(tmp[index]);
    //                tmp.RemoveAt(index);
    //            }
    //            return result;
    //        }
    //    }

    //}

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
        /// 病种的中文名称。
        /// </summary>
        public const string CnName = "痛经";

        /// <summary>
        /// 病种数据文件目录路径。
        /// </summary>
        public const string DataFilePath = "Content/痛经";

        /// <summary>
        /// 构造函数。
        /// </summary>
        public TongJingAlgorithm()
        {

        }

        public override IEnumerable<GrrBianZhengFenXingBase> BianZhengFenXings =>
            CnMedicineLogicBase.GetOrCreateAsync<TongJingFenXing>($"~/{DataFilePath}/{CnName}-分型表.txt").Result;

        public override IEnumerable<GrrJingLuoBianZhengBase> JingLuoBianZhengs =>
            CnMedicineLogicBase.GetOrCreateAsync<TongJingJingLuoBian>($"~/{DataFilePath}/{CnName}-经络辩证表.txt").Result;

        public override IEnumerable<CnDrugCorrectionBase> CnDrugCorrections =>
            CnMedicineLogicBase.GetOrCreateAsync<TongJingCnDrugCorrection>($"~/{DataFilePath}/{CnName}-药物加减表.txt").Result;

        public override IEnumerable<GeneratedNumeber> GeneratedNumebers =>
           CnMedicineLogicBase.GetOrCreateAsync<TongJingGeneratedNumeber>($"~/{DataFilePath}/{CnName}-症状归类表.txt").Result;

        /// <summary>
        /// 初始化函数。
        /// </summary>
        /// <param name="context"></param>
        [OwAdditional(InitializationFuncName)]
        public static void Initialize(DbContext context)
        {
            InitializeCore(context, $"~/{DataFilePath}/{CnName}病-症状表.txt", typeof(TongJingAlgorithm));
            var survId = Guid.Parse(SurveysTemplateIdString);
            //初始化模板数据
            var template = context.Set<SurveysTemplate>().Find(survId);
            template.Name = CnName;
            template.UserState = "支持复诊0";
            template.Description = "痛经：凡在经期或经行前后,出现周期性小腹疼痛,或痛引腰骶,甚至剧痛晕厥者,称为“痛经”,亦称“经行腹痛”。";
            //添加专病项
            InsomniaCasesItem caseItem = new InsomniaCasesItem()
            {
                Name = CnName,
            };
            context.Set<InsomniaCasesItem>().AddOrUpdate(caseItem);

            context.SaveChanges();
        }

        protected override SurveysConclusion GetResultCore(Surveys surveys, ApplicationDbContext db)
        {
            var result = new SurveysConclusion() { SurveysId = surveys.Id };
            result.GeneratedIdIfEmpty();
            var sy = db.SurveysTemplates.Where(c => c.Name == CnName).FirstOrDefault();
            if (null == sy)
                return null;
            SetSigns(surveys.SurveysAnswers, db);
            result.Conclusion = string.Join(",", Results.Select(c => $"{c.Item1}{c.Item2}"));
            SetCnPrescriptiones(result, Results);

            if (string.IsNullOrWhiteSpace(result.Conclusion))
                result.Conclusion = "(患者身体健康，无需用药。)";
            return result;
        }
    }
    #endregion

    #region 经期问题
    /// <summary>
    /// 月经提前算法类。
    /// </summary>
    [OwAdditional(SurveysTemplateIdName, SurveysTemplateIdString)]
    public class YueJingTiQianAlgorithm : GaoRonrongAlgorithmBase
    {
        /// <summary>
        /// 此算法处理的调查模板Id。
        /// </summary>
        public const string SurveysTemplateIdString = "83AE2E8A-3833-4644-8F90-41E8FD42BE36";

        /// <summary>
        /// 病种的中文名称。
        /// </summary>
        public const string CnName = "月经提前";

        /// <summary>
        /// 初始化函数。
        /// </summary>
        /// <param name="context"></param>
        [OwAdditional(InitializationFuncName)]
        public static void Initialize(DbContext context)
        {
            InitializeCore(context, $"~/content/{CnName}/{CnName}-症状表.txt", typeof(YueJingTiQianAlgorithm));
            var survId = Guid.Parse(SurveysTemplateIdString);
            //初始化模板数据
            var template = context.Set<SurveysTemplate>().Find(survId);
            template.Name = CnName;
            template.UserState = "支持复诊0";
            template.Description = "月经提前：月经提前5天以上连续两个月周期以上者，称月经先期。";
            //添加专病项
            InsomniaCasesItem caseItem = new InsomniaCasesItem()
            {
                Name = CnName,
            };
            context.Set<InsomniaCasesItem>().AddOrUpdate(caseItem);

            context.SaveChanges();

        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        public YueJingTiQianAlgorithm()
        {

        }

        public override IEnumerable<GeneratedNumeber> GeneratedNumebers =>
            CnMedicineLogicBase.GetOrCreateAsync<YueJingTiQianGeneratedNumeber>("~/Content/月经提前/月经提前-症状归类表.txt").Result;

        public override IEnumerable<GrrBianZhengFenXingBase> BianZhengFenXings =>
            CnMedicineLogicBase.GetOrCreateAsync<YueJingTiQianFenXing>("~/Content/月经提前/月经提前-辨证分型表.txt").Result;

        public override IEnumerable<GrrJingLuoBianZhengBase> JingLuoBianZhengs =>
            CnMedicineLogicBase.GetOrCreateAsync<YueJingTiQianJingLuoBian>("~/Content/月经提前/月经提前-经络辨证表.txt").Result;

        public override IEnumerable<CnDrugCorrectionBase> CnDrugCorrections =>
            CnMedicineLogicBase.GetOrCreateAsync<YueJingTiQianCnDrugCorrection>("~/Content/月经提前/月经提前-加减表.txt").Result;


        protected override SurveysConclusion GetResultCore(Surveys surveys, ApplicationDbContext db)
        {
            var result = new SurveysConclusion() { SurveysId = surveys.Id };
            result.GeneratedIdIfEmpty();
            var sy = db.SurveysTemplates.Where(c => c.Name == CnName).FirstOrDefault();
            if (null == sy)
                return null;
            SetSigns(surveys.SurveysAnswers, db);
            result.Conclusion = string.Join(",", Results.Select(c => $"{c.Item1}{c.Item2}"));
            SetCnPrescriptiones(result, Results);

            if (string.IsNullOrWhiteSpace(result.Conclusion))
                result.Conclusion = "(患者身体健康，无需用药。)";
            return result;
        }
    }

    /// <summary>
    /// 月经错后算法类。
    /// </summary>
    [OwAdditional(SurveysTemplateIdName, SurveysTemplateIdString)]
    public class YueJingCuoHouAlgorithm : GaoRonrongAlgorithmBase
    {
        /// <summary>
        /// 此算法处理的调查模板Id。
        /// </summary>
        public const string SurveysTemplateIdString = "7D20FE52-8541-4ABB-8A88-E483C238E629";

        /// <summary>
        /// 病种的中文名称。
        /// </summary>
        public const string CnName = "月经错后";

        /// <summary>
        /// 初始化函数。
        /// </summary>
        /// <param name="context"></param>
        [OwAdditional(InitializationFuncName)]
        public static void Initialize(DbContext context)
        {
            InitializeCore(context, $"~/content/{CnName}/{CnName}-症状表.txt", typeof(YueJingCuoHouAlgorithm));
            var survId = Guid.Parse(SurveysTemplateIdString);
            //初始化模板数据
            var template = context.Set<SurveysTemplate>().Find(survId);
            template.Name = CnName;
            template.UserState = "支持复诊0";
            template.Description = "月经错后：月经周期延后5天以上,甚至错后3-5个月一行,经期正常者,称为“月经后期”,亦称“经期错后”。本病相当于西医学的月经稀发。月经后期如伴经量过少,常可发展为闭经。";
            //添加专病项
            InsomniaCasesItem caseItem = new InsomniaCasesItem()
            {
                Name = CnName,
            };
            context.Set<InsomniaCasesItem>().AddOrUpdate(caseItem);

            context.SaveChanges();

        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        public YueJingCuoHouAlgorithm()
        {

        }

        public override IEnumerable<GeneratedNumeber> GeneratedNumebers =>
            CnMedicineLogicBase.GetOrCreateAsync<YueJingCuoHouGeneratedNumeber>($"~/Content/{CnName}/{CnName}-症状归类表.txt").Result;

        public override IEnumerable<GrrBianZhengFenXingBase> BianZhengFenXings =>
            CnMedicineLogicBase.GetOrCreateAsync<YueJingCuoHouFenXing>($"~/Content/{CnName}/{CnName}-辨证分型表.txt").Result;

        public override IEnumerable<GrrJingLuoBianZhengBase> JingLuoBianZhengs =>
            CnMedicineLogicBase.GetOrCreateAsync<YueJingCuoHouJingLuoBian>($"~/Content/{CnName}/{CnName}-经络辨证表.txt").Result;

        public override IEnumerable<CnDrugCorrectionBase> CnDrugCorrections =>
            CnMedicineLogicBase.GetOrCreateAsync<YueJingCuoHouCnDrugCorrection>($"~/Content/{CnName}/{CnName}-加减表.txt").Result;


        protected override SurveysConclusion GetResultCore(Surveys surveys, ApplicationDbContext db)
        {
            var result = new SurveysConclusion() { SurveysId = surveys.Id };
            result.GeneratedIdIfEmpty();
            var sy = db.SurveysTemplates.Where(c => c.Name == CnName).FirstOrDefault();
            if (null == sy)
                return null;
            SetSigns(surveys.SurveysAnswers, db);
            result.Conclusion = string.Join(",", Results.Select(c => $"{c.Item1}{c.Item2}"));
            SetCnPrescriptiones(result, Results);

            if (string.IsNullOrWhiteSpace(result.Conclusion))
                result.Conclusion = "(患者身体健康，无需用药。)";
            return result;
        }
    }

    /// <summary>
    /// 月经不定期算法类。
    /// </summary>
    [OwAdditional(SurveysTemplateIdName, SurveysTemplateIdString)]
    public class YueJingBuDingQiAlgorithm : GaoRonrongAlgorithmBase
    {
        /// <summary>
        /// 此算法处理的调查模板Id。
        /// </summary>
        public const string SurveysTemplateIdString = "05A328D5-CC12-4FFC-853E-692530832BBA";

        /// <summary>
        /// 病种的中文名称。
        /// </summary>
        public const string CnName = "月经不定期";

        /// <summary>
        /// 初始化函数。
        /// </summary>
        /// <param name="context"></param>
        [OwAdditional(InitializationFuncName)]
        public static void Initialize(DbContext context)
        {
            InitializeCore(context, $"~/content/{CnName}/{CnName}-症状表.txt", typeof(YueJingBuDingQiAlgorithm));
            var survId = Guid.Parse(SurveysTemplateIdString);
            //初始化模板数据
            var template = context.Set<SurveysTemplate>().Find(survId);
            template.Name = CnName;
            template.UserState = "支持复诊0";
            template.Description = "月经先后无定期：月经周期或前或后1-2周者,称为“月经先后无定期”";
            //添加专病项
            InsomniaCasesItem caseItem = new InsomniaCasesItem()
            {
                Name = CnName,
            };
            context.Set<InsomniaCasesItem>().AddOrUpdate(caseItem);

            context.SaveChanges();

        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        public YueJingBuDingQiAlgorithm()
        {

        }

        public override IEnumerable<GeneratedNumeber> GeneratedNumebers =>
            CnMedicineLogicBase.GetOrCreateAsync<YueJingBuDingQiGeneratedNumeber>($"~/Content/{CnName}/{CnName}-症状归类表.txt").Result;

        public override IEnumerable<GrrBianZhengFenXingBase> BianZhengFenXings =>
            CnMedicineLogicBase.GetOrCreateAsync<YueJingBuDingQiFenXing>($"~/Content/{CnName}/{CnName}-辨证分型表.txt").Result;

        public override IEnumerable<GrrJingLuoBianZhengBase> JingLuoBianZhengs =>
            CnMedicineLogicBase.GetOrCreateAsync<YueJingBuDingQiJingLuoBian>($"~/Content/{CnName}/{CnName}-经络辨证表.txt").Result;

        public override IEnumerable<CnDrugCorrectionBase> CnDrugCorrections =>
            CnMedicineLogicBase.GetOrCreateAsync<YueJingBuDingQiCnDrugCorrection>($"~/Content/{CnName}/{CnName}-加减表.txt").Result;


        protected override SurveysConclusion GetResultCore(Surveys surveys, ApplicationDbContext db)
        {
            var result = new SurveysConclusion() { SurveysId = surveys.Id };
            result.GeneratedIdIfEmpty();
            var sy = db.SurveysTemplates.Where(c => c.Name == CnName).FirstOrDefault();
            if (null == sy)
                return null;
            SetSigns(surveys.SurveysAnswers, db);
            result.Conclusion = string.Join(",", Results.Select(c => $"{c.Item1}{c.Item2}"));
            SetCnPrescriptiones(result, Results);

            if (string.IsNullOrWhiteSpace(result.Conclusion))
                result.Conclusion = "(患者身体健康，无需用药。)";
            return result;
        }
    }

    /// <summary>
    /// 经期过长算法类。
    /// </summary>
    [OwAdditional(SurveysTemplateIdName, SurveysTemplateIdString)]
    public class JingQiYanChangAlgorithm : GaoRonrongAlgorithmBase
    {
        /// <summary>
        /// 此算法处理的调查模板Id。
        /// </summary>
        public const string SurveysTemplateIdString = "F4610E21-B343-4C36-BEC5-2CD09390A8B5";

        /// <summary>
        /// 病种的中文名称。
        /// </summary>
        public const string CnName = "经期延长";

        /// <summary>
        /// 病种数据文件目录路径。
        /// </summary>
        public const string DataFilePath = "Content/经期延长";

        /// <summary>
        /// 初始化函数。
        /// </summary>
        /// <param name="context"></param>
        [OwAdditional(InitializationFuncName)]
        public static void Initialize(DbContext context)
        {
            InitializeCore(context, $"~/{DataFilePath}/{CnName}-症状表.txt", typeof(JingQiYanChangAlgorithm));
            var survId = Guid.Parse(SurveysTemplateIdString);
            //初始化模板数据
            var template = context.Set<SurveysTemplate>().Find(survId);
            template.Name = CnName;
            template.UserState = "支持复诊0";
            template.Description = "";
            //添加专病项
            InsomniaCasesItem caseItem = new InsomniaCasesItem()
            {
                Name = CnName,
            };
            context.Set<InsomniaCasesItem>().AddOrUpdate(caseItem);

            context.SaveChanges();

        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        public JingQiYanChangAlgorithm()
        {

        }

        public override IEnumerable<GeneratedNumeber> GeneratedNumebers =>
            CnMedicineLogicBase.GetOrCreateAsync<YueJingYanChangGeneratedNumeber>($"~/{DataFilePath}/{CnName}-症状归类表.txt").Result;

        public override IEnumerable<GrrBianZhengFenXingBase> BianZhengFenXings =>
            CnMedicineLogicBase.GetOrCreateAsync<YueJingYanChangFenXing>($"~/{DataFilePath}/{CnName}-辨证分型表.txt").Result;

        public override IEnumerable<GrrJingLuoBianZhengBase> JingLuoBianZhengs =>
            CnMedicineLogicBase.GetOrCreateAsync<YueJingYanChangJingLuoBian>($"~/{DataFilePath}/{CnName}-经络辨证表.txt").Result;

        public override IEnumerable<CnDrugCorrectionBase> CnDrugCorrections =>
            CnMedicineLogicBase.GetOrCreateAsync<YueJingYanChangCnDrugCorrection>($"~/{DataFilePath}/{CnName}-加减表.txt").Result;


        protected override SurveysConclusion GetResultCore(Surveys surveys, ApplicationDbContext db)
        {
            var result = new SurveysConclusion() { SurveysId = surveys.Id };
            result.GeneratedIdIfEmpty();
            var sy = db.SurveysTemplates.Where(c => c.Name == CnName).FirstOrDefault();
            if (null == sy)
                return null;
            SetSigns(surveys.SurveysAnswers, db);
            result.Conclusion = string.Join(",", Results.Select(c => $"{c.Item1}{c.Item2}"));
            SetCnPrescriptiones(result, Results);

            if (string.IsNullOrWhiteSpace(result.Conclusion))
                result.Conclusion = "(患者身体健康，无需用药。)";
            return result;
        }
    }

    #endregion 经期问题

    /// <summary>
    /// 月经量过多算法类。
    /// </summary>
    [OwAdditional(SurveysTemplateIdName, SurveysTemplateIdString)]
    public class YueJingGuoDuoAlgorithm : GaoRonrongAlgorithmBase
    {
        /// <summary>
        /// 此算法处理的调查模板Id。
        /// </summary>
        public const string SurveysTemplateIdString = "B99FEC3C-FB3E-41E9-83EE-B39D1E5AF736";

        /// <summary>
        /// 病种的中文名称。
        /// </summary>
        public const string CnName = "月经过多";

        /// <summary>
        /// 病种数据文件目录路径。
        /// </summary>
        public const string DataFilePath = "Content/月经量过多";

        /// <summary>
        /// 初始化函数。
        /// </summary>
        /// <param name="context"></param>
        [OwAdditional(InitializationFuncName)]
        public static void Initialize(DbContext context)
        {
            InitializeCore(context, $"~/{DataFilePath}/{CnName}-症状表.txt", typeof(YueJingGuoDuoAlgorithm));
            var survId = Guid.Parse(SurveysTemplateIdString);
            //初始化模板数据
            var template = context.Set<SurveysTemplate>().Find(survId);
            template.Name = CnName;
            template.UserState = "支持复诊0";
            template.Description = "月经周期正常,经量明显多于既往者,称为“月经过多”,本病相当于西医学排卵型功能失调性子宫出血病引起的月经过多,或子宫肌瘤、盆腔炎症、子宫内膜异位症等疾病引起的月经过多。宫内节育器引起的月经过多,可按本病治疗。 ";
            //添加专病项
            InsomniaCasesItem caseItem = new InsomniaCasesItem()
            {
                Name = CnName,
            };
            context.Set<InsomniaCasesItem>().AddOrUpdate(caseItem);

            context.SaveChanges();

        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        public YueJingGuoDuoAlgorithm()
        {

        }

        public override IEnumerable<GeneratedNumeber> GeneratedNumebers =>
            CnMedicineLogicBase.GetOrCreateAsync<YueJingLiangGuoDuoGeneratedNumeber>($"~/{DataFilePath}/{CnName}-症状归类表.txt").Result;

        public override IEnumerable<GrrBianZhengFenXingBase> BianZhengFenXings =>
            CnMedicineLogicBase.GetOrCreateAsync<YueJingLiangGuoDuoFenXing>($"~/{DataFilePath}/{CnName}-辨证分型表.txt").Result;

        public override IEnumerable<GrrJingLuoBianZhengBase> JingLuoBianZhengs =>
            CnMedicineLogicBase.GetOrCreateAsync<YueJingLiangGuoDuoJingLuoBian>($"~/{DataFilePath}/{CnName}-经络辨证表.txt").Result;

        public override IEnumerable<CnDrugCorrectionBase> CnDrugCorrections =>
            CnMedicineLogicBase.GetOrCreateAsync<YueJingLiangGuoDuoCnDrugCorrection>($"~/{DataFilePath}/{CnName}-加减表.txt").Result;

        protected override SurveysConclusion GetResultCore(Surveys surveys, ApplicationDbContext db)
        {
            var result = new SurveysConclusion() { SurveysId = surveys.Id };
            result.GeneratedIdIfEmpty();
            var sy = db.SurveysTemplates.Where(c => c.Name == CnName).FirstOrDefault();
            if (null == sy)
                return null;
            SetSigns(surveys.SurveysAnswers, db);
            result.Conclusion = string.Join(",", Results.Select(c => $"{c.Item1}{c.Item2}"));
            SetCnPrescriptiones(result, Results);

            if (string.IsNullOrWhiteSpace(result.Conclusion))
                result.Conclusion = "(患者身体健康，无需用药。)";
            return result;
        }
    }

    /// <summary>
    /// 月经量过少算法类。
    /// </summary>
    [OwAdditional(SurveysTemplateIdName, SurveysTemplateIdString)]
    public class YueJingGuoShaoAlgorithm : GaoRonrongAlgorithmBase
    {
        /// <summary>
        /// 此算法处理的调查模板Id。
        /// </summary>
        public const string SurveysTemplateIdString = "4E61771B-5FDB-420E-95C5-EFF7AB475E21";

        /// <summary>
        /// 病种的中文名称。
        /// </summary>
        public const string CnName = "月经过少";

        /// <summary>
        /// 病种数据文件目录路径。
        /// </summary>
        public const string DataFilePath = "Content/月经量过少";

        /// <summary>
        /// 初始化函数。
        /// </summary>
        /// <param name="context"></param>
        [OwAdditional(InitializationFuncName)]
        public static void Initialize(DbContext context)
        {
            InitializeCore(context, $"~/{DataFilePath}/{CnName}-症状表.txt", typeof(YueJingGuoShaoAlgorithm));
            var survId = Guid.Parse(SurveysTemplateIdString);
            //初始化模板数据
            var template = context.Set<SurveysTemplate>().Find(survId);
            template.Name = CnName;
            template.UserState = "支持复诊0";
            template.Description = "月经周期正常,经量明显少于既往,经期不足2天,甚或点滴即净者,称“月经过少”，本病相当于西医学性腺功能低下、子宫内膜结核、炎症或刮宫过深等引起的月经过少。 月经过少伴月经后期者,可发展为闭经。";
            //添加专病项
            InsomniaCasesItem caseItem = new InsomniaCasesItem()
            {
                Name = CnName,
            };
            context.Set<InsomniaCasesItem>().AddOrUpdate(caseItem);

            context.SaveChanges();

        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        public YueJingGuoShaoAlgorithm()
        {

        }

        public override IEnumerable<GeneratedNumeber> GeneratedNumebers =>
            CnMedicineLogicBase.GetOrCreateAsync<YueJingLiangGuoShaoGeneratedNumeber>($"~/{DataFilePath}/{CnName}-症状归类表.txt").Result;

        public override IEnumerable<GrrBianZhengFenXingBase> BianZhengFenXings =>
            CnMedicineLogicBase.GetOrCreateAsync<YueJingLiangGuoShaoFenXing>($"~/{DataFilePath}/{CnName}-辨证分型表.txt").Result;

        public override IEnumerable<GrrJingLuoBianZhengBase> JingLuoBianZhengs =>
            CnMedicineLogicBase.GetOrCreateAsync<YueJingLiangGuoShaoJingLuoBian>($"~/{DataFilePath}/{CnName}-经络辨证表.txt").Result;

        public override IEnumerable<CnDrugCorrectionBase> CnDrugCorrections =>
            CnMedicineLogicBase.GetOrCreateAsync<YueJingLiangGuoShaoCnDrugCorrection>($"~/{DataFilePath}/{CnName}-加减表.txt").Result;


        protected override SurveysConclusion GetResultCore(Surveys surveys, ApplicationDbContext db)
        {
            var result = new SurveysConclusion() { SurveysId = surveys.Id };
            result.GeneratedIdIfEmpty();
            var sy = db.SurveysTemplates.Where(c => c.Name == CnName).FirstOrDefault();
            if (null == sy)
                return null;
            SetSigns(surveys.SurveysAnswers, db);
            result.Conclusion = string.Join(",", Results.Select(c => $"{c.Item1}{c.Item2}"));
            SetCnPrescriptiones(result, Results);

            if (string.IsNullOrWhiteSpace(result.Conclusion))
                result.Conclusion = "(患者身体健康，无需用药。)";
            return result;
        }
    }

    /// <summary>
    /// 经前综合征算法类。
    /// </summary>
    [OwAdditional(SurveysTemplateIdName, SurveysTemplateIdString)]
    public class JingQianZongHeZhengAlgorithm : GaoRonrongAlgorithmBase
    {
        /// <summary>
        /// 此算法处理的调查模板Id。
        /// </summary>
        public const string SurveysTemplateIdString = "53716225-0BA6-4DFE-B538-AD8EBF78B606";

        /// <summary>
        /// 病种的中文名称。
        /// </summary>
        public const string CnName = "经前综合征";

        /// <summary>
        /// 病种数据文件目录路径。
        /// </summary>
        public const string DataFilePath = "Content/经前综合征";

        /// <summary>
        /// 初始化函数。
        /// </summary>
        /// <param name="context"></param>
        [OwAdditional(InitializationFuncName)]
        public static void Initialize(DbContext context)
        {
            InitializeCore(context, $"~/{DataFilePath}/{CnName}-症状表.txt", typeof(JingQianZongHeZhengAlgorithm));
            var survId = Guid.Parse(SurveysTemplateIdString);
            //初始化模板数据
            var template = context.Set<SurveysTemplate>().Find(survId);
            template.Name = CnName;
            template.UserState = "支持复诊0";
            template.Description = null;
            //添加专病项
            InsomniaCasesItem caseItem = new InsomniaCasesItem()
            {
                Name = CnName,
            };
            context.Set<InsomniaCasesItem>().AddOrUpdate(caseItem);

            context.SaveChanges();

        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        public JingQianZongHeZhengAlgorithm()
        {

        }

        public override IEnumerable<GeneratedNumeber> GeneratedNumebers =>
            CnMedicineLogicBase.GetOrCreateAsync<JingQianZongHeZhengGeneratedNumeber>($"~/{DataFilePath}/{CnName}-症状归类表.txt").Result;

        public override IEnumerable<GrrBianZhengFenXingBase> BianZhengFenXings =>
            CnMedicineLogicBase.GetOrCreateAsync<JingQianZongHeZhengFenXing>($"~/{DataFilePath}/{CnName}-辨证分型表.txt").Result;

        public override IEnumerable<GrrJingLuoBianZhengBase> JingLuoBianZhengs =>
            CnMedicineLogicBase.GetOrCreateAsync<JingQianZongHeZhengJingLuoBian>($"~/{DataFilePath}/{CnName}-经络辨证表.txt").Result;

        public override IEnumerable<CnDrugCorrectionBase> CnDrugCorrections =>
            CnMedicineLogicBase.GetOrCreateAsync<JingQianZongHeZhengCnDrugCorrection>($"~/{DataFilePath}/{CnName}-加减表.txt").Result;


        protected override SurveysConclusion GetResultCore(Surveys surveys, ApplicationDbContext db)
        {
            var result = new SurveysConclusion() { SurveysId = surveys.Id };
            result.GeneratedIdIfEmpty();
            var sy = db.SurveysTemplates.Where(c => c.Name == CnName).FirstOrDefault();
            if (null == sy)
                return null;
            SetSigns(surveys.SurveysAnswers, db);
            result.Conclusion = string.Join(",", Results.Select(c => $"{c.Item1}{c.Item2}"));
            SetCnPrescriptiones(result, Results);

            if (string.IsNullOrWhiteSpace(result.Conclusion))
                result.Conclusion = "(患者身体健康，无需用药。)";
            return result;
        }
    }
}