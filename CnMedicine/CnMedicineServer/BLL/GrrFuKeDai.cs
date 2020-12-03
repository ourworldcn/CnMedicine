/*
 * 高荣荣医师妇科带类疾病算法
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
    /// <summary>
    /// 非炎症带下病算法类
    /// </summary>
    [OwAdditional(SurveysTemplateIdName, SurveysTemplateIdString)]
    public class FeiYanDaiXiaAlgorithm : GaoRonrongAlgorithmBase
    {
        /// <summary>
        /// 此算法处理的调查模板Id。
        /// </summary>
        public const string SurveysTemplateIdString = "BC34ABB9-E7DD-40DA-969C-E1EF77C69250";

        /// <summary>
        /// 病种的中文名称。
        /// </summary>
        public const string CnName = "非炎症带下病";

        /// <summary>
        /// 病种数据文件目录路径。
        /// </summary>
        public const string DataFilePath = "Content/非炎症带下病";

        /// <summary>
        /// 初始化函数。
        /// </summary>
        /// <param name="context"></param>
        [OwAdditional(InitializationFuncName)]
        public static void Initialize(DbContext context)
        {
            InitializeCore(context, $"~/{DataFilePath}/{CnName}-症状表.txt", typeof(FeiYanDaiXiaAlgorithm));
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
        public FeiYanDaiXiaAlgorithm()
        {

        }

        public override IEnumerable<GeneratedNumeber> GeneratedNumebers =>
            CnMedicineLogicBase.GetOrCreateAsync<FeiYanDaiXiaGeneratedNumeber>($"~/{DataFilePath}/{CnName}-症状归类表.txt").Result;

        public override IEnumerable<GrrBianZhengFenXingBase> BianZhengFenXings =>
            CnMedicineLogicBase.GetOrCreateAsync<FeiYanDaiXiaFenXing>($"~/{DataFilePath}/{CnName}-辨证分型表.txt").Result;

        public override IEnumerable<GrrJingLuoBianZhengBase> JingLuoBianZhengs =>
            CnMedicineLogicBase.GetOrCreateAsync<FeiYanDaiXiaJingLuoBian>($"~/{DataFilePath}/{CnName}-经络辨证表.txt").Result;

        public override IEnumerable<CnDrugCorrectionBase> CnDrugCorrections =>
            CnMedicineLogicBase.GetOrCreateAsync<FeiYanDaiXiaCnDrugCorrection>($"~/{DataFilePath}/{CnName}-加减表.txt").Result;


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
    /// 炎症带下病算法类。
    /// </summary>
    [OwAdditional(SurveysTemplateIdName, SurveysTemplateIdString)]
    public class YanZhengDaiXiaAlgorithm : GaoRonrongAlgorithmBase
    {
        /// <summary>
        /// 此算法处理的调查模板Id。
        /// </summary>
        public const string SurveysTemplateIdString = "063A2233-4671-41CF-877D-F9A8AA2BEB06";

        /// <summary>
        /// 病种的中文名称。
        /// </summary>
        public const string CnName = "炎症性带下病";

        /// <summary>
        /// 病种数据文件目录路径。
        /// </summary>
        public const string DataFilePath = "Content/炎症性带下病";

        /// <summary>
        /// 初始化函数。
        /// </summary>
        /// <param name="context"></param>
        [OwAdditional(InitializationFuncName)]
        public static void Initialize(DbContext context)
        {
            InitializeCore(context, $"~/{DataFilePath}/{CnName}-症状表.txt", typeof(YanZhengDaiXiaAlgorithm));
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
        public YanZhengDaiXiaAlgorithm()
        {

        }

        public override IEnumerable<GeneratedNumeber> GeneratedNumebers =>
            CnMedicineLogicBase.GetOrCreateAsync<YanZhengDaiXiaGeneratedNumeber>($"~/{DataFilePath}/{CnName}-症状归类表.txt").Result;

        public override IEnumerable<GrrBianZhengFenXingBase> BianZhengFenXings =>
            CnMedicineLogicBase.GetOrCreateAsync<YanZhengDaiXiaFenXing>($"~/{DataFilePath}/{CnName}-辨证分型表.txt").Result;

        public override IEnumerable<GrrJingLuoBianZhengBase> JingLuoBianZhengs =>
            CnMedicineLogicBase.GetOrCreateAsync<YanZhengDaiXiaJingLuoBian>($"~/{DataFilePath}/{CnName}-经络辨证表.txt").Result;

        public override IEnumerable<CnDrugCorrectionBase> CnDrugCorrections =>
            CnMedicineLogicBase.GetOrCreateAsync<YanZhengDaiXiaCnDrugCorrection>($"~/{DataFilePath}/{CnName}-加减表.txt").Result;


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
    /// 慢性盆腔炎算法类。
    /// </summary>
    [OwAdditional(SurveysTemplateIdName, SurveysTemplateIdString)]
    public class ManXingPenQiongYanAlgorithm : GaoRonrongAlgorithmBase
    {
        /// <summary>
        /// 此算法处理的调查模板Id。
        /// </summary>
        public const string SurveysTemplateIdString = "94B76BFC-AAB1-4699-A02C-1721E4D2F130";

        /// <summary>
        /// 病种的中文名称。
        /// </summary>
        public const string CnName = "慢性盆腔炎";

        /// <summary>
        /// 病种数据文件目录路径。
        /// </summary>
        public const string DataFilePath = "Content/慢性盆腔炎";

        /// <summary>
        /// 初始化函数。
        /// </summary>
        /// <param name="context"></param>
        [OwAdditional(InitializationFuncName)]
        public static void Initialize(DbContext context)
        {
            InitializeCore(context, $"~/{DataFilePath}/{CnName}-症状表.txt", typeof(ManXingPenQiongYanAlgorithm));
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
        public ManXingPenQiongYanAlgorithm()
        {

        }

        public override IEnumerable<GeneratedNumeber> GeneratedNumebers =>
            CnMedicineLogicBase.GetOrCreateAsync<YanZhengDaiXiaGeneratedNumeber>($"~/{DataFilePath}/{CnName}-症状归类表.txt").Result;

        public override IEnumerable<GrrBianZhengFenXingBase> BianZhengFenXings =>
            CnMedicineLogicBase.GetOrCreateAsync<YanZhengDaiXiaFenXing>($"~/{DataFilePath}/{CnName}-辨证分型表.txt").Result;

        public override IEnumerable<GrrJingLuoBianZhengBase> JingLuoBianZhengs =>
            CnMedicineLogicBase.GetOrCreateAsync<YanZhengDaiXiaJingLuoBian>($"~/{DataFilePath}/{CnName}-经络辨证表.txt").Result;

        public override IEnumerable<CnDrugCorrectionBase> CnDrugCorrections =>
            CnMedicineLogicBase.GetOrCreateAsync<YanZhengDaiXiaCnDrugCorrection>($"~/{DataFilePath}/{CnName}-加减表.txt").Result;


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