using CnMedicineServer.Bll;
using CnMedicineServer.Models;
using OW;
using OW.Data.Entity;
using OW.ViewModel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;

namespace CnMedicineServer.Controllers
{

    /// <summary>
    /// 专病功能控制器。
    /// </summary>
    [RoutePrefix("api/SpecialCasesInsomnia")]
    [EnableCors("*", "*", "*")/*crossDomain: true,*/]
    public class SpecialCasesInsomniaController : OwApiControllerBase
    {
        static Lazy<ConcurrentDictionary<Guid, Type>> _AllAlgorithmTypes = new Lazy<ConcurrentDictionary<Guid, Type>>(() =>
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(c => c.GetTypes())
                .Where(c => c.IsClass && !c.IsAbstract && typeof(CnMedicineAlgorithmBase).IsAssignableFrom(c)).Distinct();
            var coll = from tmp in types
                       let attr = tmp.GetCustomAttributes<OwAdditionalAttribute>(false).FirstOrDefault(c => c.Name == CnMedicineAlgorithmBase.SurveysTemplateIdName)
                       where null != attr
                       select Tuple.Create(attr.Value, tmp);
            var result = new ConcurrentDictionary<Guid, Type>();
            foreach (var item in coll)
            {
                if (Guid.TryParse(item.Item1, out Guid guid))
                    result.TryAdd(guid, item.Item2);
            }

            return result;
        }, true);

        /// <summary>
        /// 返回所有算法类。键是算法处理调查模板Id,值是算法类的类型。
        /// </summary>
        public static ConcurrentDictionary<Guid, Type> AllAlgorithmTypes
        {
            get
            {
                if (!_AllAlgorithmTypes.IsValueCreated) //若尚未初始化
                {
                    foreach (var item in _AllAlgorithmTypes.Value.Values)
                    {
                        var coll = from tmp in item.GetMethods(BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public)
                                   let attr = tmp.GetCustomAttributes<OwAdditionalAttribute>(true).FirstOrDefault(c => c.Name == CnMedicineAlgorithmBase.InitializationFuncName)
                                   where null != attr
                                   select tmp;
                        var mi = coll.FirstOrDefault();
                        if (null != mi)
                            using (var db = new ApplicationDbContext())
                                mi.Invoke(null, new object[] { db });
                    }

                }
                return _AllAlgorithmTypes.Value;
            }
        }

        const string _SaveConclusionPath = "/web/interface/questionnaire/save";

        static Lazy<HttpClient> _LazyHttpClient = new Lazy<HttpClient>(() =>
        {
            var result = new HttpClient();
            result.BaseAddress = new Uri("http://39.104.89.104:7000");
            result.DefaultRequestHeaders.Accept.Clear();
            result.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            return result;
        }, true);

        /// <summary>
        /// 获取方位其它WebApi的对象。
        /// </summary>
        /// <returns></returns>
        public static HttpClient GetHttpClient()
        {
            return _LazyHttpClient.Value;
        }

        /// <summary>
        /// 測試用發送到平臺數據的地址。
        /// </summary>
        const string _TestSaveConclusionPath = "/web/interface/questionnaire/save";
        static Lazy<HttpClient> _LazyTestHttpClient = new Lazy<HttpClient>(() =>
        {
            var result = new HttpClient();
            result.BaseAddress = new Uri("http://39.104.89.104:7080");
            result.DefaultRequestHeaders.Accept.Clear();
            result.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            return result;
        }, true);

