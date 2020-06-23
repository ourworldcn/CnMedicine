using CnMedicineServer.Models;
using OW;
using OW.Data.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Threading;

#pragma warning disable CS3021 // 由于程序集没有 CLSCompliant 特性，因此类型或成员不需要 CLSCompliant 特性
namespace CnMedicineServer.Bll
{

    public static class CnMedicineExtends
    {
        /// <summary>
        /// 获取根据阈值匹配的编号集合。
        /// </summary>
        /// <typeparam name="T">规则集合的元素类型。</typeparam>
        /// <typeparam name="TResult">结果集合的元素类型。</typeparam>
        /// <param name="numbers"></param>
        /// <param name="rules"></param>
        /// <param name="numbersSelector"></param>
        /// <param name="thresholdsSelector"></param>
        /// <param name="resultSelector"></param>
        /// <returns></returns>
        public static IEnumerable<TResult> GetNumbers<T, TResult>(this IEnumerable<int> numbers, IEnumerable<T> rules, Func<T, IEnumerable<int>> numbersSelector, Func<T, float> thresholdsSelector, Func<T, TResult> resultSelector)
        {
            var result = from tmp in rules
                         let conditions = numbersSelector(tmp)
                         let count = conditions.Count()
                         where (float)numbers.Intersect(conditions).Count() / count >= thresholdsSelector(tmp)
                         select resultSelector(tmp);
            return result;
        }

        /// <summary>
        /// 取权值比较后最大的一组结果。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="valueSelector"></param>
        /// <param name="resultSelector"></param>
        /// <returns></returns>
        public static IEnumerable<TResult> Tops<TSource, TValue, TResult>(this IEnumerable<TSource> source, Func<TSource, TValue> valueSelector, Func<TSource, TValue, TResult> resultSelector) where TValue : IComparable<TValue>
        {
            if (!source.Any())   //若是空集合
                return Array.Empty<TResult>();
            var max = source.Max(c => valueSelector(c));
            return source.Where(c => max.CompareTo(valueSelector(c)) == 0).Select(c => resultSelector(c, max));
        }

        /// <summary>
        /// 将源序列打乱顺序，返回一个顺序随机的序列。但两个序列包含的元素一样。时间复杂度O(n),n是<paramref name="source"/>中元素数量。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static List<T> RandomSequence<T>(this IEnumerable<T> source)
        {
            var tmp = source.ToList();
            var result = new List<T>();
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

    /// <summary>
    /// 分析数据类的基类。
    /// </summary>
    public class CnMedicineAnalysisDataBase
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        public CnMedicineAnalysisDataBase()
        {

        }

        List<SurveysAnswer> _Answers;

        /// <summary>
        /// 答案集合。
        /// </summary>
        public List<SurveysAnswer> Answers { get => _Answers; protected set => _Answers = value; }

        /// <summary>
        /// 键是编号，值是对应的模板。
        /// </summary>
        Dictionary<int, SurveysAnswerTemplate> _AnswerTemplates;

        /// <summary>
        /// 键是编号，值是对应的模板。
        /// </summary>
        public Dictionary<int, SurveysAnswerTemplate> AnswerTemplates { get => _AnswerTemplates; set => _AnswerTemplates = value; }

        Dictionary<int, SurveysQuestionTemplate> _QuestionTemplates;

        /// <summary>
        /// 键是编号，值是对应的模板。
        /// </summary>
        public Dictionary<int, SurveysQuestionTemplate> QuestionTemplates { get => _QuestionTemplates; set => _QuestionTemplates = value; }

        /// <summary>
        /// 调查问卷中所有编号的集合。
        /// </summary>
        public HashSet<int> Numbers { get => _Numbers; set => _Numbers = value; }

        HashSet<int> _Numbers;

        public virtual void SetAnswers(IEnumerable<SurveysAnswer> answers, DbContext dbContext)
        {
            _Answers = answers.ToList();
            var tIds = _Answers.Select(c => c.TemplateId).ToArray();
            _AnswerTemplates = dbContext.Set<SurveysAnswerTemplate>().Where(c => tIds.Contains(c.Id)).ToDictionary(c => c.IdNumber);
            _QuestionTemplates = dbContext.Set<SurveysQuestionTemplate>().Where(c => tIds.Contains(c.Id)).ToDictionary(c => c.IdNumber);
            _Numbers = new HashSet<int>(_AnswerTemplates.Keys.Union(_QuestionTemplates.Keys));
        }

        /// <summary>
        /// 设置症状的集合。
        /// </summary>
        /// <param name="answers"></param>
        /// <param name="dbContext"></param>
        public virtual void SetSigns(IEnumerable<SurveysAnswer> answers, DbContext dbContext)
        {
            _Answers = answers.ToList();
            var tIds = _Answers.Select(c => c.TemplateId).ToArray();
            _AnswerTemplates = dbContext.Set<SurveysAnswerTemplate>().Where(c => tIds.Contains(c.Id)).ToDictionary(c => c.IdNumber);
            _QuestionTemplates = dbContext.Set<SurveysQuestionTemplate>().Where(c => tIds.Contains(c.Id)).ToDictionary(c => c.IdNumber);
            _Numbers = new HashSet<int>(_AnswerTemplates.Keys.Concat(_QuestionTemplates.Keys));
        }


        private readonly object _SyncLocker = new object();

        /// <summary>
        /// 获取同步使用的锁。
        /// </summary>
        public object SyncLocker { get => _SyncLocker; }
    }

