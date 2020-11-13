
using System.Runtime.Serialization;

namespace CnMedicineServer.Models
{
    #region 崩漏

    [DataContract]
    public class BengLouFenXing : GrrBianZhengFenXingBase
    {
        public BengLouFenXing()
        {
        }
    }

    [DataContract]
    public class BengLouJingLuoBian : GrrJingLuoBianZhengBase
    {
        public BengLouJingLuoBian()
        {
        }
    }

    [DataContract]
    public class BengLouGeneratedNumeber : GeneratedNumeber
    {
        public BengLouGeneratedNumeber()
        {
        }
    }

    [DataContract]
    public class BengLouCnDrugCorrection : CnDrugCorrectionBase
    {
        public BengLouCnDrugCorrection()
        {
        }
    }

    #endregion 崩漏

    #region 经前综合征

    [DataContract]
    public class JingQianZongHeZhengFenXing : GrrBianZhengFenXingBase
    {
        public JingQianZongHeZhengFenXing()
        {
        }
    }

    [DataContract]
    public class JingQianZongHeZhengJingLuoBian : GrrJingLuoBianZhengBase
    {
        public JingQianZongHeZhengJingLuoBian()
        {
        }
    }

    [DataContract]
    public class JingQianZongHeZhengGeneratedNumeber : GeneratedNumeber
    {
        public JingQianZongHeZhengGeneratedNumeber()
        {
        }
    }

    [DataContract]
    public class JingQianZongHeZhengCnDrugCorrection : CnDrugCorrectionBase
    {
        public JingQianZongHeZhengCnDrugCorrection()
        {
        }
    }

    #endregion 经前综合征
}
