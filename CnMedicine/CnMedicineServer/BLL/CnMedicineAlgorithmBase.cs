using CnMedicineServer.Models;
using OW;
using OW.Data.Entity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

#pragma warning disable CS3021 // 由于程序集没有 CLSCompliant 特性，因此类型或成员不需要 CLSCompliant 特性
namespace CnMedicineServer.Bll
{
    public class MatchableBase<TKey, TValue> where TKey : IComparer<TKey>
    {
        public MatchableBase()
        {

        }

        public int Count { get; set; } = 1;

        List<MatchableItemBase<TKey, TValue>> com = new List<MatchableItemBase<TKey, TValue>>();
        TValue Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="keys"></param>
        /// <returns>可能返回空集合，但不会返回null。</returns>
        public IEnumerable<IMatchableItem<TKey, TValue>> GetMatch(IEnumerable<TKey> keys)
        {
            var coll = (from tmp in com
                        let match = tmp.GetMatch(keys)
                        where match >= tmp.Threshold
                        orderby match descending
                        select ValueTuple.Create(tmp, match)).ToArray();    //获取匹配度,这里顺便确信是内存中的对象集合
            if (!coll.Any()) //若没有可能的匹配的项
                return Enumerable.Empty<MatchableItemBase<TKey, TValue>>();
            var avg = from tmp in coll
                      group tmp by tmp.Item1.Group into g
                      let avgMatch = g.Average(c => c.Item2)
                      orderby avgMatch descending
                      select ValueTuple.Create(g.Key, avgMatch); //获取组的平均匹配度

            var maxAvgMatchGroup = avg.First().Item1;   //获取平均匹配度最大的组号
            var result = coll.Where(c => c.Item1.Group == maxAvgMatchGroup).Take(Count).Select(c => c.Item1);

            return result;
        }
    }

    public interface IMatchableItem<TKey, TValue>
    {
        /// <summary>
        /// 组号。
        /// </summary>
        int Group { get; }

        /// <summary>
        /// 带权值的键。
        /// </summary>
        IEnumerable<(TKey, float)> Keys { get; }

        /// <summary>
        /// 阈值。
        /// </summary>
        float Threshold { get; }

        /// <summary>
        /// 获取或设置值数据，使用者可用该属性扩展数据。
        /// </summary>
        TValue Value { get; }
    }

    public class MatchableItemBase<TKey, TValue> : IMatchableItem<TKey, TValue>
    {
        public IEnumerable<ValueTuple<TKey, float>> Keys { get; set; }

        public int Group { get; set; }

        public float Threshold { get; set; }

        public TValue Value { get; set; }

        float? _TotalPower;
        public float TotalPower
        {
            get
            {
                if (null == _TotalPower)
                    _TotalPower = Keys.Sum(c => c.Item2);   //不包含任何元素，则此方法返回零。
                return _TotalPower.Value;
            }
        }

        /// <summary>
        /// 获取匹配度。
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public float GetMatch(IEnumerable<TKey> keys)
        {
            var coll = from key in keys
                       join tmp in Keys
                       on key equals tmp.Item1
                       select tmp;
            return coll.Sum(c => c.Item2) / TotalPower;
        }
    }

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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="powers"></param>
        /// <param name="keys"></param>
        /// <returns>返回匹配度，若参数是空集合，或没有匹配项则返回0.</returns>
        public static float GetMatch<TKey>(this IEnumerable<ValueTuple<TKey, float>> powers, IEnumerable<TKey> keys)
        {
            var coll = from key in keys
                       join tmp in powers
                       on key equals tmp.Item1
                       select tmp;
            var match = coll.Sum(c => c.Item2);
            if (Math.Abs(match) < 0.000001)
                return 0;
            return match / powers.Sum(c => c.Item2);
        }

