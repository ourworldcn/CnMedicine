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
using System.Web.Http.Description;

namespace CnMedicineServer.Controllers
{
    /// <summary>
    /// 专病功能控制器。
    /// </summary>
    [RoutePrefix("api/SpecialCasesInsomnia")]
    //[EnableCors]
    public class SpecialCasesInsomniaController : OwApiControllerBase
    {
        /// <summary>
        /// 获取所有专病的列表。暂时不用。
        /// </summary>
        /// <param name="model">控制分页数据返回的参数。</param>
        /// <returns></returns>
        [Route("List")]
        [ResponseType(typeof(PagingResult<SpecialCases>))]
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IHttpActionResult GetList([FromUri]PagingControlBaseViewModel model)
        {
            var result = DbContext.SpecialCases.ToList();
            return Ok(result);
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
                if (null != result)
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
                      r1.TemplateId = c.AnswerTemplates.Skip(rnd.Next(c.AnswerTemplates.Count)).FirstOrDefault()?.Id ?? Guid.Empty;
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
            SurveysTemplate queryResult = DbContext.SurveysTemplates.FirstOrDefault();
            /*
             * Access-Control-Allow-Headers
            Content-Type, api_key, Authorization
            Access-Control-Allow-Methods
            GET,POST,PUT,DELETE,OPTIONS
            Access-Control-Allow-Origin
            *
            */
            SurveysTemplate result = (SurveysTemplate)queryResult.Clone();  //XML序列化要增加KnownTypes
            return Ok(result);
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

            try
            {
                model.GeneratedIdIfEmpty();
                foreach (var item in model.SurveysAnswers)
                {
                    item.GeneratedIdIfEmpty();
                    if (item.SurveysId == Guid.Empty)
                        item.SurveysId = model.Id;
                }

                var methods = new InsomniaMethod();
                DbContext.Set<Surveys>().Add(model);
                DbContext.SaveChanges();
                var result = methods.GetFirstResult(model, DbContext);
                DbContext.SurveysConclusions.Add(result);
                DbContext.SaveChanges();
                return Ok(result);
            }
            catch (Exception err)
            {
                return InternalServerError(err);
            }
        }
    }

}
