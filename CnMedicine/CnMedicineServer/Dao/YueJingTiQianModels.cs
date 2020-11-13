
using System.Runtime.Serialization;

namespace CnMedicineServer.Models
{
    #region 月经提前

    [DataContract]
    public class YueJingTiQianFenXing : GrrBianZhengFenXingBase
    {
        public YueJingTiQianFenXing()
        {
        }
    }

    [DataContract]
    public class YueJingTiQianJingLuoBian : GrrJingLuoBianZhengBase
    {
        public YueJingTiQianJingLuoBian()
        {
        }
    }

    [DataContract]
    public class YueJingTiQianGeneratedNumeber : GeneratedNumeber
    {
        public YueJingTiQianGeneratedNumeber()
        {
        }
    }

    [DataContract]
    public class YueJingTiQianCnDrugCorrection : CnDrugCorrectionBase
    {
        public YueJingTiQianCnDrugCorrection()
        {
        }
    }

    #endregion 月经提前

    #region 月经错后

    [DataContract]
    public class YueJingCuoHouFenXing : GrrBianZhengFenXingBase
    {
        public YueJingCuoHouFenXing()
        {
        }
    }

    [DataContract]
    public class YueJingCuoHouJingLuoBian : GrrJingLuoBianZhengBase
    {
        public YueJingCuoHouJingLuoBian()
        {
        }
    }

    [DataContract]
    public class YueJingCuoHouGeneratedNumeber : GeneratedNumeber
    {
        public YueJingCuoHouGeneratedNumeber()
        {
        }
    }

    [DataContract]
    public class YueJingCuoHouCnDrugCorrection : CnDrugCorrectionBase
    {
        public YueJingCuoHouCnDrugCorrection()
        {
        }
    }

    #endregion 月经错后

    #region 月经不定

    [DataContract]
    public class YueJingBuDingQiFenXing : GrrBianZhengFenXingBase
    {
        public YueJingBuDingQiFenXing()
        {
        }
    }

    [DataContract]
    public class YueJingBuDingQiJingLuoBian : GrrJingLuoBianZhengBase
    {
        public YueJingBuDingQiJingLuoBian()
        {
        }
    }

    [DataContract]
    public class YueJingBuDingQiGeneratedNumeber : GeneratedNumeber
    {
        public YueJingBuDingQiGeneratedNumeber()
        {
        }
    }

    [DataContract]
    public class YueJingBuDingQiCnDrugCorrection : CnDrugCorrectionBase
    {
        public YueJingBuDingQiCnDrugCorrection()
        {
        }
    }

    #endregion 月经不定

    #region 月经延长

    [DataContract]
    public class YueJingYanChangFenXing : GrrBianZhengFenXingBase
    {
        public YueJingYanChangFenXing()
        {
        }
    }

    [DataContract]
    public class YueJingYanChangJingLuoBian : GrrJingLuoBianZhengBase
    {
        public YueJingYanChangJingLuoBian()
        {
        }
    }

    [DataContract]
    public class YueJingYanChangGeneratedNumeber : GeneratedNumeber
    {
        public YueJingYanChangGeneratedNumeber()
        {
        }
    }

    [DataContract]
    public class YueJingYanChangCnDrugCorrection : CnDrugCorrectionBase
    {
        public YueJingYanChangCnDrugCorrection()
        {
        }
    }

    #endregion 月经延长

}