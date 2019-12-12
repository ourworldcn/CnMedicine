using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Runtime.Serialization;

namespace OW.Data.Entity
{
    /// <summary>
    /// 调查问卷的模板类。封装一个专项的问卷调查提问数据。
    /// </summary>
    [DataContract]
    public class SurveysTemplate : ThingEntityBase
    {
        /// <summary>
        /// 该问卷内所有问题的导航属性。
        /// </summary>
        public virtual List<SurveysQuestionTemplate> SurveysQuestionTemplates { get; set; }
    }

    /// <summary>
    /// 每个问题的类型。
    /// </summary>
    [Serializable]
    [Flags]
    public enum QuestionsKind : short
    {
        /// <summary>
        /// 对先择题，有此标志为多选，无此标志为单选题。
        /// </summary>
        Multi=1,

        /// <summary>
        /// 获得子类型的掩码。
        /// </summary>
        SubKindMask = 255,

        /// <summary>
        /// 判断题。
        /// </summary>
        Judgment = 256,

        /// <summary>
        /// 选择题。单选题。
        /// </summary>
        Choice = 512,

        /// <summary>
        /// 多选题。
        /// </summary>
        MultiChoice= Choice+ Multi,

        /// <summary>
        /// 问答题。
        /// </summary>
        Describe = 1024,

    }

    /// <summary>
    /// 问卷调查的每个问题的设置数据。
    /// </summary>
    [DataContract]
    public class SurveysQuestionTemplate : ThingEntityBase
    {
        /// <summary>
        /// 所属调查问卷的Id。
        /// </summary>
        [DataMember]
        [ForeignKey(nameof(SurveysTemplate))]
        public Guid SurveysTemplateId { get; set; }

        /// <summary>
        /// 所属调查问卷的导航属性。
        /// </summary>
        public virtual SurveysTemplate SurveysTemplate { get; set; }

        /// <summary>
        /// 答案项。仅对选择题有效。
        /// </summary>
        public virtual List<SurveysAnswerTemplate> SurveysTemplateAnswerItems{ get; set; }

        /// <summary>
        /// 对问题的说明。
        /// </summary>
        [DataMember]
        public string QuestionTitle { get; set; }

        /// <summary>
        /// 此问题出现的顺序号。越小越靠前。不必连续，也未必是正数。
        /// </summary>
        [DataMember]
        public int OrderNum { get; set; }

        /// <summary>
        /// 问题的类型。
        /// </summary>
        [DataMember]
        public QuestionsKind Kind { get; set; }

    }

    /// <summary>
    /// 每个答案的设置数据。
    /// </summary>
    [DataContract]
    public class SurveysAnswerTemplate : ThingEntityBase
    {
        /// <summary>
        /// 所属问题的Id。
        /// </summary>
        [DataMember]
        [ForeignKey(nameof(SurveysQuestionTemplate))]
        public Guid SurveysQuestionTemplateId { get; set; }

        /// <summary>
        /// 所属问题。
        /// </summary>
        public virtual SurveysQuestionTemplate SurveysQuestionTemplate { get; set; }

        /// <summary>
        /// 答案的标题。
        /// </summary>
        [DataMember]
        public string AnswerTitle { get; set; }

        /// <summary>
        /// 此项出现的顺序号。越小越靠前。不必连续，也未必是正数。
        /// </summary>
        [DataMember]
        public int OrderNum { get; set; }

        /// <summary>
        /// 如果此项是一个选择题的答案项，为true时标识此项是一个允许填写文字的附加项。例如：其它，选中后要用户填写一段文字。
        /// </summary>
        [DataMember]
        public bool AllowAdditional { get; set; }
    }
}