    /// <summary>
    /// 所有智能问诊的算法基类。
    /// </summary>
    public abstract class CnMedicineAlgorithmBase
    {
        /// <summary>
        /// 此字符串标志<see cref="OwAdditionalAttribute"/>中Name属性内容。表示其Value中内容是该类处理的调查模板Id。
        /// </summary>
        public const string SurveysTemplateIdName = "782281B8-D74B-42FA-9F32-9E8EB7B5049B";

        /// <summary>
        /// 此字符串标志<see cref="OwAdditionalAttribute"/>中Name属性内容。Value的内容将被忽略，此批注放在静态初始化的函数上，函数必须签名类似static void xxx(DbContext context)。
        /// </summary>
        public const string InitializationFuncName = "175EFEDB-7561-44E5-A053-86FC03C39255";

        /// <summary>
        /// 方剂存储在ThingProperties中的Name属性内容。
        /// </summary>
        public const string CnPrescriptionesName = "FD293367-9E05-4466-AA13-B9B529A2DE89";

        /// <summary>
        /// 初始化症状数据。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="signsFileName">症状数据的文件名，如"~/content/xxx/xxx1.txt"</param>
        /// <param name="algorithmType"></param>
        public static void InitializeCore(DbContext context, string signsFileName, Type algorithmType)
        {
            var idAttr = algorithmType.GetCustomAttributes(true).OfType<OwAdditionalAttribute>().FirstOrDefault(c => c.Name == SurveysTemplateIdName);
            var SurveysTemplateIdString = idAttr.Value;
            var survId = Guid.Parse(SurveysTemplateIdString);
            var tmp = context.Set<SurveysTemplate>().Find(survId);

            if (null != tmp)
                return;

            var fullPath = System.Web.HttpContext.Current.Server.MapPath(signsFileName);    //本机全路径
            var path = Path.GetDirectoryName(fullPath); //路径名
            List<CnMedicineSignsBase> signs;
            using (var tdb = new TextFileContext(path) { IgnoreQuotes = true, })
            {
                var fileName = Path.GetFileName(fullPath);
                signs = tdb.GetList<CnMedicineSignsBase>(fileName);
            }
            //初始化调查模板项
            var surveysTemplate = new SurveysTemplate()
            {
                Id = survId,
                //Name = "经行乳房胀痛",
                //UserState = "支持复诊0",
                Questions = new List<SurveysQuestionTemplate>(),
                //Description = "经行乳房痛：每值经前或经期乳房作胀,甚至胀满疼痛,或乳头痒痛者,称“经行乳房痛”。包含乳腺增生、乳腺纤维瘤等乳腺疾病的伴发症状。",
            };
            context.Set<SurveysTemplate>().AddOrUpdate(surveysTemplate);
            //添加专病项
            //InsomniaCasesItem caseItem = new InsomniaCasesItem()
            //{
            //    Name = "经行乳房胀痛",
            //};
            //context.Set<InsomniaCasesItem>().AddOrUpdate(caseItem);
            //添加问题项
            var coll = signs.GroupBy(c => c.Question).Select(c =>
            {
                SurveysQuestionTemplate sqt = new SurveysQuestionTemplate()
                {
                    Kind = c.First().QuestionsKind,
                    IdNumber = c.First().Number,
                    QuestionTitle = c.Key,
                    UserState = "",
                    OrderNum = c.First().OrderNum,
                };
                sqt.Answers = c.Select(subc =>
                {
                    SurveysAnswerTemplate sat = new SurveysAnswerTemplate()
                    {
                        AnswerTitle = subc.ZhengZhuang,
                        IdNumber = subc.Number,
                        UserState = $"编号{subc.Number}",
                        DisplayConditions = subc.DisplayContions,
                        OrderNum = subc.OrderNum,
                    };
                    return sat;
                }).ToList();
                return sqt;
            });
            surveysTemplate.Questions.AddRange(coll);
        }

