using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Runtime.Serialization;

namespace OW.Data.Entity
{

    public static class OwDbContextExtensions
    {
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> values)
        {
            List<T> lst = collection as List<T>;
            if (null != lst)
                lst.AddRange(values);
            else
                foreach (var item in values)
                {
                    collection.Add(item);
                }
        }

        public static void Load<T>(this DbContext context, ICollection<T> collection)
        {
        }

        /// <summary>
        /// 获取合并的数据。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDest"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="dests"></param>
        /// <param name="srcs"></param>
        /// <param name="destKeyCreator">从集合的元素中提取key,key不可重复。</param>
        /// <param name="srcKeyCreator">从集合的元素中提取key,key不可重复。</param>
        /// <param name="removes">应移除的对象，null则忽略此参数。</param>
        /// <param name="adds">应增加的对象，null则忽略此参数。</param>
        /// <param name="modifies">应修改的对象，null则忽略此参数。</param>
        /// <exception cref="ArgumentException">从集合的元素中提取key,key不可重复。</exception>
        static public void GetMergeInfo<TSource, TDest, TKey>(this IEnumerable<TDest> dests, IEnumerable<TSource> srcs, Func<TDest, TKey> destKeyCreator, Func<TSource, TKey> srcKeyCreator,
            ICollection<TDest> removes, ICollection<TSource> adds, ICollection<Tuple<TSource, TDest>> modifies)
        {
            var intersectKeys = dests.Join(srcs, destKeyCreator, srcKeyCreator, (dest, src) => Tuple.Create(src, dest)).ToDictionary(c => destKeyCreator(c.Item2));  //重合的key集合
            modifies?.AddRange(intersectKeys.Values);  //生成更新对象集合
            removes?.AddRange(dests.Where(c => !intersectKeys.ContainsKey(destKeyCreator(c)))); //生成删除对象集合
            adds?.AddRange(srcs.Where(c => !intersectKeys.ContainsKey(srcKeyCreator(c))));   //生成追加对象集合
        }

    }

    public class MyValidation : ValidationAttribute
    {
        public MyValidation()
        {
        }

        public override bool RequiresValidationContext { get; } = true;

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //var result = base.IsValid(value, validationContext);

            return ValidationResult.Success;
        }
    }


    /// <summary>
    /// 每个问题的类型。
    /// </summary>
    [Serializable]
    [Flags]
    public enum QuestionsKind : short
    {
        /// <summary>
        /// 对选择题，有此标志为多选，无此标志为单选题。
        /// </summary>
        Multi = 1,
        多重 = Multi,

        /// <summary>
        /// 有该标志，这个问题不填写答案，否则需要填写。
        /// </summary>
        AllowEmpty = 2,
        可空 = AllowEmpty,

        /// <summary>
        /// 本项存在附属图片。
        /// </summary>
        Picture = 4,
        图片 = Picture,

        /// <summary>
        /// 获得子类型的掩码。
        /// </summary>
        SubKindMask = 255,

        /// <summary>
        /// 判断题。
        /// </summary>
        Judgment = 256,
        判断 = Judgment,

        /// <summary>
        /// 选择题。单选题。
        /// </summary>
        Choice = 512,
        选择 = Choice,

        /// <summary>
        /// 多选题。
        /// </summary>
        MultiChoice = Choice + Multi,

        /// <summary>
        /// 问答题。
        /// </summary>
        Describe = 1024,
        问答 = Describe,
    }

    /// <summary>
    /// 调查问卷的模板类。封装一个专项的问卷调查提问数据。
    /// 每个"问卷调查模板对象"有多个"问卷调查问题模板"子对象，每个"问卷调查问题模板"对象，有多个"问卷调查答案模板"子对象。
    /// </summary>
    [DataContract]
    public class SurveysTemplate : ThingEntityBase, ICloneable
    {
        /// <summary>
        /// 该问卷内所有问题的导航属性。
        /// </summary>
        [DataMember]
        public virtual List<SurveysQuestionTemplate> Questions { get; set; }

        /// <summary>
        /// 扩展属性的字符串。如:支持复诊1。表示是需要调用上次的复诊记录比对。
        /// </summary>
        [DataMember]
        public string UserState { get; set; }

        public object Clone()
        {
            var obj = this;
            return new SurveysTemplate()
            {
                CreateUtc = obj.CreateUtc,
                Description = obj.Description,
                Id = obj.Id,
                Name = obj.Name,
                ShortName = obj.ShortName,
                Questions = obj.Questions.Select(c =>
                  {
                      return (SurveysQuestionTemplate)c.Clone();
                  }).ToList(),
            };
        }

        public override string ToString()
        {
            return $"Name={Name}";
        }

        /// <summary>
        /// 该属性是该模板的所有问卷实例。
        /// 有延迟加载机制，不访问就不会有开销。
        /// </summary>
        // public virtual List<Surveys> Surveys { get; set; }

    }

    /// <summary>
    /// 问卷调查的每个问题的设置数据。
    /// </summary>
    [DataContract]
    public class SurveysQuestionTemplate : ThingEntityBase, ICloneable
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        public SurveysQuestionTemplate()
        {

        }

        /// <summary>
        /// 返回表示当前对象的字符串。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Name = {Name},QuestionTitle = {QuestionTitle}";
        }


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
        [DataMember]
        public virtual List<SurveysAnswerTemplate> Answers { get; set; }

        /// <summary>
        /// 对问题的说明。即问题的名字。
        /// </summary>
        [DataMember]
        public string QuestionTitle { get; set; }

        /// <summary>
        /// 此问题的编号。
        /// </summary>
        [DataMember]
        public int IdNumber { get; set; }

        /// <summary>
        /// 此问题出现的顺序号。越小越靠前。不必连续，也未必是正数。
        /// </summary>
        int _OrderNum = int.MinValue;
        [DataMember]
        public int OrderNum { get => _OrderNum == int.MinValue ? IdNumber : _OrderNum; set => _OrderNum = value; }

        /// <summary>
        /// 问题的类型。
        /// </summary>
        [DataMember]
        public QuestionsKind Kind { get; set; }

        /// <summary>
        /// 内部逻辑后期使用的数据。客户端可以不用管。
        /// </summary>
        [DataMember]
        public string UserState { get; set; }

        /// <summary>
        /// 显示该问题的前提编号。如果是空则不限制（无条件正常显示），否则逗号分隔的序号，如 "1005" 或 "2589,1560"之类。
        /// 多个编号则需要多个编号全存在才显示此项。
        /// </summary>
        [DataMember]
        public string DisplayConditions { get; set; }

        public object Clone()
        {
            var obj = this;
            return new SurveysQuestionTemplate()
            {
                CreateUtc = obj.CreateUtc,
                Description = obj.Description,
                Id = obj.Id,
                Name = obj.Name,
                ShortName = obj.ShortName,
                Answers = obj.Answers.Select(c => (SurveysAnswerTemplate)c.Clone()).ToList(),
                Kind = obj.Kind,
                IdNumber = obj.IdNumber,
                QuestionTitle = obj.QuestionTitle,
                SurveysTemplateId = obj.SurveysTemplateId,
                UserState = obj.UserState,
            };
        }

    }

    /// <summary>
    /// 每个答案的设置数据。
    /// </summary>
    [DataContract]
    public class SurveysAnswerTemplate : ThingEntityBase, ICloneable
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        public SurveysAnswerTemplate()
        {

        }

        /// <summary>
        /// 返回表示当前对象的字符串。
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Name = {Name} ,AnswerTitle = {AnswerTitle}";
        }

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
        /// 此答案的编号。
        /// </summary>
        [DataMember]
        public int IdNumber { get; set; }

        int _OrderNum = int.MinValue;

        /// <summary>
        /// 此项出现的顺序号。越小越靠前。不必连续，也未必是正数。仅作为排序参考。
        /// </summary>
        [DataMember]
        public int OrderNum { get => _OrderNum == int.MinValue ? IdNumber : _OrderNum; set => _OrderNum = value; }

        /// <summary>
        /// 暂时无用。
        /// 如果此项是一个选择题的答案项，为true时标识此项是一个允许填写文字的附加项。例如：其它，选中后要用户填写一段文字。
        /// </summary>
        [DataMember]
        public bool AllowAdditional { get; set; }

        /// <summary>
        /// 内部逻辑后期使用的数据。客户端可以不用管。没有采用其它编码方式是为了符合医生习惯，如果使用Json之类模式，还需要额外的转换步骤。但的确可以使用其他模式，只要应用自己可以解释即可。
        /// 具体到该项目，例如"编号:21;症候:侧头痛;脏腑评分：心+1，肝+1，胆+1，胃+1，小肠+1;证型评分:火亢+1，气郁+1，湿热+1，脾胃不和+1，少阳枢机不利+1，痰热+1"，为避免输入错误分割时能理解中文符号。
        /// 这里未来不排除是记录了一个Id,连接到其它表。但无论如何，这个就是通用调查问卷子系统和其它子系统的一种连接手段，问卷子系统本身不考虑。
        /// </summary>
        [DataMember]
        public string UserState { get; set; }

        /// <summary>
        /// 显示该问题的前提编号。如果是空则不限制（无条件正常显示），否则逗号分隔的序号，如 "1005" 或 "2589,1560"之类。
        /// 若隶属的整个问题已经不显示，则不考虑该属性。
        /// 多个编号则需要多个编号全存在才显示此项。
        /// </summary>
        [DataMember]
        public string DisplayConditions { get; set; }

        public object Clone()
        {
            var obj = this;
            return new SurveysAnswerTemplate()
            {
                CreateUtc = obj.CreateUtc,
                Description = obj.Description,
                Id = obj.Id,
                Name = obj.Name,
                ShortName = obj.ShortName,
                AllowAdditional = obj.AllowAdditional,
                AnswerTitle = obj.AnswerTitle,
                IdNumber = obj.IdNumber,
                SurveysQuestionTemplateId = obj.SurveysQuestionTemplateId,
                UserState = obj.UserState,
            };
        }
    }

    /// <summary>
    /// 调查问卷的实例数据。每个用户填写一份问卷就有这个类的一个实例，对应数据库的一条记录。
    /// </summary>
    [DataContract]
    public class Surveys : ThingEntityBase
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        public Surveys()
        {

        }

        /// <summary>
        /// 对应调查问卷模板Id。
        /// </summary>
        [DataMember]
        [ForeignKey(nameof(Template))]
        [Required]
        public Guid TemplateId { get; set; }

        /// <summary>
        /// 模板类的导航属性。
        /// </summary>
        public virtual SurveysTemplate Template { get; set; }

        /// <summary>
        /// 答案项的导航属性。必填。
        /// </summary>
        [DataMember]
        public virtual List<SurveysAnswer> SurveysAnswers { get; set; } = new List<SurveysAnswer>();

        /// <summary>
        /// 加载图片信息。
        /// </summary>
        /// <param name="dbContext"></param>
        public void LoadPictures(DbContext dbContext)
        {
            if (null == SurveysAnswers)
                return;
            foreach (var item in SurveysAnswers)
            {
                item.LoadPictures(dbContext);
            }
        }

        /// <summary>
        /// 保存图片信息。
        /// </summary>
        /// <param name="dbContext"></param>
        public void SavePictures(DbContext dbContext)
        {
            if (null == SurveysAnswers)
                return;
            foreach (var item in SurveysAnswers)
            {
                item.SavePictures(dbContext);
            }
        }

        /// <summary>
        /// 这是通用调查问卷子系统与其它系统交互使用的属性。
        /// 目前如果是复诊则这里必须是,"复诊1"
        /// </summary>
        [DataMember]
        public string UserState { get; set; }

        /// <summary>
        /// 所属用户Id。必填。
        /// </summary>
        [DataMember]
        [MaxLength(128)]
        [Index]
        [Required(AllowEmptyStrings = false)]
        public string UserId { get; set; }

        /// <summary>
        /// 结论Id。
        /// </summary>
        [DataMember]
        public Guid? ConclusionId { get; set; }

    }

    /// <summary>
    /// 答案项实例。对复选题有多个该对象的实例对应一个问题。
    /// </summary>
    [DataContract]
    public class SurveysAnswer : EntityWithGuid
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        public SurveysAnswer()
        {

        }

        /// <summary>
        /// 对选择题绑定答案模板Id，绑定了该Id对就意味着选择了该项。显然对复选题有多个该对象的实例对应一个问题。
        /// 其它题型绑定到问题模板Id。必填。
        /// </summary>
        [DataMember]
        [Index]
        [Required]
        public Guid TemplateId { get; set; }

        /// <summary>
        /// 所属问卷Id。
        /// 在嵌套在 Surveys 中，此值不用填写。
        /// </summary>
        [DataMember]
        [ForeignKey(nameof(Surveys))]
        public Guid SurveysId { get; set; }

        /// <summary>
        /// 所属问卷的导航属性。
        /// </summary>
        public virtual Surveys Surveys { get; set; }

        /// <summary>
        /// 对问答题是答题的文字。对选择题允许扩展的项，这里填写额外说明文字。
        /// </summary>
        [DataMember(IsRequired = false)]
        public string Guts { get; set; }

        /// <summary>
        /// 额外附属信息。这是通用调查问卷子系统与其它系统交互使用的属性。
        /// 目前已知的，如果填写为"无效1"，怎表示此项治疗是无效的。
        /// </summary>
        [DataMember(IsRequired = false)]
        public string UserState { get; set; }

        /// <summary>
        /// 附属的图片，仅在模板指定了图片属性时有效。否则总返回空集合。
        /// </summary>
        [DataMember]
        [NotMapped]
        public virtual List<ResourceStore> Pictures { get; set; } = new List<ResourceStore>();

        public void LoadPictures(DbContext dbContext)
        {
            var coll = dbContext.Set<ResourceStore>().Where(c => c.ThingEntityId == Id);
            Pictures = coll.ToList();
        }

        public void SavePictures(DbContext dbContext)
        {
            if (null != Pictures && Pictures.Count > 0)
                dbContext.Set<ResourceStore>().AddOrUpdate(Pictures.ToArray());
        }
    }

    /// <summary>
    /// 调查的结论。
    /// Description用于记录简单结论。
    /// </summary>
    [DataContract]
    public class SurveysConclusion : ThingEntityBase
    {
        public SurveysConclusion()
        {

        }

        /// <summary>
        /// 所属调查问卷实例的导航属性。
        /// </summary>
        public virtual Surveys Surveys { get; set; }

        /// <summary>
        /// 所属调查问卷实例的Id。
        /// </summary>
        [ForeignKey(nameof(Surveys))]
        [DataMember]
        public Guid SurveysId { get; set; }

        /// <summary>
        /// 主要诊疗结果。目前是处方数据。
        /// </summary>
        [DataMember]
        public string Conclusion { get; set; }

        /// <summary>
        /// 一些额外信息，目前有医生所看的详细结论。
        /// </summary>
        [DataMember(IsRequired = false)]
        public string ExtendedInfomation { get; set; }

        /// <summary>
        /// 调试用信息。
        /// </summary>
        [DataMember]
        public string DebugMessage { get; set; }
    }
}