        /// <summary>
        /// 获取發送到測試平臺WebApi的对象。
        /// </summary>
        /// <returns></returns>
        public static HttpClient GetTestHttpClient()
        {
            return _LazyTestHttpClient.Value;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        static SpecialCasesInsomniaController()
        {
            var tmp = AllAlgorithmTypes;
        }

        /// <summary>
        /// 获取所有专病的列表。
        /// </summary>
        /// <param name="model">控制分页数据返回的参数。</param>
        /// <returns>专病列表。</returns>
        [Route("ListTemplate")]
        [ResponseType(typeof(PagingResult<SurveysTemplate>))]
        [HttpGet]
        public IHttpActionResult GetList([FromUri]PagingControlBaseViewModel model)
        {
            var coll = DbContext.SurveysTemplates.AsNoTracking().OrderBy(c => c.Name);
            var result = Paging(coll, model);
            foreach (var item in result.Content.Datas)
            {

                item.LoadThingPropertyItemsAsync(DbContext).Wait();
            }

            return result;
        }

        private Surveys GetLastSurveysCore(string userId)
        {
            Surveys result;
            DateTime dt = DateTime.UtcNow.Date;
            var last = SubMonth(dt, 3);
            var coll = DbContext.Set<Surveys>().Where(c => c.UserId == userId && c.CreateUtc >= last).OrderByDescending(c => c.CreateUtc).Take(1).ToArray();
            result = coll.FirstOrDefault();
            if (null != result && !string.IsNullOrWhiteSpace(result.UserState))
            {
                var ary = EntityUtility.GetTuples(result.UserState);
                var flag = ary.FirstOrDefault(c => c.Item1 == "复诊")?.Item2 ?? 0;
                if (flag > 0)
                    result = null;
            }
            result?.LoadThingPropertyItemsAsync(DbContext).Wait();
            return result;
        }

        /// <summary>
        /// 获取用户最后的调查结果即最后一次诊断的结果。模板数据还是要调用 FirstSurveysTemplate 获取。这里给出的是实例数据。
        /// </summary>
        /// <param name="userId">用户Id，最长128个字符。不可为空。</param>
        /// <return></return>
        [Route("LastSurveys")]
        [HttpGet]
        [ResponseType(typeof(Surveys))]
        public IHttpActionResult GetLastSurveys([FromUri]string userId)
        {
            var db = DbContext;
            try
            {
                DateTime dt = DateTime.UtcNow.Date;
                var last = SubMonth(dt, 3);
                var coll = db.Set<Surveys>().Where(c => c.UserId == userId && c.CreateUtc >= last).OrderByDescending(c => c.CreateUtc).Take(1).ToArray();
                var result = coll.FirstOrDefault();
                if (null != result && !string.IsNullOrWhiteSpace(result.UserState))
                {
                    var ary = EntityUtility.GetTuples(result.UserState);
                    var flag = ary.FirstOrDefault(c => c.Item1 == "复诊")?.Item2 ?? 0;
                    if (flag > 0)
                        result = null;
                }
                result?.LoadThingPropertyItemsAsync(db);
                return Ok(result);
            }
            catch (Exception err)
            {
                return InternalServerError(err);
            }
        }

        /// <summary>
        /// 获取指定调查问卷上一次调查问卷。
        /// </summary>
        /// <param name="id"></param>
        /// <returns>对不支持复诊的问卷不会返回任何数据。</returns>
        [Route("LastSurveysFromId")]
        [HttpGet]
        [ResponseType(typeof(Surveys))]
        public IHttpActionResult GetLastSurveysFromId([FromUri] Guid id)
        {
            Surveys result = null;
            var db = DbContext;
            var current = db.Surveys.Find(id);
            if (null == current)
                return BadRequest($"找不到指定Id={id}的调查问卷。");
            if (!current.Template.UserState.Contains("支持复诊1"))
                return Ok(result);
            result = db.Surveys.Where(c => c.UserId == current.UserId && c.CreateUtc < current.CreateUtc).OrderByDescending(c => c.CreateUtc).FirstOrDefault();
            if (null == result)   //若没找到
                return Ok(result);
            if ((current.CreateUtc - result.CreateUtc) > TimeSpan.FromDays(90))   //若超时
                return Ok<Surveys>(null);
            result.LoadThingPropertyItemsAsync(db);
            return Ok(result);
        }

        DateTime SubMonth(DateTime dateTime, int month)
        {
            if (month < 0 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month), "应在0-12之间");
            return new DateTime(dateTime.Month <= month ? dateTime.Year - 1 : dateTime.Year, (dateTime.Month + 11 - month) % 12 + 1, dateTime.Day > 28 ? 28 : dateTime.Day,
                dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond);
        }

