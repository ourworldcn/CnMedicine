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
                result.Conclusion = "(您输入的症状暂无对应药方，请联系医生。)";
            return result;
        }
    }

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
                result.Conclusion = "(您输入的症状暂无对应药方，请联系医生。)";
            return result;
        }
    }
}