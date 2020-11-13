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
    #region 痛经

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
                Description = "痛经：凡在经期或经行前后,出现周期性小腹疼痛,或痛引腰骶,甚至剧痛晕厥者,称为“痛经”,亦称“经行腹痛”。",
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
                    IdNumber = c.First().Number,
                    QuestionTitle = c.Key,
                    UserState = string.Empty,
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