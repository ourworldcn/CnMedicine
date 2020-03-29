using CnMedicineServer.Models;
using OW.Data.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

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
        /// 键是编号，值是对应的模板。
        /// </summary>
        Dictionary<int, SurveysAnswerTemplate> _AnswerTemplates;

        Dictionary<int, SurveysQuestionTemplate> _QuestionTemplates;

        /// <summary>
        /// 答案集合。
        /// </summary>
        public List<SurveysAnswer> Answers { get => _Answers; set => _Answers = value; }

        /// <summary>
        /// 键是编号，值是对应的模板。
        /// </summary>
        public Dictionary<int, SurveysAnswerTemplate> AnswerTemplates { get => _AnswerTemplates; set => _AnswerTemplates = value; }

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
            _AnswerTemplates = dbContext.Set<SurveysAnswerTemplate>().Where(c => tIds.Contains(c.Id)).ToDictionary(c => c.OrderNum);
            _QuestionTemplates = dbContext.Set<SurveysQuestionTemplate>().Where(c => tIds.Contains(c.Id)).ToDictionary(c => c.OrderNum);
            _Numbers = new HashSet<int>(_AnswerTemplates.Keys.Union(_QuestionTemplates.Keys));
        }

        private object _SyncLocker = new object();

        /// <summary>
        /// 获取同步使用的锁。
        /// </summary>
        public object SyncLocker { get => _SyncLocker; }
    }

    public class GaoRongrongAnalysisDataBase : CnMedicineAnalysisDataBase
    {
        public GaoRongrongAnalysisDataBase()
        {

        }

        public override void SetAnswers(IEnumerable<SurveysAnswer> answers, DbContext dbContext)
        {
            base.SetAnswers(answers, dbContext);

        }

        Dictionary<int, string> _TypeNumeber;

        /// <summary>
        /// 获取编号的类型码。
        /// </summary>
        public Dictionary<int, string> TypeNumeber
        {
            get
            {
                lock (SyncLocker)
                    if (null == _TypeNumeber)
                    {
                        _TypeNumeber = new Dictionary<int, string>();
                        foreach (var item in Numbers)
                        {
                            if (AnswerTemplates.TryGetValue(item, out SurveysAnswerTemplate surveysAnswer))
                            {
                                var tuples = EntityUtility.GetTuples(surveysAnswer.UserState);
                                if (tuples.Any(c => "类型号A" == c.Item1 && 1 == c.Item2))
                                    _TypeNumeber.Add(item, "A");
                                else if (tuples.Any(c => "类型号B" == c.Item1 && 1 == c.Item2))
                                    _TypeNumeber.Add(item, "B");
                                else if (tuples.Any(c => "类型号C" == c.Item1 && 1 == c.Item2))
                                    _TypeNumeber.Add(item, "C");
                            }
                        }
                    }
                return _TypeNumeber;
            }
        }

        /// <summary>
        /// 获取指定编号的类型号。
        /// </summary>
        /// <param name="number"></param>
        /// <returns>A参与计算且阈值为0.7；B不参与计算；C参与计算且没有阈值,未指定则返回<see cref="string.Empty"/>。</returns>
        public string GetTypeNumber(int number)
        {
            if (TypeNumeber.TryGetValue(number, out string typeNumber))
                return typeNumber.ToUpper();
            else
                return string.Empty;
        }
    }

    public abstract class CnMedicineAlgorithmBase
    {
        /// <summary>
        /// 此字符串标志<see cref="OW.OwAdditionalAttribute"/>中Name属性内容。表示其Value中内容是该类处理的调查模板Id。
        /// </summary>
        public const string SurveysTemplateIdName = "782281B8-D74B-42FA-9F32-9E8EB7B5049B";

        /// <summary>
        /// 此字符串标志<see cref="OW.OwAdditionalAttribute"/>中Name属性内容。Value的内容将被忽略，此批注放在静态初始化的函数上，函数必须签名类似static void xxx(DbContext context)。
        /// </summary>
        public const string InitializationFuncName = "175EFEDB-7561-44E5-A053-86FC03C39255";

        protected abstract SurveysConclusion GetResultCore(Surveys surveys, ApplicationDbContext db);

        public SurveysConclusion GetResult(Surveys surveys, ApplicationDbContext db)
        {
            if (null == surveys.Template)   //若有必要则将强制填写模板类
                surveys.Template = db.SurveysTemplates.Find(surveys.TemplateId);
            return GetResultCore(surveys, db);
        }
    }
}