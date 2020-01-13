using OW.Data.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace CnMedicineServer.Models
{
    /// <summary>
    /// 专病种类。暂时未用，因仅单一病种。未来有归类作用。
    /// </summary>
    [DataContract]
    public class SpecialCases : ThingEntityBase
    {
        /// <summary>
        /// 使用的问卷调查模板的导航属性。
        /// </summary>
        public virtual SurveysTemplate SurveysTemplate { get; set; }

        [DataMember]
        [ForeignKey(nameof(SurveysTemplate))]
        public Guid? SurveysTemplateId { get; set; }
    }
    [DataContract]
    public class SpecialCasesItem : ThingEntityBase
    {

    }

    /// <summary>
    /// 专病的诊断模型。
    /// </summary>
    [DataContract]
    public class InsomniaCasesItem : SpecialCasesItem
    {
        /// <summary>
        /// 评分表1导航属性。
        /// </summary>
        public virtual List<InsomniaConversion11> Conversion11s { get; set; }

        /// <summary>
        /// 评分表2导航属性。
        /// </summary>
        public virtual List<InsomniaConversion12> Conversion12s { get; set; }
        
        /// <summary>
        /// 药物输出导航属性。
        /// </summary>
        public virtual List<InsomniaCnDrugConversion> CnDrugCorrections { get; set; }

        /// <summary>
        /// 药物加味导航属性。
        /// </summary>
        public virtual List<InsomniaCnDrugConversion2> CnDrugConversion2s { get; set; }

        /// <summary>
        /// 药物剂量矫正导航属性。
        /// </summary>
        public virtual List<InsomniaCnDrugCorrection> CnDrugCorrection { get; set; }

        public virtual List<Surveys> Surveys { get; set; }
    }


    /// <summary>
    /// 专病鼻炎的诊断模型。
    /// </summary>
    [DataContract]
    public class RhinitisCasesItem : SpecialCasesItem
    {
        /// <summary>
        /// 评分表导航属性。
        /// </summary>
        public virtual List<RhinitisConversion> Conversions { get; set; }

        /// <summary>
        /// 药物输出导航属性。
        /// </summary>
        public virtual List<RhinitisCnDrugConversion> CnDrugCorrections { get; set; }

        /// <summary>
        /// 病机药物输出导航属性。
        /// </summary>
        public virtual List<RhinitisCnDrugPathogen> CnDrugConversion2s { get; set; }

        public virtual List<Surveys> Surveys { get; set; }
    }

}