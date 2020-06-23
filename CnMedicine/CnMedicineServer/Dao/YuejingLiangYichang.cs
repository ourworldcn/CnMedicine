using System.Runtime.Serialization;

namespace CnMedicineServer.Models
{
    #region 月经过多

    [DataContract]
    public class YueJingLiangGuoDuoFenXing : GrrBianZhengFenXingBase
    {
        public YueJingLiangGuoDuoFenXing()
        {
        }
    }

    [DataContract]
    public class YueJingLiangGuoDuoJingLuoBian : GrrJingLuoBianZhengBase
    {
        public YueJingLiangGuoDuoJingLuoBian()
        {
        }
    }

    [DataContract]
    public class YueJingLiangGuoDuoGeneratedNumeber : GeneratedNumeber
    {
        public YueJingLiangGuoDuoGeneratedNumeber()
        {
        }
    }

    [DataContract]
    public class YueJingLiangGuoDuoCnDrugCorrection : CnDrugCorrectionBase
    {
        public YueJingLiangGuoDuoCnDrugCorrection()
        {
        }
    }
    #endregion 月经过多

    #region 月经过少

    [DataContract]
    public class YueJingLiangGuoShaoFenXing : GrrBianZhengFenXingBase
    {
        public YueJingLiangGuoShaoFenXing()
        {
        }
    }

    [DataContract]
    public class YueJingLiangGuoShaoJingLuoBian : GrrJingLuoBianZhengBase
    {
        public YueJingLiangGuoShaoJingLuoBian()
        {
        }
    }

    [DataContract]
    public class YueJingLiangGuoShaoGeneratedNumeber : GeneratedNumeber
    {
        public YueJingLiangGuoShaoGeneratedNumeber()
        {
        }
    }

    [DataContract]
    public class YueJingLiangGuoShaoCnDrugCorrection : CnDrugCorrectionBase
    {
        public YueJingLiangGuoShaoCnDrugCorrection()
        {
        }
    }
    #endregion 月经过少

}