
using CnMedicineServer.Models;
using OW.Data.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Helpers;

namespace CnMedicineServer.Bll
{
    public static class InsomniaExtensions
    {
        public static Dictionary<string, int> GetProperties(this SurveysAnswerTemplate surveysAnswerTemplate)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            return result;
        }

        /// <summary>
        /// 获取一个这是第几次诊断。
        /// </summary>
        /// <param name="surveys"></param>
        /// <returns>0初诊，1复诊。</returns>
        public static int GetDiagnosisCount(this Surveys surveys)
        {
            var ary = EntityUtil.GetTuples(surveys.UserState);
            var flag = ary.FirstOrDefault(c => c.Item1 == "诊次")?.Item2 ?? 0;
            return (int)flag;
        }

        /// <summary>
        /// 设置诊断次数。
        /// </summary>
        /// <param name="surveys"></param>
        /// <param name="count">0初诊，1复诊。</param>
        public static void SetDiagnosisCount(this Surveys surveys, int count)
        {
            var ary = EntityUtil.GetTuples(surveys.UserState);
            var flag = ary.FirstOrDefault(c => c.Item1 == "诊次");
            if (null != flag)
                ary.Remove(flag);
            flag = Tuple.Create("诊次", (decimal)count);
            ary.Add(flag);
            surveys.UserState = string.Join(",", ary.Select(c => $"{c.Item1}{c.Item2.ToString()}"));
        }
    }

    /// <summary>
    /// 失眠专病的内部数据。
    /// 有大量冗余数据，供病患参见，这些中间数据保留是有一定意义的。
    /// </summary>
    public class InsomniaAnalysisData
    {
        /// <summary>
        /// 根据症状，填写脏腑和症候评分。
        /// </summary>
        /// <param name="conversion11s">引用的评分表1的项集合。</param>
        /// <param name="conversion12s">输出采用的评分表2的项集合。传递空将导致不生成也使用评分表2.</param>
        /// <param name="cnVisceral">脏腑评分集合。</param>
        /// <param name="cnPhenomenon">证型评分集合。</param>
        public static void Fill(IEnumerable<InsomniaConversion11> conversion11s, List<InsomniaConversion12> conversion12s,
            List<Tuple<string, decimal>> cnVisceral, List<Tuple<string, decimal>> cnPhenomenon)
        {
            HashSet<string> hsViscerals = new HashSet<string>(InsomniaConversion11.DefaultCollection.SelectMany(c => c.CnVisceralProperties).Select(c => c.Item1));   //如果 collection 包含重复的元素，该集将包含一个唯一的每个元素。
            HashSet<string> hsPhenomenons = new HashSet<string>(InsomniaConversion11.DefaultCollection.SelectMany(c => c.CnPhenomenonProperties).Select(c => c.Item1));

            HashSet<string> hsNumbers = new HashSet<string>(conversion11s.Select(c => c.CnSymptomNumber));  //所有编号
            List<InsomniaConversion12> conv12s = null == conversion12s ? new List<InsomniaConversion12>() : InsomniaConversion12.DefaultCollection.Where(c => hsNumbers.IsSupersetOf(c.Numbers)).ToList(); //引用的评分表2
            conversion12s?.AddRange(conv12s);

            //脏腑评分
            var VisceralScroes = conversion11s.SelectMany(c => c.CnVisceralProperties).Concat(conv12s.SelectMany(c => c.CnScoreProperties)).Where(c => hsViscerals.Contains(c.Item1));
            var visceralAry = VisceralScroes.GroupBy(c => c.Item1).Select(c => Tuple.Create(c.Key, c.Sum(c1 => c1.Item2)));
            cnVisceral.AddRange(visceralAry);

            //症候评分
            var PhenomenonScores = conversion11s.SelectMany(c => c.CnPhenomenonProperties).Concat(conv12s.SelectMany(c => c.CnScoreProperties)).Where(c => hsPhenomenons.Contains(c.Item1));
            var phenomennoAry = PhenomenonScores.GroupBy(c => c.Item1).Select(c => Tuple.Create(c.Key, c.Sum(c1 => c1.Item2)));
            cnPhenomenon.AddRange(phenomennoAry);
        }

        /// <summary>
        /// 获取脏腑-证型组合。目前算法是取两个集合的评分最高的项，作为新集合，求笛卡尔积。
        /// </summary>
        /// <param name="cnVisceral"></param>
        /// <param name="cnPhenomenon"></param>
        /// <returns>获取的脏腑-证型组合的集合。</returns>
        public static List<Tuple<string, string, int>> GetCnVPs(List<Tuple<string, decimal>> cnVisceral, List<Tuple<string, decimal>> cnPhenomenon)
        {
            List<Tuple<string, string, int>> result = new List<Tuple<string, string, int>>();
            cnVisceral.Sort((l, r) => -decimal.Compare(l.Item2, r.Item2));
            var maxVisceralScore = cnVisceral.FirstOrDefault()?.Item2 ?? 0;

            cnPhenomenon.Sort((l, r) => -decimal.Compare(l.Item2, r.Item2));
            var maxPhenomenonScore = cnPhenomenon.FirstOrDefault()?.Item2 ?? 0;

            var coll = from tmp1 in cnVisceral.TakeWhile(c => c.Item2 >= maxVisceralScore)
                       from tmp2 in cnPhenomenon.TakeWhile(c => c.Item2 >= maxPhenomenonScore)
                       select Tuple.Create(tmp1.Item1, tmp2.Item1);
            var count = coll.Count();
            result.AddRange(coll.Select(c => Tuple.Create(c.Item1, c.Item2, count)));
            return result;
        }

        public static void Fill(IEnumerable<Tuple<string, string>> src, List<InsomniaCnDrugConversion> drugs)
        {

        }

        /// <summary>
        /// 年龄。
        /// </summary>
        public decimal Age { get; set; }

        /// <summary>
        /// 性别。
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// 姓名。
        /// </summary>
        public string Name { get; set; }

        public List<InsomniaConversion11> InsomniaConversion11s { get; set; } = new List<InsomniaConversion11>();

        /// <summary>
        /// 治疗无效条目。
        /// </summary>
        public List<InsomniaConversion11> Invalid11s { get; set; } = new List<InsomniaConversion11>();


        List<InsomniaCnDrugConversion> _InvalidDrugs;

        /// <summary>
        /// 针对无效项的药物输出。
        /// </summary>
        public List<InsomniaCnDrugConversion> InvalidDrugs
        {
            get
            {
                if (null == _InvalidDrugs)
                {
                    List<Tuple<string, decimal>> cnVisceral = new List<Tuple<string, decimal>>();
                    List<Tuple<string, decimal>> cnPhenomenon = new List<Tuple<string, decimal>>();
                    Fill(Invalid11s, null, cnVisceral, cnPhenomenon);
                    var cnVPs = GetCnVPs(cnVisceral, cnPhenomenon);

                    var coll = InsomniaCnDrugConversion.DefaultCollection.Join(cnVPs, c => Tuple.Create(c.CnMedicineVisceral, c.CnMedicinePhenomenon), c => Tuple.Create(c.Item1, c.Item2), (c, c1) => c);
                    _InvalidDrugs = new List<InsomniaCnDrugConversion>(coll);
                }
                return _InvalidDrugs;
            }
        }

        public int MyProperty { get; set; }
        List<InsomniaConversion12> _InsomniaConversion12s;

        /// <summary>
        /// 采用的评分表2的项集合。
        /// </summary>
        public List<InsomniaConversion12> InsomniaConversion12s
        {
            get
            {
                lock (InsomniaConversion11s)
                    if (null == _InsomniaConversion12s)
                        RefreshInsomniaConversion12();
                return _InsomniaConversion12s;
            }
        }

        /// <summary>
        /// 刷新采用的评分表2的项。调用此方法重新计算使用的评分表2的项。
        /// </summary>
        private void RefreshInsomniaConversion12()
        {
            if (null == _InsomniaConversion12s)
                _InsomniaConversion12s = new List<InsomniaConversion12>();
            else
                _InsomniaConversion12s.Clear();
            HashSet<string> hs = new HashSet<string>(InsomniaConversion11s.Select(c => c.CnSymptomNumber).Distinct());
            _InsomniaConversion12s.AddRange(InsomniaConversion12.DefaultCollection.Where(c => hs.IsSupersetOf(c.Numbers)));
        }

        private List<InsomniaCnDrugConversion> _InsomniaCnDrugConversions;

        /// <summary>
        /// 采用的药物输出表的项。注意这里没有加味问题。
        /// 可并发访问。
        /// </summary>
        public List<InsomniaCnDrugConversion> InsomniaCnDrugConversions
        {
            get
            {
                lock (this)
                    if (null == _InsomniaCnDrugConversions)
                    {
                        RefreshInsomniaCnDrugConversions();
                    }
                return _InsomniaCnDrugConversions;
            }
        }

        /// <summary>
        /// 最大脏腑。
        /// </summary>
        public List<Tuple<string, decimal>> MaxVicerals { get; set; }

        /// <summary>
        /// 最大症候。
        /// </summary>
        public List<Tuple<string, decimal>> MaxPhenomenons { get; set; }

        public List<Tuple<string, string, int>> _CnVP;

        /// <summary>
        /// 诊断脏腑-证型。
        /// </summary>
        public List<Tuple<string, string, int>> CnVP
        {
            get
            {
                if (null == _CnVP)
                {

                }
                return _CnVP;
            }
        }

        /// <summary>
        /// 刷新药物输出表的项。
        /// </summary>
        private void RefreshInsomniaCnDrugConversions()
        {
            if (null == _InsomniaCnDrugConversions)
                _InsomniaCnDrugConversions = new List<InsomniaCnDrugConversion>();
            else
                _InsomniaCnDrugConversions.Clear();
            HashSet<string> hsViscerals = new HashSet<string>(InsomniaConversion11s.SelectMany(c => c.CnVisceralProperties).Select(c => c.Item1));   //如果 collection 包含重复的元素，该集将包含一个唯一的每个元素。
            HashSet<string> hsPhenomenons = new HashSet<string>(InsomniaConversion11s.SelectMany(c => c.CnPhenomenonProperties).Select(c => c.Item1));
            //脏腑评分
            var VisceralScroes = InsomniaConversion11s.SelectMany(c => c.CnVisceralProperties).Concat(InsomniaConversion12s.SelectMany(c => c.CnScoreProperties)).Where(c => hsViscerals.Contains(c.Item1));
            var visceralAry = VisceralScroes.GroupBy(c => c.Item1).Select(c => Tuple.Create(c.Key, c.Sum(c1 => c1.Item2))).OrderByDescending(c => c.Item2).ToArray();
            var maxVisceral = visceralAry.FirstOrDefault()?.Item2 ?? 0;
            MaxVicerals = visceralAry.Where(c => c.Item2 >= maxVisceral).ToList();
            //症候评分
            var PhenomenonScores = InsomniaConversion11s.SelectMany(c => c.CnPhenomenonProperties).Concat(InsomniaConversion12s.SelectMany(c => c.CnScoreProperties)).Where(c => hsPhenomenons.Contains(c.Item1));
            var phenomennoAry = PhenomenonScores.GroupBy(c => c.Item1).Select(c => Tuple.Create(c.Key, c.Sum(c1 => c1.Item2))).OrderByDescending(c => c.Item2).ToArray();
            var maxPhenomenon = phenomennoAry.FirstOrDefault()?.Item2 ?? 0;
            MaxPhenomenons = phenomennoAry.Where(c => c.Item2 >= maxPhenomenon).ToList();
            //数字逻辑上可能出现例如:心，肝并列第一；且火亢，阴虚并列第一。此时药物输出算作4个么？心-火亢，心-阴虚，肝-火亢，肝-阴虚,结算笛卡尔积
            var coll = from tmp in MaxVicerals
                       from tmp1 in MaxPhenomenons
                       select Tuple.Create(tmp.Item1, tmp1.Item1);  //无论如何先获取笛卡尔积
            _InsomniaCnDrugConversions.AddRange(InsomniaCnDrugConversion.DefaultCollection.Join(coll, c => Tuple.Create(c.CnMedicineVisceral, c.CnMedicinePhenomenon), c => c, (drug, c) => drug));

        }

        private List<InsomniaCnDrugConversion2> _InsomniaCnDrugConversion2s;

        /// <summary>
        /// 采用的药物加味表。
        /// </summary>
        public List<InsomniaCnDrugConversion2> InsomniaCnDrugConversion2s
        {
            get
            {
                if (null == _InsomniaCnDrugConversion2s)
                {
                    _InsomniaCnDrugConversion2s = new List<InsomniaCnDrugConversion2>();
                    HashSet<string> hs = new HashSet<string>(InsomniaConversion11s.Select(c => c.CnSymptomNumber));
                    _InsomniaCnDrugConversion2s.AddRange(InsomniaCnDrugConversion2.DefaultCollection.Where(c => hs.IsSupersetOf(c.Numbers)));
                }
                return _InsomniaCnDrugConversion2s;
            }
        }

        private List<Tuple<string, decimal>> _CnDrugBase;

        /// <summary>
        /// 药物输出基准。未矫正剂量
        /// </summary>
        public List<Tuple<string, decimal>> CnDrugBase
        {
            get
            {
                if (null == _CnDrugBase)
                {
                    var drugs = InsomniaCnDrugConversions;
                    _CnDrugBase = new List<Tuple<string, decimal>>();
                    var count = MaxVicerals.Count * MaxVicerals.Count;
                    if (1 == count)    //唯一诊断
                    {
                        var coll = InsomniaCnDrugConversions.SelectMany(c => c.CnDrugProperties1).Concat(InsomniaCnDrugConversion2s.SelectMany(c => c.CnDrugProperties)).Concat(InvalidDrugs.SelectMany(c => c.CnDrugProperties2))
                            .GroupBy(c => c.Item1).Select(c => Tuple.Create(c.Key, c.Max(c1 => c1.Item2)));
                        _CnDrugBase.AddRange(coll);
                    }
                    else if (2 == count)   //两个并列诊断
                    {
                        var coll = InsomniaCnDrugConversions.SelectMany(c => c.CnDrugProperties3).Concat(InsomniaCnDrugConversion2s.SelectMany(c => c.CnDrugProperties)).Concat(InvalidDrugs.SelectMany(c => c.CnDrugProperties2))
                            .GroupBy(c => c.Item1).Select(c => Tuple.Create(c.Key, c.Max(c1 => c1.Item2)));
                        _CnDrugBase.AddRange(coll);
                    }
                    else if (2 < count)    //3个或更多诊断
                    {
                        var coll = InsomniaCnDrugConversions.SelectMany(c => c.CnDrugProperties2).Concat(InsomniaCnDrugConversion2s.SelectMany(c => c.CnDrugProperties)).Concat(InvalidDrugs.SelectMany(c => c.CnDrugProperties2))
                            .GroupBy(c => c.Item1).Select(c => Tuple.Create(c.Key, c.Max(c1 => c1.Item2)));
                        _CnDrugBase.AddRange(coll);
                    }
                }
                return _CnDrugBase;
            }
        }

        private List<Tuple<string, decimal>> _CnDrugResult;

        /// <summary>
        /// 最终输出的药物。
        /// </summary>
        public List<Tuple<string, decimal>> CnDrugResult
        {
            get
            {
                if (null == _CnDrugResult)
                {
                    var factor = InsomniaCnDrugCorrection.DefaultCollection.OrderBy(c => c.Age).LastOrDefault(c => Age < c.Age)?.Factor ?? 1;
                    var coll = CnDrugBase.Select(c => Tuple.Create(c.Item1, c.Item2 * factor));
                    _CnDrugResult = new List<Tuple<string, decimal>>(coll);
                }
                return _CnDrugResult;
            }
        }

        /// <summary>
        /// 获取诊断的详细信息。
        /// </summary>
        /// <returns></returns>
        public string GetDescription()
        {
            StringBuilder sb = new StringBuilder();
            string result;
            sb.AppendLine($"诊断： ({string.Join(";", InsomniaConversion11s.Select(c => c.ToString()))}) ");
            sb.AppendLine($"脏腑评分:{string.Join(";", MaxVicerals.Select(c => c.ToString()))};症候评分:{string.Join(";", MaxPhenomenons.Select(c => c.ToString()))}");
            sb.AppendLine($"评分表2:{string.Join(";", InsomniaConversion12s.Select(c => c.ToString()))}");
            sb.AppendLine($"药物加味:{string.Join(";", InsomniaCnDrugConversion2s.Select(c => c.ToString()))}");
            sb.AppendLine($"无效项:{string.Join(";", Invalid11s.Select(c => c.ToString()))}");
            result = sb.ToString();
            return result;
        }
    }

    /// <summary>
    /// 封装失眠的算法。
    /// </summary>
    public class InsomniaAlgorithm : CnMedicineAlgorithmBase
    {

        /// <summary>
        /// 获取初诊结果。
        /// </summary>
        /// <param name="surveys"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        protected override SurveysConclusion GetResultCore(Surveys surveys, ApplicationDbContext db)
        {
            SurveysConclusion result = new SurveysConclusion() { SurveysId = surveys.Id, };
            db.Set<SurveysAnswerTemplate>().Load();
            var answerTemplates = db.Set<SurveysAnswerTemplate>();
            var coll = GetFirstCore(surveys, db);
            result.Conclusion = string.Join(",", coll.CnDrugResult.Select(c => $"{c.Item1}{c.Item2}"));
            result.ExtendedInfomation = coll.GetDescription();
            result.Description = $"{string.Join(",", coll.InsomniaCnDrugConversions.Select(c => c.CnMedicineConclusions).Distinct())}";
            if (string.IsNullOrWhiteSpace(result.Description))
                result.Description = "(无有效病症输入)";
            return result;
        }

        /// <summary>
        /// 获取最初症状的结构化数据。
        /// </summary>
        /// <param name="surveys"></param>
        /// <param name="db"></param>
        InsomniaAnalysisData GetFirstCore(Surveys surveys, ApplicationDbContext db)
        {
            InsomniaAnalysisData result = new InsomniaAnalysisData();
            var answerTemplates = db.Set<SurveysAnswerTemplate>();
            var quesetions = db.Set<SurveysQuestionTemplate>();
            quesetions.Load();
            var tb1 = db.Set<InsomniaConversion11>();
            tb1.Load();
            var answers = surveys.SurveysAnswers;
            foreach (var answer in answers)
            {
                var answerTemplate = answerTemplates.Find(answer.TemplateId);
                if (null == answerTemplate) //若不是选择题
                {
                    var question = quesetions.Find(answer.TemplateId);
                    if (null == quesetions)
                        continue;
                    switch (question.QuestionTitle)
                    {
                        case "姓名":
                            result.Name = answer.Guts;
                            break;
                        case "性别":
                            result.Gender = answer.Guts;
                            break;
                        case "年龄":
                            if (decimal.TryParse(answer.Guts, out decimal age))
                                result.Age = age;
                            break;
                        default:
                            break;
                    }
                    continue;
                }
                //选择题
                var props = EntityUtil.GetTuples(answerTemplate.UserState);
                var number = props.FirstOrDefault(c => c.Item1 == "编号");
                if (null == number)
                    continue;
                var conv11 = InsomniaConversion11.DefaultCollection.FirstOrDefault(c => c.CnSymptomNumber == number.Item2.ToString());
                if (null == conv11)
                    continue;

                result.InsomniaConversion11s.Add(conv11);

                var propIns = EntityUtil.GetTuples(answer.UserState);
                var invalid = propIns.FirstOrDefault(c => c.Item1 == "无效");
                if (null != invalid && invalid.Item2 != 0)   //若存在无效条目
                {
                    result.Invalid11s.Add(conv11);
                }
            }
            return result;
        }

    }
}
