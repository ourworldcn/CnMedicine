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

    }
}
