
using System.Runtime.Serialization;

namespace CnMedicineServer.Models
{
    #region 非炎症带下病

    [DataContract]
    public class FeiYanDaiXiaFenXing : GrrBianZhengFenXingBase
    {
        public FeiYanDaiXiaFenXing()
        {
        }
    }

    [DataContract]
    public class FeiYanDaiXiaJingLuoBian : GrrJingLuoBianZhengBase
    {
        public FeiYanDaiXiaJingLuoBian()
        {
        }
    }

    [DataContract]
    public class FeiYanDaiXiaGeneratedNumeber : GeneratedNumeber
    {
        public FeiYanDaiXiaGeneratedNumeber()
        {
        }
    }

    [DataContract]
    public class FeiYanDaiXiaCnDrugCorrection : CnDrugCorrectionBase
    {
        public FeiYanDaiXiaCnDrugCorrection()
        {
        }
    }

    #endregion 非炎症带下病

    #region 炎症带下病

    /// <summary>
    /// 炎症带下病分型。
    /// </summary>
    [DataContract]
    public class YanZhengDaiXiaFenXing : GrrBianZhengFenXingBase
    {
        public YanZhengDaiXiaFenXing()
        {
        }
    }

    /// <summary>
    /// 炎症带下病经络辩证。
    /// </summary>
    [DataContract]
    public class YanZhengDaiXiaJingLuoBian : GrrJingLuoBianZhengBase
    {
        public YanZhengDaiXiaJingLuoBian()
        {
        }
    }

    /// <summary>
    /// 炎症带下病派生编号。
    /// </summary>
    [DataContract]
    public class YanZhengDaiXiaGeneratedNumeber : GeneratedNumeber
    {
        public YanZhengDaiXiaGeneratedNumeber()
        {
        }
    }

    /// <summary>
    /// 炎症带下病药物矫正。
    /// </summary>
    [DataContract]
    public class YanZhengDaiXiaCnDrugCorrection : CnDrugCorrectionBase
    {
        public YanZhengDaiXiaCnDrugCorrection()
        {
        }
    }

    #endregion 炎症带下病

    #region 慢性盆腔炎
    /// <summary>
    /// 慢性盆腔炎分型。
    /// </summary>
    [DataContract]
    public class ManXingPenQiongYanFenXing : GrrBianZhengFenXingBase
    {
        public ManXingPenQiongYanFenXing()
        {
        }
    }

    /// <summary>
    /// 慢性盆腔炎经络辩证。
    /// </summary>
    [DataContract]
    public class ManXingPenQiongYanJingLuoBian : GrrJingLuoBianZhengBase
    {
        public ManXingPenQiongYanJingLuoBian()
        {
        }
    }

    /// <summary>
    /// 慢性盆腔炎派生编号。
    /// </summary>
    [DataContract]
    public class ManXingPenQiongYanGeneratedNumeber : GeneratedNumeber
    {
        public ManXingPenQiongYanGeneratedNumeber()
        {
        }
    }

    /// <summary>
    /// 慢性盆腔炎药物矫正。
    /// </summary>
    [DataContract]
    public class ManXingPenQiongYanCnDrugCorrection : CnDrugCorrectionBase
    {
        public ManXingPenQiongYanCnDrugCorrection()
        {
        }
    }

    #endregion 慢性盆腔炎
}