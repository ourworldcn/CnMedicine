using CnMedicineServer.Bll;
using CnMedicineServer.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.OleDb;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CnMedicineServer.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";
            ApplicationDbContext db = new ApplicationDbContext();
            db.Users.Count();
            var objs = InsomniaCnDrugConversion.DefaultCollection;
            var objs1 = InsomniaCnDrugConversion2.DefaultCollection;
            var objs2 = InsomniaCnDrugCorrection.DefaultCollection;
            var objs3 = InsomniaConversion11.DefaultCollection;
            var objs4 = InsomniaConversion12.DefaultCollection;
            var ss = new JingJianQiChuXueAlgorithm();
            //var types = SpecialCasesInsomniaController.AllAlgorithmTypes;
            return View();
        }

        /// <summary>
        /// 测试索引页。
        /// </summary>
        /// <returns></returns>
        public ActionResult TestCover()
        {
            ViewBag.Title = "测试索引页";

            return View();
        }

        /// <summary>
        /// 测试简单本地注册页。
        /// </summary>
        /// <returns></returns>
        public ActionResult TestRegister()
        {
            ViewBag.Title = "测试本地简要注册功能";

            return View();
        }

        /// <summary>
        /// 测试简单本地登录页。
        /// </summary>
        /// <returns></returns>
        public ActionResult TestLogin()
        {
            ViewBag.Title = "测试本地简要注册功能";

            return View();
        }

        /// <summary>
        /// 测试成功登录的页。
        /// 仅当成功登陆后此操作才能返回正常的页面。
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult TestLogined()
        {
            ViewBag.Title = "测试本地简要注册功能";
            var context = Request.GetOwinContext();
            return View("TestLogin");
        }


    }
}
