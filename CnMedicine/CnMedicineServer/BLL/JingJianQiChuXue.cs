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
                        _AllNumbers = Numbers.GetNumbers(JingJianQiChuXueZhengZhuangGuiLei.DefaultCollection, c => c.Numbers, c => c.Thresholds, c => c.Number).ToList();
                        _AllNumbers.AddRange(Numbers);
                    }
                return _AllNumbers;
            }
        }

        List<Tuple<string, decimal>> _FenXingCnMedicine;

        /// <summary>
        /// 获取分型所用药物。
        /// </summary>
        public List<Tuple<string, decimal>> FenXingCnMedicine
        {
            get
            {
                lock (SyncLocker)
                    if (null == _FenXingCnMedicine)
                    {
                        var coll = AllNumbers.Where(c => GetTypeNumber(c) == "A" || string.Empty == GetTypeNumber(c))
                            .GetNumbers(JingJianQiChuXueFenXing.DefaultCollection, c => c.Numbers, c => c.Thresholds, c => c.CnDrugs).SelectMany(c => c);
                        var must = new HashSet<int>(AllNumbers.Where(c => GetTypeNumber(c) == "C"));    //必定选择的编号
                        var coll2 = JingJianQiChuXueFenXing.DefaultCollection.Where(c => must.Intersect(c.Numbers).Count() > 0).SelectMany(c => c.CnDrugs);
                        _FenXingCnMedicine = coll.Union(coll2).Distinct().ToList();
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
    public class JingJianQiChuXueAlgorithm : CnMedicineAlgorithmBase
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
                        UserState="",
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
            var result = new SurveysConclusion() { SurveysId=surveys.Id };
            result.GeneratedIdIfEmpty();
            var sy = db.SurveysTemplates.Where(c => c.Name == "经间期出血").FirstOrDefault();
            if (null == sy)
                return null;
            var data = new JingJianQiChuXueAnalysisData();
            data.SetAnswers(surveys.SurveysAnswers, db);
            result.Conclusion = string.Join(",", data.Results.Select(c => $"{c.Item1}{c.Item2}"));
            if (string.IsNullOrWhiteSpace(result.Conclusion))
                result.Conclusion = "(您输入的症状暂无对应药方，请联系医生。)";
            return result;
        }
    }
}