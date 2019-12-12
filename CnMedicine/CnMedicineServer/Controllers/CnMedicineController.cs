using CnMedicineServer.Models;
using OW.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace CnMedicineServer.Controllers
{
    /// <summary>
    /// 专病功能控制器。
    /// </summary>
    [RoutePrefix("api/SpecialCases")]
    public class SpecialCasesController : OwApiControllerBase
    {
        /// <summary>
        /// 获取所有专病的列表。
        /// </summary>
        /// <param name="model">控制分页数据返回的参数。</param>
        /// <returns></returns>
        [Route("List")]
        [ResponseType(typeof(PagingResult<SpecialCases>))]
        [HttpGet]
        public IHttpActionResult GetList([FromUri]PagingControlBaseViewModel model)
        {
            var result = DbContext.SpecialCases.ToList();
            return Ok(result);
        }
    }
}