        public static IEnumerable<TSource> GetMatchs<TSource, TKey>(this IEnumerable<TSource> src, IEnumerable<TKey> keys, Func<TSource, IEnumerable<ValueTuple<TKey, float>>> powerCreator,
            Func<TSource, int> groupCreator, Func<TSource, float> thresholdCreator)
        {
            var coll = (from tmp in src
                        let match = powerCreator(tmp).GetMatch(keys)
                        where match >= thresholdCreator(tmp)
                        orderby match descending
                        select ValueTuple.Create(tmp, match)).ToArray();    //获取匹配度
            if (!coll.Any()) //若没有可能的匹配的项
                return Enumerable.Empty<TSource>();
            var avg = from tmp in coll
                      let gn = groupCreator(tmp.Item1)
                      group tmp by gn into g
                      let avgMatch = g.Average(c => c.Item2)
                      orderby avgMatch descending
                      select ValueTuple.Create(g.Key, avgMatch); //获取组的平均匹配度

            var maxAvgMatchGroup = avg.First().Item1;   //获取平均匹配度最大的组号
            var result = coll.Where(c => groupCreator(c.Item1) == maxAvgMatchGroup).Select(c => c.Item1).Take(1);
            return result;
        }

        public static float GetMatch<TKey, TValue>(this IMatchableItem<TKey, TValue> source, IEnumerable<TKey> keys)
        {
            var coll = from tmp in keys.Distinct()  //不考虑重复输入的匹配
                       join mi in source.Keys   //IMatchableItem.Keys中的重复项被认为有意义
                       on tmp equals mi.Item1
                       select mi;
            return coll.Sum(c => c.Item2) / source.Keys.Sum(c => c.Item2);
        }

