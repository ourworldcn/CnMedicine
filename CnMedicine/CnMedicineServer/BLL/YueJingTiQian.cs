
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
                result.Conclusion = "(您输入的症状暂无对应药方，请联系医生。)";
            return result;
        }
    }
}