        Surveys GetSeedSurveys()
        {
            Random rnd = new Random();
            Surveys result = new Surveys();
            var coll = DbContext.Set<SurveysQuestionTemplate>().ToArray().Select(c =>
              {
                  var r1 = new SurveysAnswer()
                  {
                      Guts = "18",
                  };
                  if (QuestionsKind.Describe == c.Kind)
                      r1.TemplateId = c.Id;
                  else
                      r1.TemplateId = c.Answers.Skip(rnd.Next(c.Answers.Count)).FirstOrDefault()?.Id ?? Guid.Empty;
                  return r1;
              });
            result.SurveysAnswers = coll.ToList();
            return result;
        }

        /// <summary>
        /// 获取调查数据模板。
        /// </summary>
        [Route("SurveysTemplate")]
        [HttpGet]
        [ResponseType(typeof(SurveysTemplate))]
        public IHttpActionResult GetSurveysTemplate([FromUri]Guid? id = null)
        {
            var db = DbContext;
            var proxy = db.Configuration.ProxyCreationEnabled;
            db.Configuration.ProxyCreationEnabled = false;
            if (null == id)
                id = Guid.Empty;
            try
            {
                var query = DbContext.SurveysTemplates.Include("Questions").Include("Questions.Answers");
                SurveysTemplate queryResult = query.Where(c => c.Id == id.Value).FirstOrDefault();
                if (null == queryResult)
                    queryResult = query.Where(c => c.Name == "失眠").FirstOrDefault();
                queryResult?.LoadThingPropertyItemsAsync(db).Wait();
                return Ok(queryResult);
            }
            finally
            {
                db.Configuration.ProxyCreationEnabled = proxy;
            }
            /*
             * Access-Control-Allow-Headers
            Content-Type, api_key, Authorization
            Access-Control-Allow-Methods
            GET,POST,PUT,DELETE,OPTIONS
            Access-Control-Allow-Origin
            *
            */
        }

        /// <summary>
        /// 设置或追加调查数据。
        /// 注释：没用Put,为通用性用Post。轶事:我居然听过前端告诉我没用过Put,不会。
        /// </summary>
        /// <param name="model">其中各对象的Id设置为全0，标识添加。</param>
        /// <returns>200时返回一个结论对象。DebugMessage目前仅调试用。</returns>
        [Route("SetSurveys")]
        [HttpPost]
        [ResponseType(typeof(SurveysConclusion))]
        public IHttpActionResult SetSurveys(Surveys model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (string.IsNullOrWhiteSpace(model.UserId))
                return BadRequest("UserId不可为空。");
            //var collx = ThingEntityBase.LoadThingPropertyItemsAsync(DbContext, DbContext.Surveys.Where(c=>c.Id!=Guid.Empty)).Result;
            try
            {
                model.GeneratedIdIfEmpty();
                foreach (var item in model.SurveysAnswers)
                {
                    item.GeneratedIdIfEmpty();
                    if (item.SurveysId == Guid.Empty)
                        item.SurveysId = model.Id;
                }

                var last = GetLastSurveysCore(model.UserId);
                if (null != last)
                    model.UserState = "复诊1";
                else
                    model.UserState = "复诊0";
                DbContext.Surveys.AddOrUpdate(model);
                model.SaveThingPropertyItemsAsync(DbContext).Wait();

                var strName = DbContext.SurveysTemplates.Find(model.TemplateId)?.Name;
                CnMedicineAlgorithmBase algs;
                SurveysConclusion result = null;
                try
                {
                    switch (strName)
                    {
                        case "失眠":
                            algs = new InsomniaAlgorithm();
                            result = algs.GetResult(model, DbContext);
                            break;
                        case "鼻炎":
                            algs = new RhinitisAlgorithm();
                            result = algs.GetResult(model, DbContext);
                            break;

                        default:
                            if (AllAlgorithmTypes.TryGetValue(model.TemplateId, out Type type))
                            {
                                algs = TypeDescriptor.CreateInstance(null, type, null, null) as CnMedicineAlgorithmBase;
                                result = algs.GetResult(model, DbContext);
                            }
                            break;
                    }
                }
                catch (Exception err)
                {
                    return InternalServerError(err);
                }
                result.GeneratedIdIfEmpty();
                DbContext.SurveysConclusions.Add(result);
                result.SaveThingPropertyItemsAsync(DbContext).Wait();
                //写入图片
                result?.Surveys?.SavePictures(DbContext);
                model.ConclusionId = result.Id;

                DbContext.SaveChanges();
                try
                {
                    var client = GetHttpClient();

                    var guts = new SaveConclusioModel(model, result, DbContext);
                    result.SaveThingPropertyItemsAsync(DbContext).Wait();
                    var response = client.PostAsJsonAsync(_SaveConclusionPath, guts).Result;
                    response.EnsureSuccessStatusCode();

                    //向測試平臺發送數據
                    var testClient = GetHttpClient();
                    GetTestHttpClient()?.PostAsJsonAsync(_TestSaveConclusionPath, guts);
                }
                catch (Exception err)
                {
                    //TO DO
                }
                DbContext.SaveChanges();

                return Ok(result);
            }
            catch (Exception err)
            {
                return InternalServerError(err);
            }
        }

