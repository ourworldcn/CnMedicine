using CnMedicineServer.Bll;
using CnMedicineServer.Models;
using OW.Data.Entity;
using OW.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
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
            var coll = DbContext.SurveysTemplates.OrderBy(c => c.Name);
            var result = Paging(coll, model);
            foreach (var item in result.Content.Datas)
            {
                item.Questions = null;
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
                var ary = EntityUtil.GetTuples(result.UserState);
                var flag = ary.FirstOrDefault(c => c.Item1 == "复诊")?.Item2 ?? 0;
                if (flag > 0)
                    result = null;
            }
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
            try
            {
                DateTime dt = DateTime.UtcNow.Date;
                var last = SubMonth(dt, 3);
                var coll = DbContext.Set<Surveys>().Where(c => c.UserId == userId && c.CreateUtc >= last).OrderByDescending(c => c.CreateUtc).Take(1).ToArray();
                var result = coll.FirstOrDefault();
                if (null != result && !string.IsNullOrWhiteSpace(result.UserState))
                {
                    var ary = EntityUtil.GetTuples(result.UserState);
                    var flag = ary.FirstOrDefault(c => c.Item1 == "复诊")?.Item2 ?? 0;
                    if (flag > 0)
                        result = null;
                }
                return Ok(result);
            }
            catch (Exception err)
            {
                return InternalServerError(err);
            }
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
                model= DbContext.Set<Surveys>().Add(model);
                DbContext.SaveChanges();
                var strName = DbContext.SurveysTemplates.Find(model.TemplateId)?.Name;
                CnMedicineAlgorithm algs;
                SurveysConclusion result = null;
                switch (strName)
                {
                    case "失眠":
                        algs = new InsomniaAlgorithm();
                        result = algs.GetResult(model, DbContext);
                        break;
                    case "鼻炎":
                        algs = new RhinitisMethods();
                        result = algs.GetResult(model, DbContext);
                        break;
                    default:
                        break;
                }
                DbContext.SurveysConclusions.Add(result);
                DbContext.SaveChanges();
                return Ok(result);
            }
            catch (Exception err)
            {
                return InternalServerError(err);
            }
        }

        /// <summary>
        /// 获取指定Id的诊断结论。
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
            return Ok(result);
        }
    }

}
