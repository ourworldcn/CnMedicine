
using CnMedicineServer.Models;
using OW.Data.Entity;
using System;

namespace CnMedicineServer.Bll
{
    public class RhinitisMethods : CnMedicineAlgorithm
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        public RhinitisMethods()
        {

        }

        /// <summary>
        /// 获取诊断结果和药方。可参见<see cref="InsomniaAlgorithm.GetFirstCore(Surveys, ApplicationDbContext)"/>实现。
        /// </summary>
        /// <param name="surveys">参见<see cref="Surveys"/></param>
        /// <param name="db">使用的数据库上下文。</param>
        /// <returns><see cref="SurveysConclusion.Conclusion"/>包含药方。<see cref="ThingEntityBase.Description"/>包含诊断结论。</returns>
        override protected SurveysConclusion GetResultCore(Surveys surveys, ApplicationDbContext db)
        {
            throw new NotImplementedException("实现该函数后，去掉该行。");
        }
    }
}