        /// <summary>
        /// 获取符合匹配阈值要求的可匹配对象，此方法通过使用延迟执行实现。
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="source"></param>
        /// <param name="keys">自动去除重复项。</param>
        /// <param name="result">匹配的项会调用此委托生成结果元素。</param>
        /// <returns></returns>
        public static IEnumerable<TResult> GetMatches<TKey, TValue, TResult>(this IEnumerable<IMatchableItem<TKey, TValue>> source, IEnumerable<TKey> keys,
            Func<IMatchableItem<TKey, TValue>, float, TResult> result)
        {
            var coll = from mi in source
                       from keyAndPower in mi.Keys
                       join inKey in keys.Distinct()  //避免输入重复
                       on keyAndPower.Item1 equals inKey
                       group keyAndPower by mi into g
                       let totalMatch = g.Key.Keys.Sum(c => c.Item2)   //总计匹配值
                       where totalMatch != 0
                       let match = g.Sum(c => c.Item2) / totalMatch    //匹配度
                       where match >= g.Key.Threshold
                       select result(g.Key, match);
            return coll;
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

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    sealed class CnMedicineAlgorithmAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        readonly string _CnName;

        readonly string _DataFilePath;

        // This is a positional argument
        public CnMedicineAlgorithmAttribute(string cnName, string dataFilePath = null)
        {
            _CnName = cnName;

            // TODO: Implement code here
            if (string.IsNullOrEmpty(dataFilePath))
                dataFilePath = $"Content/{cnName}";
            _DataFilePath = dataFilePath;
        }

        public string CnName
        {
            get { return _CnName; }
        }

        public string DataFilePath
        {
            get { return _DataFilePath; }
        }
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
        /// <param name="context">访问的数据上下文。</param>
        /// <param name="signs">症状数据集合。</param>
        /// <param name="algorithmType"></param>
        /// <exception cref="InvalidOperationException">algorithmType 类型没有合适的OwAdditionalAttribute批注。</exception>
        public static void InitializeCore2(DbContext context, IEnumerable<CnMedicineSignsBase> signs, Type algorithmType)
        {
            #region 获取专病的Id。
            var idAttr = algorithmType.GetCustomAttributes(true).OfType<OwAdditionalAttribute>().FirstOrDefault(c => c.Name == SurveysTemplateIdName);
            if (null == idAttr)
                throw new InvalidOperationException($"{algorithmType}类型没有合适的{typeof(OwAdditionalAttribute)}批注。");
            var SurveysTemplateIdString = idAttr.Value;
            var survId = Guid.Parse(SurveysTemplateIdString);
            #endregion 获取专病的Id。

            #region 处理问卷模板对象
            var surveysTemplate = context.Set<SurveysTemplate>().Find(survId);
            if (null == surveysTemplate)  //若没有模板对象
            {
                surveysTemplate = new SurveysTemplate()
                {
                    Id = survId,
                    //Name = "经行乳房胀痛",
                    //UserState = "支持复诊0",
                    Questions = new List<SurveysQuestionTemplate>(),
                    //Description = "经行乳房痛：每值经前或经期乳房作胀,甚至胀满疼痛,或乳头痒痛者,称“经行乳房痛”。包含乳腺增生、乳腺纤维瘤等乳腺疾病的伴发症状。",
                };
                context.Set<SurveysTemplate>().Add(surveysTemplate);
            }
            var questions = surveysTemplate.Questions;    //问题对象集合
            #endregion 处理问卷模板对象

            var srcColl = (from tmp in signs
                           group tmp by tmp.Question).ToArray();

            #region 处理问题对象

            List<SurveysQuestionTemplate> removes = new List<SurveysQuestionTemplate>();
            List<IGrouping<string, CnMedicineSignsBase>> adds = new List<IGrouping<string, CnMedicineSignsBase>>();
            List<Tuple<IGrouping<string, CnMedicineSignsBase>, SurveysQuestionTemplate>> modifies = new List<Tuple<IGrouping<string, CnMedicineSignsBase>, SurveysQuestionTemplate>>();
            questions.GetMergeInfo(c => c.QuestionTitle, srcColl, c => c.Key, removes, adds, modifies);
            //删除问题对象
            context.Set<SurveysQuestionTemplate>().RemoveRange(removes);
            //增加问题对象
            List<SurveysAnswerTemplate> removedAnswers = new List<SurveysAnswerTemplate>();
            surveysTemplate.Questions.AddRange(adds.Select(c =>
            {
                var sqt = new SurveysQuestionTemplate()
                {
                    SurveysTemplateId = surveysTemplate.Id,
                };
                sqt.Merge(c, removedAnswers);
                return sqt;
            }));
            //更新问题对象
            foreach (var item in modifies)
            {
                item.Item2.Merge(item.Item1, removedAnswers);
            }
            context.Set<SurveysAnswerTemplate>().RemoveRange(removedAnswers);

            //surveysTemplate.Questions.AddRange(adds.Select(c => new SurveysQuestionTemplate()));
            var lst = surveysTemplate.Questions.Where(c => c.Kind == 0).ToList();
            #endregion 处理问题对象
        }

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

            //if (null != tmp)
            //    return;

            var fullPath = System.Web.HttpContext.Current.Server.MapPath(signsFileName);    //本机全路径
            var path = Path.GetDirectoryName(fullPath); //路径名
            List<CnMedicineSignsBase> signs;
            using (var tdb = new TextFileContext(path) { IgnoreQuotes = true, })
            {
                var fileName = Path.GetFileName(fullPath);
                signs = tdb.GetList<CnMedicineSignsBase>(fileName);
            }
            InitializeCore2(context, signs, algorithmType);
            return;
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

        /// <summary>
        /// 获取病名，可能是空引用。
        /// </summary>
        /// <param name="type">算法类的类型对象。</param>
        /// <returns>病名，可能是空引用。</returns>
        protected static string GetCnName(Type type)
        {
            return TypeDescriptor.GetAttributes(type).OfType<CnMedicineAlgorithmAttribute>().FirstOrDefault()?.CnName;
        }

        /// <summary>
        /// 获取数据文件的路径，如"Content/病名"
        /// </summary>
        /// <param name="type">算法类的类型对象。</param>
        /// <returns></returns>
        protected static string GetDataFilePath(Type type)
        {
            var result = TypeDescriptor.GetAttributes(type).OfType<CnMedicineAlgorithmAttribute>().FirstOrDefault()?.DataFilePath;
            if (string.IsNullOrEmpty(result))
                result = $"Content/{GetCnName(type)}";
            return result;
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

        public Dictionary<int, SurveysQuestionTemplate> QuestionTemplates { get => _QuestionTemplates; }
        public List<SurveysAnswer> Answers { get => _Answers; }

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

        /// <summary>
        /// 将指定的 CnMedicineSignsBase 对象的集合组合并到当前的 SurveysQuestionTemplate 中。
        /// </summary>
        /// <param name="surveysQuestionTemplate"></param>
        /// <param name="signs">调试状态下若不是同一个问题的数据项会引发断言异常。发布状态下会导致未知问题。</param>
        /// <param name="removedAnswers">返回时移除的答案项将被添加到此参数(此参数已有元素不会改变)，如果为null,则忽略此参数。</param>
        public static void Merge(this SurveysQuestionTemplate surveysQuestionTemplate, IEnumerable<CnMedicineSignsBase> signs, List<SurveysAnswerTemplate> removedAnswers = null)
        {
            var first = signs.First();
            Debug.Assert(signs.All(c => c.Question == first.Question)); //确定所有sign都是同一个问题的数据项
            surveysQuestionTemplate.Kind = first.QuestionsKind;
            surveysQuestionTemplate.IdNumber = first.Number;
            surveysQuestionTemplate.QuestionTitle = first.Question;
            surveysQuestionTemplate.UserState = "";
            surveysQuestionTemplate.OrderNum = first.OrderNum;
            surveysQuestionTemplate.Description = first.Description;
            surveysQuestionTemplate.DisplayConditions = first.DisplayContions;
            if (null == surveysQuestionTemplate.Answers)
                surveysQuestionTemplate.Answers = new List<SurveysAnswerTemplate>();

            List<SurveysAnswerTemplate> removes = new List<SurveysAnswerTemplate>();
            List<CnMedicineSignsBase> adds = new List<CnMedicineSignsBase>();
            List<Tuple<CnMedicineSignsBase, SurveysAnswerTemplate>> modifies = new List<Tuple<CnMedicineSignsBase, SurveysAnswerTemplate>>();
            surveysQuestionTemplate.Answers.GetMergeInfo(c => c.AnswerTitle, signs, c => c.ZhengZhuang, removes, adds, modifies);
            //删除答案对象
            removedAnswers?.AddRange(removes);
            foreach (var item in removes)
                surveysQuestionTemplate.Answers.Remove(item);
            //增加新答案对象
            surveysQuestionTemplate.Answers.AddRange(adds.Select(c =>
            {
                SurveysAnswerTemplate sat = new SurveysAnswerTemplate() { SurveysQuestionTemplateId = surveysQuestionTemplate.Id };
                sat.Merge(c);
                return sat;
            }));
            //更新答案对象
            foreach (var item in modifies)
            {
                var dest = item.Item2;
                var src = item.Item1;
                dest.Merge(src);
            }
        }

        /// <summary>
        /// 将指定的 CnMedicineSignsBase 对象组合并到当前的 SurveysAnswerTemplate 中。
        /// </summary>
        /// <param name="surveysAnswerTemplate"></param>
        /// <param name="sign"></param>
        public static void Merge(this SurveysAnswerTemplate surveysAnswerTemplate, CnMedicineSignsBase sign = null)
        {
            surveysAnswerTemplate.AnswerTitle = sign.ZhengZhuang;
            surveysAnswerTemplate.IdNumber = sign.Number;
            surveysAnswerTemplate.UserState = $"编号{sign.Number}";
            surveysAnswerTemplate.DisplayConditions = sign.DisplayContions;
            surveysAnswerTemplate.OrderNum = sign.OrderNum;
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
