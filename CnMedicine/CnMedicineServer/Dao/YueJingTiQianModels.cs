﻿
using System.Runtime.Serialization;

namespace CnMedicineServer.Models
{
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

}