        public CnMedicineAlgorithmBase()
        {
            AdditionalNumbers.CollectionChanged += AdditionalNumbers_CollectionChanged;
        }

        private void AdditionalNumbers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    _AllNumbers = null;
                    break;
                default:
                    break;
            }
        }

        private readonly object _SyncLocker = new object();

        /// <summary>
        /// 获取同步使用的锁。
        /// </summary>
        public object SyncLocker => _SyncLocker;

        private List<SurveysAnswer> _Answers;
        private Dictionary<int, SurveysAnswerTemplate> _AnswerTemplates;
        private Dictionary<int, SurveysQuestionTemplate> _QuestionTemplates;

        private HashSet<int> _Numbers;

        /// <summary>
        /// 调查问卷中所有编号的集合。
        /// </summary>
        public HashSet<int> Numbers { get => _Numbers; set => _Numbers = value; }

        /// <summary>
        /// 因某种原因加入的编号集合。
        /// 修改该集合后，下次调用<see cref="AllNumbers"/>时将自动添加到其中。
        /// </summary>
        protected ObservableCollection<int> AdditionalNumbers { get; } = new ObservableCollection<int>();

        Tuple<string, Type> _GeneratedNumebersInfo;

        /// <summary>
        /// 获取或设置<see cref="GeneratedNumebers"/>属性的默认获取方法。
        /// </summary>
        /// <exception cref="InvalidOperationException">只能设置一次。</exception>
        public Tuple<string, Type> GeneratedNumebersInfo
        {
            get => _GeneratedNumebersInfo;
            set
            {
                if (null != _GeneratedNumebersInfo)
                    throw new InvalidOperationException($"{nameof(GeneratedNumebersInfo)}属性只能设置一次。");
                _GeneratedNumebersInfo = value;
            }
        }

        /// <summary>
        /// 获取派生编号的规则集合。
        /// </summary>
        public virtual IEnumerable<GeneratedNumeber> GeneratedNumebers
        {
            get => CnMedicineLogicBase.GetOrCreateAsync<GeneratedNumeber>(_GeneratedNumebersInfo.Item1).Result;
        }

        Tuple<string, Type> _CnDrugCorrectionsInfo;

        /// <summary>
        /// 获取或设置<see cref="CnDrugCorrectionsInfo"/>属性的默认获取方法。
        /// </summary>
        /// <exception cref="InvalidOperationException">只能设置一次。</exception>
        public Tuple<string, Type> CnDrugCorrectionsInfo
        {
            get => _CnDrugCorrectionsInfo;
            set
            {
                if (null != _GeneratedNumebersInfo)
                    throw new InvalidOperationException($"{nameof(CnDrugCorrectionsInfo)}属性只能设置一次。");
                _CnDrugCorrectionsInfo = value;
            }
        }

        /// <summary>
        /// 获取药物加减的规则集合。
        /// </summary>
        public virtual IEnumerable<CnDrugCorrectionBase> CnDrugCorrections
        {
            get => CnMedicineLogicBase.GetOrCreateAsync<CnDrugCorrectionBase>(CnDrugCorrectionsInfo.Item1).Result;
        }

        /// <summary>
        /// 最终药物结果列表。
        /// </summary>
        public abstract List<Tuple<string, decimal>> Results { get; }

        private List<int> _AllNumbers;

        /// <summary>
        /// 添加了追加编号后的再添加派生编号的所有编号。
        /// </summary>
        public List<int> AllNumbers
        {
            get
            {
                lock (SyncLocker)
                    if (null == _AllNumbers)
                    {
                        var adding = Numbers.Union(AdditionalNumbers).ToArray();
                        var coll = from tmp in GeneratedNumebers
                                   let fac = (float)tmp.Numbers.Intersect(adding).Count() / tmp.Numbers.Count
                                   where fac >= tmp.Thresholds
                                   select tmp.Number;
                        _AllNumbers = adding.Union(coll).ToList();
                    }
                return _AllNumbers;
            }
        }

        protected abstract SurveysConclusion GetResultCore(Surveys surveys, ApplicationDbContext db);

        public SurveysConclusion GetResult(Surveys surveys, ApplicationDbContext db)
        {
            if (null == surveys.Template)   //若有必要则将强制填写模板类
                surveys.Template = db.SurveysTemplates.Find(surveys.TemplateId);
            return GetResultCore(surveys, db);
        }

