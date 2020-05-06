
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
}