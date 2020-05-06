using OW.Data.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

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

    /// <summary>
    /// 方剂。Name属性是药方名称。
    /// </summary>
    [DataContract]
    public class CnPrescription : ThingEntityBase
    {
        public CnPrescription()
        {

        }

        [DataMember]
        public virtual List<CnDrug> Drugs { get; set; } = new List<CnDrug>();

        /// <summary>
        /// <see cref="Drugs"/>属性中，名称相同的药物合并，取最大值。
        /// </summary>
        public void Max()
        {

        }

    }

    /// <summary>
    /// 药物成分。
    /// </summary>
    [DataContract]
    public class CnDrug : EntityWithGuid
    {
        public CnDrug()
        {

        }

        /// <summary>
        /// 药物名称。
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// 数量。如5克，这里是5。
        /// </summary>
        [DataMember]
        public decimal Number { get; set; }

        /// <summary>
        /// 单位。如:克。
        /// </summary>
        [DataMember]
        public string Unit { get; set; }

        
    }
}