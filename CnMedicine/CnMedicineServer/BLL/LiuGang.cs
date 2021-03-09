using CnMedicineServer.Models;
using OW;
using OW.Data.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Reflection;

namespace CnMedicineServer.Bll
{
    [OwAdditional(SurveysTemplateIdName, SurveysTemplateIdString)]
    [CnMedicineAlgorithm("胃脘痛", "Content/刘刚/胃脘痛")]
    public class WeiWanTongAlgorithm : LiuGangAlgorithmBase
    {
        /// <summary>
        /// 此算法处理的调查模板Id。
        /// </summary>
        public const string SurveysTemplateIdString = "50D40E3C-806F-4DCF-A088-D0234B787334";

        /// <summary>
        /// 初始化函数。
        /// </summary>
        /// <param name="context"></param>
        [OwAdditional(InitializationFuncName)]
        public static void Initialize(DbContext context)
        {
            var currentType = MethodBase.GetCurrentMethod().DeclaringType;
            var dataFilePath = GetDataFilePath(currentType);
            var cnName = GetCnName(currentType);
            InitializeCore(context, $"~/{dataFilePath}/{cnName}-症状表.txt", currentType);
            var survId = Guid.Parse(SurveysTemplateIdString);
            //初始化模板数据
            var template = context.Set<SurveysTemplate>().Find(survId);
            template.Name = cnName;
            template.UserState = "支持复诊0";
            template.Description = null;
            //添加专病项
            InsomniaCasesItem caseItem = new InsomniaCasesItem()
            {
                Name = cnName,
            };
            context.Set<InsomniaCasesItem>().AddOrUpdate(caseItem);

            context.SaveChanges();

        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        public WeiWanTongAlgorithm()
        {
        }

    }

    [OwAdditional(SurveysTemplateIdName, SurveysTemplateIdString)]
    [CnMedicineAlgorithm("腹胀", "Content/刘刚/腹胀")]
    public class FuZhangAlgorithm : LiuGangAlgorithmBase
    {
        /// <summary>
        /// 此算法处理的调查模板Id。
        /// </summary>
        public const string SurveysTemplateIdString = "9A084B93-BCD5-417F-BCE1-6795B80A406D";

        /// <summary>
        /// 初始化函数。
        /// </summary>
        /// <param name="context"></param>
        [OwAdditional(InitializationFuncName)]
        public static void Initialize(DbContext context)
        {
            var currentType = MethodBase.GetCurrentMethod().DeclaringType;
            var dataFilePath = GetDataFilePath(currentType);
            var cnName = GetCnName(currentType);
            InitializeCore(context, $"~/{dataFilePath}/{cnName}-症状表.txt", currentType);
            var survId = Guid.Parse(SurveysTemplateIdString);
            //初始化模板数据
            var template = context.Set<SurveysTemplate>().Find(survId);
            template.Name = cnName;
            template.UserState = "支持复诊0";
            template.Description = null;
            //添加专病项
            InsomniaCasesItem caseItem = new InsomniaCasesItem()
            {
                Name = cnName,
            };
            context.Set<InsomniaCasesItem>().AddOrUpdate(caseItem);

            context.SaveChanges();

        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        public FuZhangAlgorithm()
        {
        }

    }

    [OwAdditional(SurveysTemplateIdName, SurveysTemplateIdString)]
    [CnMedicineAlgorithm("嘈杂", "Content/刘刚/嘈杂")]
    public class CaoZaAlgorithm : LiuGangAlgorithmBase
    {
        /// <summary>
        /// 此算法处理的调查模板Id。
        /// </summary>
        public const string SurveysTemplateIdString = "CB8E1297-F2B7-4297-A6E0-04D015794352";

        /// <summary>
        /// 初始化函数。
        /// </summary>
        /// <param name="context"></param>
        [OwAdditional(InitializationFuncName)]
        public static void Initialize(DbContext context)
        {
            var currentType = MethodBase.GetCurrentMethod().DeclaringType;
            var dataFilePath = GetDataFilePath(currentType);
            var cnName = GetCnName(currentType);
            InitializeCore(context, $"~/{dataFilePath}/{cnName}-症状表.txt", currentType);
            var survId = Guid.Parse(SurveysTemplateIdString);
            //初始化模板数据
            var template = context.Set<SurveysTemplate>().Find(survId);
            template.Name = cnName;
            template.UserState = "支持复诊0";
            template.Description = null;
            //添加专病项
            InsomniaCasesItem caseItem = new InsomniaCasesItem()
            {
                Name = cnName,
            };
            context.Set<InsomniaCasesItem>().AddOrUpdate(caseItem);

            context.SaveChanges();

        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        public CaoZaAlgorithm()
        {
        }

    }

    [OwAdditional(SurveysTemplateIdName, SurveysTemplateIdString)]
    [CnMedicineAlgorithm("噫气", "Content/刘刚/噫气")]
    public class YiQiAlgorithm : LiuGangAlgorithmBase
    {
        /// <summary>
        /// 此算法处理的调查模板Id。
        /// </summary>
        public const string SurveysTemplateIdString = "C62C533B-35E8-459D-8EDF-F3075A04B0A1";

        /// <summary>
        /// 初始化函数。
        /// </summary>
        /// <param name="context"></param>
        [OwAdditional(InitializationFuncName)]
        public static void Initialize(DbContext context)
        {
            var currentType = MethodBase.GetCurrentMethod().DeclaringType;
            var dataFilePath = GetDataFilePath(currentType);
            var cnName = GetCnName(currentType);
            InitializeCore(context, $"~/{dataFilePath}/{cnName}-症状表.txt", currentType);
            var survId = Guid.Parse(SurveysTemplateIdString);
            //初始化模板数据
            var template = context.Set<SurveysTemplate>().Find(survId);
            template.Name = cnName;
            template.UserState = "支持复诊0";
            template.Description = null;
            //添加专病项
            InsomniaCasesItem caseItem = new InsomniaCasesItem()
            {
                Name = cnName,
            };
            context.Set<InsomniaCasesItem>().AddOrUpdate(caseItem);

            context.SaveChanges();

        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        public YiQiAlgorithm()
        {
        }

    }
}