        /// <summary>
        /// 获取指定Id的诊断结论。Description是诊断信息。Conclusion是处方，
        /// </summary>
        /// <param name="id">结论的Id。一般来源于 SetSurveys 的结果。</param>
        /// <returns>诊断结论。</returns>
        [Route("Conclusions")]
        [HttpGet]
        [ResponseType(typeof(SurveysConclusion))]
        public IHttpActionResult GetConclusions([FromUri]Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = DbContext.SurveysConclusions.Find(id);
            result?.LoadThingPropertyItemsAsync(DbContext).Wait();
            return Ok(result);
        }

        /// <summary>
        /// 示例测试。
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [Route("TestDemo")]
        [ResponseType(typeof(SaveConclusioModel))]
        public IHttpActionResult ListFromUserId(string userId)
        {
            return Ok();
        }

        /// <summary>
        /// 按指定Id获取调查信息。参数是调查问卷的Id。
        /// </summary>
        /// <param name="surveysId">调查问卷的Id。</param>
        /// <returns>返回调查数据，如果找到指定Id的调查数据则返回null。</returns>
        [ResponseType(typeof(Surveys))]
        [Route("Surveys")]
        [HttpGet]
        public IHttpActionResult GetSurveysById([FromUri]Guid surveysId)
        {
            var db = DbContext;
            var result = db.Set<Surveys>().Find(surveysId);
            result?.LoadThingPropertyItemsAsync(db).Wait();
            result?.LoadPictures(db);
            return Ok(result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="templateId"></param>
        /// <returns></returns>
        [Route("SetSurveysWithNumbers")]
        [HttpPost]
        [ResponseType(typeof(SurveysConclusion))]
        public IHttpActionResult SetSurveysWithNumbers(List<int> model, Guid templateId)
        {
            Surveys answers = new Surveys() { TemplateId = templateId, UserId = "BySetSurveysWithNumbers" };
            answers.ThingPropertyItems.Add(new ThingPropertyItem()
            {
                Name = "PreId",
                Value = Guid.NewGuid().ToString("D"),
            });
            var db = DbContext;
            var template = db.SurveysTemplates.Find(templateId);
            var answerTemplates = template.Questions.SelectMany(c => c.Answers).ToArray();
            var coll = from tmp in answerTemplates
                       where model.Contains(tmp.OrderNum)
                       select tmp;
            answers.SurveysAnswers.AddRange(coll.Select(c => new SurveysAnswer() { TemplateId = c.Id }));
            return SetSurveys(answers);
        }

        [Route("FenXingConfig")]
        [ResponseType(typeof(JingJianQiChuXueFenXing))]
        public IHttpActionResult GetFenXingConfig()
        {
            return Ok();
        }
    }

}
