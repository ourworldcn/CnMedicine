
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
    public static class RhinitisExtensions
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
            // return (int)flag;
            return 0;//鼻炎全算初诊
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
    /// 鼻炎专病的内部数据。
    /// 有大量冗余数据，供病患参见，这些中间数据保留是有一定意义的。
    /// </summary>
    public class RhinitisAnalysisData
    {
        /// <summary>
        /// 根据症状，填写脏腑、症候和病机评分。
        /// </summary>
        /// <param name="conversions">引用的评分表的项集合。</param>
        /// <param name="cnVisceral">脏腑评分集合。</param>
        /// <param name="cnPhenomenon">证型评分集合。</param>
        /// <param name="CnPathogen">病机评分集合。</param>
        public static void Fill(IEnumerable<RhinitisConversion> conversions,
            List<Tuple<string, decimal>> cnVisceral, List<Tuple<string, decimal>> cnPhenomenon,
            List<Tuple<string, decimal>> CnPathogen)
        {
            HashSet<string> hsViscerals = new HashSet<string>(RhinitisConversion.DefaultCollection.SelectMany(c => c.CnVisceralProperties).Select(c => c.Item1));   
            //如果 collection 包含重复的元素，该集将包含一个唯一的每个元素。
            HashSet<string> hsPhenomenons = new HashSet<string>(RhinitisConversion.DefaultCollection.SelectMany(c => c.CnPhenomenonProperties).Select(c => c.Item1));
            HashSet<string> hsPathogens = new HashSet<string>(RhinitisConversion.DefaultCollection.SelectMany(c => c.CnPathogenProperties).Select(c => c.Item1));

            HashSet<string> hsNumbers = new HashSet<string>(conversions.Select(c => c.CnSymptomNumber));  //所有编号

            //脏腑评分
            var VisceralScroes = conversions.SelectMany(c => c.CnVisceralProperties);
            var visceralAry = VisceralScroes.GroupBy(c => c.Item1).Select(c => Tuple.Create(c.Key, c.Sum(c1 => c1.Item2)));
            cnVisceral.AddRange(visceralAry);

            //症候评分
            var PhenomenonScores = conversions.SelectMany(c => c.CnPhenomenonProperties);
            var phenomennoAry = PhenomenonScores.GroupBy(c => c.Item1).Select(c => Tuple.Create(c.Key, c.Sum(c1 => c1.Item2)));
            cnPhenomenon.AddRange(phenomennoAry);

            //病机评分
            var PathogenScores = conversions.SelectMany(c => c.CnPathogenProperties);
            var pathogennoAry = PathogenScores.GroupBy(c => c.Item1).Select(c => Tuple.Create(c.Key, c.Sum(c1 => c1.Item2)));
            CnPathogen.AddRange(pathogennoAry);
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

        public static void Fill(IEnumerable<Tuple<string, string>> src, List<RhinitisCnDrugConversion> drugs)
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

        public List<RhinitisConversion> RhinitisConversions { get; set; } = new List<RhinitisConversion>();

        private List<RhinitisCnDrugConversion> _RhinitisCnDrugConversions;

        /// <summary>
        /// 采用的药物输出表的项。注意这里没有病机药物问题。
        /// 可并发访问。
        /// </summary>
        public List<RhinitisCnDrugConversion> RhinitisCnDrugConversions
        {
            get
            {
                lock (this)
                    if (null == _RhinitisCnDrugConversions)
                    {
                        RefreshRhinitisCnDrugConversions();
                    }
                return _RhinitisCnDrugConversions;
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

        /// <summary>
        /// 病机数量，不够得分不输出药物。
        /// </summary>
        public List<Tuple<string, decimal>> Pathogens { get; set; }

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
        private void RefreshRhinitisCnDrugConversions()
        {
            if (null == _RhinitisCnDrugConversions)
                _RhinitisCnDrugConversions = new List<RhinitisCnDrugConversion>();
            else
                _RhinitisCnDrugConversions.Clear();
            HashSet<string> hsViscerals = new HashSet<string>(RhinitisConversions.SelectMany(c => c.CnVisceralProperties).Select(c => c.Item1));   //如果 collection 包含重复的元素，该集将包含一个唯一的每个元素。
            HashSet<string> hsPhenomenons = new HashSet<string>(RhinitisConversions.SelectMany(c => c.CnPhenomenonProperties).Select(c => c.Item1));
            HashSet<string> hsPathogens = new HashSet<string>(RhinitisConversion.DefaultCollection.SelectMany(c => c.CnPathogenProperties).Select(c => c.Item1));

            ///脏腑评分
            var VisceralScroes = RhinitisConversions.SelectMany(c => c.CnVisceralProperties);
            var visceralAry = VisceralScroes.GroupBy(c => c.Item1).Select(c => Tuple.Create(c.Key, c.Sum(c1 => c1.Item2))).OrderByDescending(c => c.Item2).ToArray();
            var maxVisceral = visceralAry.FirstOrDefault()?.Item2 ?? 0;

            ///一定要和第二分值比较，鼻炎取前两分值，并列第一时，取并列第一值，不再取第二值
            decimal sendVisceral = 0;
            if (visceralAry.Length > 1)
            { 
              sendVisceral = visceralAry[1]?.Item2 ?? 0;//第二个脏腑分 
            };
            if (sendVisceral == maxVisceral) //并列第一
            {
                MaxVicerals = visceralAry.Where(c => c.Item2 >= maxVisceral).ToList();
            }
            else
            {
                MaxVicerals = visceralAry.Where(c => c.Item2 >= sendVisceral).ToList();
            }


            
            //症候评分
            var PhenomenonScores = RhinitisConversions.SelectMany(c => c.CnPhenomenonProperties);
            var phenomennoAry = PhenomenonScores.GroupBy(c => c.Item1).Select(c => Tuple.Create(c.Key, c.Sum(c1 => c1.Item2))).OrderByDescending(c => c.Item2).ToArray();
            var maxPhenomenon = phenomennoAry.FirstOrDefault()?.Item2 ?? 0;

            decimal sendPhenomenon = 0;
            if (phenomennoAry.Length > 1)
            {
                sendPhenomenon = phenomennoAry[1]?.Item2 ?? 0;//第二个症候分 
            };
            if (sendPhenomenon == maxPhenomenon)
            {
                MaxPhenomenons = phenomennoAry.Where(c => c.Item2 >= maxPhenomenon).ToList();
                //数字逻辑上可能出现例如:心，肝并列第一；且风热，痰热并列第一。此时药物输出算作4个么？心-风热，心-痰热，肝-风热，肝-痰热,结算笛卡尔积
            }
            else
            {
                MaxPhenomenons = phenomennoAry.Where(c => c.Item2 >= sendPhenomenon).ToList();

            }

            //病机评分
            var PathogenScores = RhinitisConversions.SelectMany(c => c.CnPathogenProperties);
            var pathogennoAry = PathogenScores.GroupBy(c => c.Item1).Select(c => Tuple.Create(c.Key, c.Sum(c1 => c1.Item2))).OrderByDescending(c => c.Item2).ToArray();
            Pathogens = pathogennoAry.Where(c => c.Item2 >= 0).ToList();


            var coll = from tmp in MaxVicerals
                       from tmp1 in MaxPhenomenons
                       select Tuple.Create(tmp.Item1, tmp1.Item1);  //无论如何先获取笛卡尔积
            _RhinitisCnDrugConversions.AddRange(RhinitisCnDrugConversion.DefaultCollection.Join(coll, c => Tuple.Create(c.CnMedicineVisceral, c.CnMedicinePhenomenon), c => c, (drug, c) => drug));

        }

        private List<RhinitisCnDrugPathogen> _RhinitisCnDrugPathogens;

        /// <summary>
        /// 病机药物输出表。
        /// </summary>
        public List<RhinitisCnDrugPathogen> RhinitisCnDrugPathogens
        {
            get
            {
                if (null == _RhinitisCnDrugPathogens)
                {
                    _RhinitisCnDrugPathogens = new List<RhinitisCnDrugPathogen>();
                    HashSet<string> hsCnPathogens = new HashSet<string>(RhinitisConversions.SelectMany(c => c.CnPathogenProperties).Select(c => c.Item1));

                    //症候评分
                    var PathogenScores = RhinitisConversions.SelectMany(c => c.CnPathogenProperties);
                    var pathogennoAry = PathogenScores.GroupBy(c => c.Item1).Select(c => Tuple.Create(c.Key, c.Sum(c1 => c1.Item2))).OrderByDescending(c => c.Item2).ToArray();
                    var maxathogen  = pathogennoAry.FirstOrDefault()?.Item2 ?? 0;//此处不用有最大值，有一个算一个
                    Pathogens = pathogennoAry.ToList();

                    
                    foreach (var Pathogen in Pathogens)
                    {
                        switch (Pathogen.Item1)
                        {
                            case "胃肠湿热":
                                if (Pathogen.Item2 >= 5)
                                {
                                    _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "1"));
                                };
                                break;
                            case "软坚化结":
                                if (Pathogen.Item2 >= 5)
                                {
                                    _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "2"));
                                };
                                break;
                            case "通窍":
                                if (Pathogen.Item2  <6)
                                {
                                    _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "4"));
                                }
                                else
                                 {
                                    _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "3"));

                                };
                                break;
                            case "气逆":
                                if (Pathogen.Item2 >= 5)
                                {
                                    _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "5"));
                                };
                                break;
                            case "止血":
                                if (Pathogen.Item2 >= 5)
                                {
                                    _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "6"));
                                };
                                break;
                            case "止流":
                                if (Pathogen.Item2 > 6)
                                {
                                    _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "7"));
                                };
                                break;
                            case "排脓":
                                if (Pathogen.Item2 >= 5)
                                {
                                    _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "8"));
                                };
                                break;
                            case "消胬肉":
                                if (Pathogen.Item2 >= 5)
                                {
                                    _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "9"));
                                };
                                break;

                            case "气交阻":
                                if (Pathogen.Item2 >= 5)
                                {
                                    _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "10"));
                                };
                                break;
                            case "积食":
                                if (Pathogen.Item2 >= 5)
                                {
                                    _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "11"));
                                };
                                break;
                            case "辛开苦降":
                                if (Pathogen.Item2 >= 5)
                                {
                                    _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "12"));
                                };
                                break;
                            case "悬饮":
                                if (Pathogen.Item2 >= 5)
                                {
                                    _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "13"));
                                };
                                break;
                            case "安神":
                                if (Pathogen.Item2 > 0)
                                {//四种情况，四中输出,单独都可输出！
                                    if (MaxVicerals.Where(c => c.Item1 == "心") != null)
                                    {
                                        _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "14"));
                                    };
                                    if (MaxVicerals.Where(c => c.Item1 == "肝") != null)
                                    {
                                        _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "15"));
                                    };
                                    if (MaxVicerals.Where(c => c.Item1 == "胆") != null)
                                    {
                                        _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "16"));
                                    };
                                    if (MaxVicerals.Where(c => c.Item1 == "胃") != null)
                                    {
                                        _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "17"));
                                    };
                                };
                                break;
                            case "利咽":
                                if (Pathogen.Item2 >= 5)
                                {
                                    _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "18"));
                                };
                                break;
                            case "肝气犯胃":
                                if (Pathogen.Item2 >= 5)
                                {
                                    _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "19"));
                                };
                                break;
                            case "太阳经":
                                if (Pathogen.Item2 >= 5)
                                {
                                    _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "20"));
                                };
                                break;
                            case "阳明经":
                                if (Pathogen.Item2 >= 5)
                                {
                                    _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "21"));
                                };
                                break;
                            case "少阳经":
                                if (Pathogen.Item2 >= 5)
                                {
                                    _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "22"));
                                };
                                break;
                            case "厥阴经":
                                if (Pathogen.Item2 >= 5)
                                {
                                    _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "23"));
                                };
                                break;
                            case "通便":
                                if (Pathogen.Item2 >= 5)
                                {
                                    _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "24"));
                                };
                                break;
                            case "肝脾不调":
                                if (Pathogen.Item2 >= 5)
                                {
                                    _RhinitisCnDrugPathogens.AddRange(RhinitisCnDrugPathogen.DefaultCollection.Where(c => c.MatchString == "25"));
                                };
                                break;
                            default:
                                break;
                        }
                    }

                    ///此处开始输出所有的病机药物的，
                    ///按照病机评分项做一个循环，然后对照每个项目满足时RhinitisCnDrugConversion2s的数据
                    ///按照编号做唯一检索输出
                    /// 编号此处编号为内部定义，不和症候做任何关联。例如:编号为"3"代表通窍》=6的输出。
                    /// 例如调用时判断基础输出中有脏腑心的输出，并且安神>0，即可输出编号为15的药物
                }
                return _RhinitisCnDrugPathogens;
            }
        }

        private List<Tuple<string, decimal>> _CnDrugBase;

        /// <summary>
        /// 药物输出基准。
        /// </summary>
        public List<Tuple<string, decimal>> CnDrugBase
        {
            get
            {
                if (null == _CnDrugBase)
                {
                    var drugs = RhinitisCnDrugConversions;
                    _CnDrugBase = new List<Tuple<string, decimal>>();                 
                   
                        var coll = RhinitisCnDrugConversions.SelectMany(c => c.CnDrugProperties).Concat(RhinitisCnDrugPathogens.SelectMany(c => c.CnDrugProperties))
                            .GroupBy(c => c.Item1).Select(c => Tuple.Create(c.Key, c.Max(c1 => c1.Item2)));
                        _CnDrugBase.AddRange(coll);
                   
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
                    var coll = CnDrugBase.Select(c => Tuple.Create(c.Item1, c.Item2 ));
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
            sb.AppendLine($"诊断： ({string.Join(";", RhinitisConversions.Select(c => c.ToString()))}) ");
            sb.AppendLine($"脏腑评分:{string.Join(";", MaxVicerals.Select(c => c.ToString()))};症候评分:{string.Join(";", MaxPhenomenons.Select(c => c.ToString()))};病机评分:{string.Join(";", Pathogens.Select(c => c.ToString()))}");
            result = sb.ToString();
            return result;
        }
    }

    /// <summary>
    /// 封装鼻炎的算法。
    /// </summary>
    public class RhinitisMethods : CnMedicineAlgorithm
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
            result.Description = $"{string.Join(",", coll.RhinitisCnDrugConversions.Select(c => c.CnMedicineConclusions).Distinct())}";
            if (string.IsNullOrWhiteSpace(result.Description))
                result.Description = "鼻炎";
            return result;
        }

        /// <summary>
        /// 获取最初症状的结构化数据。
        /// </summary>
        /// <param name="surveys"></param>
        /// <param name="db"></param>
        RhinitisAnalysisData GetFirstCore(Surveys surveys, ApplicationDbContext db)
        {
            RhinitisAnalysisData result = new RhinitisAnalysisData();
            var answerTemplates = db.Set<SurveysAnswerTemplate>();
            var quesetions = db.Set<SurveysQuestionTemplate>();
            quesetions.Load();
            var tb1 = db.Set<RhinitisConversion>();
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
                var conv11 = RhinitisConversion.DefaultCollection.FirstOrDefault(c => c.CnSymptomNumber == number.Item2.ToString());
                if (null == conv11)
                    continue;

                result.RhinitisConversions.Add(conv11);

                var propIns = EntityUtil.GetTuples(answer.UserState);
                var invalid = propIns.FirstOrDefault(c => c.Item1 == "无效");
                if (null != invalid && invalid.Item2 != 0)   //若存在无效条目
                {
                   // result.Invalid11s.Add(conv11);
                }
            }
            return result;
        }

    }
}
