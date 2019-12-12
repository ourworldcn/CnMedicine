using OW.Data.Entity;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace CnMedicineServer.Models
{
    /// <summary>
    /// 专病种类。
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

    /// <summary>
    /// 专病的诊断模型。
    /// </summary>
    [DataContract]
    public class SpecialCasesItem : ThingEntityBase
    {

    }
}