        /// <summary>
        /// 设置症状的集合。
        /// </summary>
        /// <param name="answers"></param>
        /// <param name="dbContext"></param>
        public virtual void SetSigns(IEnumerable<SurveysAnswer> answers, DbContext dbContext)
        {
            _Answers = answers.ToList();
            var tIds = _Answers.Select(c => c.TemplateId).ToArray();
            _AnswerTemplates = dbContext.Set<SurveysAnswerTemplate>().Where(c => tIds.Contains(c.Id)).ToDictionary(c => c.IdNumber);
            _QuestionTemplates = dbContext.Set<SurveysQuestionTemplate>().Where(c => tIds.Contains(c.Id)).ToDictionary(c => c.IdNumber);
            _Numbers = new HashSet<int>(_AnswerTemplates.Keys.Concat(_QuestionTemplates.Keys));
        }

        /// <summary>
        /// 设置方剂内容。
        /// </summary>
        /// <param name="conclusion">调查结论对象。</param>
        /// <param name="prescriptiones"></param>
        [CLSCompliant(false)]
        public void SetCnPrescriptiones(SurveysConclusion conclusion, params IEnumerable<Tuple<string, decimal>>[] prescriptiones)
        {
            var coll = new List<CnPrescription>();
            int index = 1;
            foreach (var item in prescriptiones)
            {
                var drugs = item.Select(c => new CnDrug()
                {
                    Name = c.Item1,
                    Number = c.Item2,
                    Unit = "g",
                });
                var pres = new CnPrescription() { Name = $"方剂{index++}" };
                pres.Drugs.AddRange(drugs);
                coll.Add(pres);
            }
            ThingPropertyItem tpi = new ThingPropertyItem() { Name = CnPrescriptionesName };
            tpi.Value = EntityUtility.ToJson(coll);
            conclusion.ThingPropertyItems.Add(tpi);
        }
    }

    /// <summary>
    /// 常用算法帮助器类。
    /// </summary>
    public static class CnMedicineUtility
    {
        public static void get<TKey, TCode>(IEnumerable<Tuple<TKey, IEnumerable<TCode>, float>> dic, IEnumerable<TCode> codes)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signs"></param>
        /// <param name="template">问卷模板，传递空则自动生成新模板，否则更新原有数据。</param>
        public static void GetSurveysTemplateFromCnMedicineSigns(CnMedicineSignsBase signs, ref SurveysTemplate template)
        {
            if (null == template)
                template = new SurveysTemplate();
        }
    }

    /// <summary>
    /// 调查问卷的基础类。
    /// </summary>
    public class CnMedicineSignsBase
    {
        /*
            编号	问题	症状	问题类型    额外信息    说明
            1101	经前乳房胀痛	经前乳房胀痛	"选择,多重" 复诊0 文字说明
        */

        /// <summary>
        /// 构造函数。
        /// </summary>
        public CnMedicineSignsBase()
        {

        }

        /// <summary>
        /// 编号。
        /// </summary>
        [TextFieldName("编号")]
        public virtual int Number { get; set; }

        /// <summary>
        /// 问题。
        /// </summary>
        [TextFieldName("问题")]
        public virtual string Question { get; set; }

        /// <summary>
        /// 症状。
        /// </summary>
        [TextFieldName("症状")]
        public virtual string ZhengZhuang { get; set; }

        /// <summary>
        /// 问题类型。
        /// </summary>
        [TextFieldName("问题类型")]
        public virtual QuestionsKind QuestionsKind { get; set; }

        /// <summary>
        /// 额外信息。
        /// </summary>
        [TextFieldName("额外信息")]
        public virtual string UserState { get; set; }

        /// <summary>
        /// 说明。
        /// </summary>
        [TextFieldName("说明")]
        public virtual string Description { get; set; }

        /// <summary>
        /// 显示前提条件编号列表，多个编号以逗号分开，要满足所有条件才会显示。
        /// </summary>
        [TextFieldName("前提")]
        public string DisplayContions { get; set; }

        private int _OrderNum = int.MinValue;

        /// <summary>
        /// 控制显示顺序的序号，不必连续可正可负，从最小的开始显示。
        /// 默认值：<see cref="int.MinValue"/>，此时使用 <see cref="Number"/>属性的值。
        /// </summary>
        [TextFieldName("显示序列号")]
        public virtual int OrderNum
        {
            get { return _OrderNum == int.MinValue ? Number : _OrderNum; }
            set { _OrderNum = value; }
        }

    }

}
#pragma warning restore CS3021 // 由于程序集没有 CLSCompliant 特性，因此类型或成员不需要 CLSCompliant 特性
