
using CnMedicineServer;
using CnMedicineServer.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using OW.ViewModel;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;

namespace CnMedicineServer.Controllers
{
    /// <summary>
    /// 纯WebApi的控制器基类。
    /// </summary>
    public abstract class OwApiControllerBase : ApiController
    {
        private ApplicationUserManager _userManager;

        /// <summary>
        /// 获取或设置用户管理器。
        /// </summary>
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            protected set
            {
                _userManager = value;
            }
        }

        ApplicationDbContext _DbContext;

        /// <summary>
        /// 获取或设置用户管理器。
        /// 由宿主环境负责生命周期，一般不需要处置。
        /// </summary>
        public ApplicationDbContext DbContext
        {
            get
            {
                return _DbContext ?? Request.GetOwinContext().Get<ApplicationDbContext>();
            }
            private set
            {
                _DbContext = value;
            }
        }

        //ApplicationRoleManager _ApplicationRoleManager;

        ///// <summary>
        ///// 获取或设置角色管理器。
        ///// </summary>
        //public ApplicationRoleManager RoleManager
        //{
        //    get
        //    {
        //        return _ApplicationRoleManager ?? (_ApplicationRoleManager = new ApplicationRoleManager(DbContext));
        //    }
        //    set
        //    {
        //        _ApplicationRoleManager = value;
        //    }
        //}


        /// <summary>
        /// 返回错误信息500。
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // 没有可发送的 ModelState 错误，因此仅返回空 BadRequest。
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }

        /// <summary>
        /// 返回分页数据结果。包含了错误处理。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryable"></param>
        /// <param name="model"></param>
        /// <returns>200成功，此时返回分页数据。</returns>
        protected virtual OkNegotiatedContentResult<PagingResult<T>> HyPaging<T>(IQueryable<T> queryable, PagingControlBaseViewModel model)
        {
            var result = new PagingResult<T>() { UserState = model.UserState, };
            try
            {
                result.MaxCount = queryable.Count();
                result.Datas = queryable.Skip(model.Index).Take(model.Count).ToList();
            }
            catch (Exception )
            {
                //return InternalServerError(err);
            }
            return Ok(result);
        }


    }

}