
using OW.Data.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
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

    #region 痛经
    /// <summary>
    /// 痛经分型类。
    /// 分型	编号	阈值	药物			对症药
    /// </summary>
    public class TongJingFenxing
    {
        static List<TongJingFenxing> _DefaultCollection;
        static bool _IsInit = false;

        public static List<TongJingFenxing> DefaultCollection
        {
            get
            {
                if (null == _DefaultCollection)
                {
                    var path = System.Web.HttpContext.Current.Server.MapPath("~/content/痛经");
                    using (var tdb = new TextFileContext(path))
                    {
                        _DefaultCollection = tdb.GetList<TongJingFenxing>("痛经病-分型表.txt");
                    }

                }
                return _DefaultCollection;
            }
        }

        /*
         * 分型组号	分型号	编号	阈值	最低阈值	方药
         * 10	101	1104,1114，10002,3109,5108,3112，1113,4416，2202,3502。	0.65	0.3	乌药10，香附15，枳实20，郁金10，柴胡20
         * */

        /// <summary>
        /// 构造函数。
        /// </summary>
        public TongJingFenxing()
        {

        }

        /// <summary>
        /// 分型。
        /// </summary>
        [TextFieldName("分型")]
        public string Fenxing { get; set; }

        /// <summary>
        /// 编号。
        /// </summary>
        [TextFieldName("编号")]
        public string Number { get; set; }

        private List<int> _Numbers;

        /// <summary>
        /// 编号列表。
        /// </summary>
        public List<int> Numbers
        {
            get
            {
                lock (this)
                {
                    if (null == _Numbers)
                    {
                        _Numbers = EntityUtility.GetArray(Number.Trim('\"')).Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => int.Parse(c)).ToList();
                    }
                }
                return _Numbers;
            }
        }



        /// <summary>
        /// 阈值。
        /// </summary>
        [TextFieldName("阈值")]
        public float Thresholds { get; set; }

        /// <summary>
        /// 阈值。
        /// </summary>
        [TextFieldName("最低阈值")]
        public float ThresholdsOfLowest { get; set; }

        [TextFieldName("分型组号")]
        public int GroupNumber { get; set; }

        [TextFieldName("方药")]
        public string Yaowu { get; set; }

        private List<Tuple<string, decimal>> _Yaowu;

        public List<Tuple<string, decimal>> YaowuList
        {
            get
            {
                lock (this)
                {
                    if (null == _Yaowu)
                    {
                        _Yaowu = EntityUtility.GetTuples(Yaowu);
                    }
                }
                return _Yaowu;
            }
        }

    }

    /// <summary>
    /// 痛经经络辩证类。
    /// 病位（六经）	编号	经期引经药	对症药	备注
    /// </summary>
    public class TongJingJingluoBianzheng
    {
        /*
         * 病位（六经）	编号1	分组	优先度	对症药
         * 少阳经	1806，1812，5309，1906，2204。	1	11	柴胡15g
         * */

        static List<TongJingJingluoBianzheng> _DefaultCollection;
        static bool _IsInit = false;

        public static List<TongJingJingluoBianzheng> DefaultCollection
        {
            get
            {
                if (null == _DefaultCollection)
                {
                    var path = System.Web.HttpContext.Current.Server.MapPath("~/content/痛经");
                    using (var tdb = new TextFileContext(path))
                    {
                        _DefaultCollection = tdb.GetList<TongJingJingluoBianzheng>("痛经病-经络辩证表.txt");
                    }

                }
                return _DefaultCollection;
            }
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        public TongJingJingluoBianzheng()
        {

        }

        [TextFieldName("分组")]
        public int GroupNumber { get; set; }

        /// <summary>
        /// 编号1。
        /// </summary>
        [TextFieldName("编号1")]
        public string NumersString1 { get; set; }

        private List<int> _Numbers1;

        /// <summary>
        /// 编号的数组。
        /// </summary>
        public List<int> Numbers1
        {
            get
            {
                lock (this)
                {
                    if (null == _Numbers1)
                    {
                        _Numbers1 = EntityUtility.GetArray(NumersString1).Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => int.Parse(c)).ToList();
                    }
                }
                return _Numbers1;
            }
        }

        /// <summary>
        /// 编号2。
        /// </summary>
        [TextFieldName("编号2")]
        public string NumersString2 { get; set; }

        private List<int> _Numbers2;

        /// <summary>
        /// 编号2的数组。
        /// </summary>
        public List<int> Numbers2
        {
            get
            {
                lock (this)
                {
                    if (null == _Numbers2)
                    {
                        _Numbers2 = EntityUtility.GetArray(NumersString2).Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => int.Parse(c)).ToList();
                    }
                }
                return _Numbers2;
            }
        }

        [TextFieldName("经期引经药")]
        public string Jingqiyinjingyao { get; set; }

        List<Tuple<string, decimal>> _JingqiyinjingyaoList;

        [NotMapped]
        public List<Tuple<string, decimal>> JingqiyinjingyaoList
        {
            get
            {
                lock (this)
                    if (null == _JingqiyinjingyaoList)
                    {
                        _JingqiyinjingyaoList = EntityUtility.GetTuples(Jingqiyinjingyao).ToList();
                    }
                return _JingqiyinjingyaoList;
            }
        }

        [TextFieldName("病位（六经）")]
        public string Bingwei { get; set; }

        [TextFieldName("对症药")]
        public string Duizhengyao { get; set; }


        List<Tuple<string, decimal>> _DuizhengyaoList;

        /// <summary>
        /// 对症药列表。
        /// </summary>
        [NotMapped]
        public List<Tuple<string, decimal>> DuizhengyaoList
        {
            get
            {
                lock (this)
                    if (null == _DuizhengyaoList)
                    {
                        _DuizhengyaoList = EntityUtility.GetTuples(Duizhengyao);
                    }
                return _DuizhengyaoList;
            }
        }

        List<Tuple<string, decimal>> _AllYao;

        /// <summary>
        /// 所有药物的组合。
        /// </summary>
        [NotMapped]
        public List<Tuple<string, decimal>> AllYao
        {
            get
            {
                lock (this)
                    if (null == _AllYao)
                    {
                        _AllYao = DuizhengyaoList.Union(JingqiyinjingyaoList).ToList();
                    }
                return _AllYao;
            }
        }

        /// <summary>
        /// 备注。
        /// </summary>
        [TextFieldName("备注")]
        public string Remark { get; set; }

        /// <summary>
        /// 优先度。
        /// </summary>
        [TextFieldName("优先度")]
        public int Priority { get; set; }


    }

    /// <summary>
    /// 药物加减表。
    /// </summary>
    public class TongJingMedicineCorrection
    {
        /*
         * 症状编号	症状	阈值	加减药	类型号
         * 10,1111。	痛经剧烈伴有恶心呕吐者	1	吴茱萸5、法半夏9、莪术15	1
         */

        static Lazy<List<TongJingMedicineCorrection>> _DefaultCollection = new Lazy<List<TongJingMedicineCorrection>>(() =>
        {
            List<TongJingMedicineCorrection> result;
            var path = System.Web.HttpContext.Current.Server.MapPath("~/content/痛经");

            using (var tdb = new TextFileContext(path) { IgnoreQuotes = true, })
            {
                result = tdb.GetList<TongJingMedicineCorrection>("痛经病-药物加减表.txt");
            }
            return result;
        }, true);

        /// <summary>
        /// 加载的数据集合。
        /// </summary>
        public static List<TongJingMedicineCorrection> DefaultCollection
        {
            get
            {
                return _DefaultCollection.Value;
            }
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        public TongJingMedicineCorrection()
        {

        }

        /// <summary>
        /// 证型。
        /// </summary>
        [TextFieldName("证型")]
        public string ZhengXing { get; set; }

        /// <summary>
        /// 症状。
        /// </summary>
        [TextFieldName("症状")]
        public string ZhengZhuang { get; set; }

        /// <summary>
        /// 症状编号。
        /// </summary>
        [TextFieldName("症状编号")]
        public string NumbersString { get; set; }

        List<int> _Numbers;

        /// <summary>
        /// 编号列表。
        /// </summary>
        public List<int> Numbers
        {
            get
            {
                lock (this)
                    if (null == _Numbers)
                    {
                        _Numbers = EntityUtility.GetArray(NumbersString).Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => int.Parse(c)).ToList();
                    }
                return _Numbers;
            }
        }
        /// <summary>
        /// 阈值。
        /// </summary>
        [TextFieldName("阈值")]
        public float Thresholds { get; set; }

        /// <summary>
        /// 加减药。
        /// </summary>
        [TextFieldName("加减药")]
        public string DrugsString { get; set; }

        List<Tuple<string, decimal>> _Drugs;

        /// <summary>
        /// 加减的药物。
        /// </summary>
        public List<Tuple<string, decimal>> Drugs
        {
            get
            {
                lock (this)
                    if (null == _Drugs)
                    {
                        _Drugs = EntityUtility.GetTuples(DrugsString);
                    }
                return _Drugs;
            }
        }

        /// <summary>
        /// 类型号。
        /// </summary>
        [TextFieldName("类型号")]
        public int TypeNumber { get; set; }
    }

    /// <summary>
    /// 经间期出血症状归类表。
    /// </summary>
    public class TongJingZhengZhuangGuiLei
    {
        /*
         * 归类编号	逻辑	阈值	说明
         * 10001	3703,3704,3705,3706,3707。	0.20 	睡眠

         */

        /// <summary>
        /// 构造函数。
        /// </summary>
        public TongJingZhengZhuangGuiLei()
        {

        }

        static Lazy<List<TongJingZhengZhuangGuiLei>> _DefaultCollection = new Lazy<List<TongJingZhengZhuangGuiLei>>(() =>
        {
            List<TongJingZhengZhuangGuiLei> result;
            var path = System.Web.HttpContext.Current.Server.MapPath("~/content/痛经");

            using (var tdb = new TextFileContext(path) { IgnoreQuotes = true, })
            {
                result = tdb.GetList<TongJingZhengZhuangGuiLei>("痛经病-症状归类表.txt");
            }
            return result;
        }, true);

        /// <summary>
        /// 默认的集合。
        /// </summary>
        public static List<TongJingZhengZhuangGuiLei> DefaultCollection
        {
            get
            {
                return _DefaultCollection.Value;
            }
        }

        /// <summary>
        /// 症状编号。
        /// </summary>
        [TextFieldName("归类编号")]
        public int Number { get; set; }

        /// <summary>
        /// 逻辑。
        /// </summary>
        [TextFieldName("逻辑")]
        public string NumbersString { get; set; }

        private List<int> _Numbers;

        /// <summary>
        /// 逻辑的数组。
        /// </summary>
        public List<int> Numbers
        {
            get
            {
                lock (this)
                {
                    if (null == _Numbers)
                    {
                        _Numbers = EntityUtility.GetArray(NumbersString).Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => int.Parse(c)).ToList();
                    }
                }
                return _Numbers;
            }
        }

        /// <summary>
        /// 阈值。
        /// </summary>
        [TextFieldName("阈值")]
        public float Thresholds { get; set; }

        /// <summary>
        /// 说明。
        /// </summary>
        [TextFieldName("说明")]
        public string Description { get; set; }

        /// <summary>
        /// 类型号。
        /// 1取最高量。2取最低量，其它处无此药则不添加
        /// </summary>
        [TextFieldName("类型号")]
        public int TypeNumber { get; set; }

    }

    [DataContract]
    public class TongJingFenXing : GrrBianZhengFenXingBase
    {
        public TongJingFenXing()
        {
        }
    }

    [DataContract]
    public class TongJingJingLuoBian : GrrJingLuoBianZhengBase
    {
        public TongJingJingLuoBian()
        {
        }
    }

    [DataContract]
    public class TongJingGeneratedNumeber : GeneratedNumeber
    {
        public TongJingGeneratedNumeber()
        {
        }
    }

    [DataContract]
    public class TongJingCnDrugCorrection : CnDrugCorrectionBase
    {
        public TongJingCnDrugCorrection()
        {
        }
    }

    #endregion 痛经

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

    #region 经间期出血

    /// <summary>
    /// 经间期出血-分型表。
    /// </summary>
    [DataContract]
    public class JingJianQiChuXueFenXing
    {
        /*
         * 分型组号	分型号	编号	阈值	最低阈值	方药
        10	101	"1602,1301,3104,5205,2202,3706,3703,3502,3113,4407,4416。"	0.65	0.3	陈皮20，柴胡18，川芎15，香附20，枳壳12，白芍12，炙甘草6，川楝子10，郁金10，木香20，酸枣仁10。
         * */
        static Lazy<List<JingJianQiChuXueFenXing>> _DefaultCollection = new Lazy<List<JingJianQiChuXueFenXing>>(() =>
        {
            List<JingJianQiChuXueFenXing> result;
            var path = System.Web.HttpContext.Current.Server.MapPath("~/content/经间期出血");

            using (var tdb = new TextFileContext(path) { IgnoreQuotes = true, })
            {
                result = tdb.GetList<JingJianQiChuXueFenXing>("经间期出血-分型表.txt");
            }
            return result;
        }, true);

        public static List<JingJianQiChuXueFenXing> DefaultCollection
        {
            get
            {
                return _DefaultCollection.Value;
            }
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        public JingJianQiChuXueFenXing()
        {

        }

        /// <summary>
        /// 分型组号。
        /// </summary>
        [TextFieldName("分型组号")]
        public int GroupNumber { get; set; }

        /// <summary>
        /// 分型号。
        /// </summary>
        [TextFieldName("分型号")]
        public int Number { get; set; }

        /// <summary>
        /// 编号。
        /// </summary>
        [TextFieldName("编号")]
        public string NumbersString { get; set; }

        private Dictionary<int, float> _Numbers;

        /// <summary>
        /// 键是编号值是其权值。
        /// </summary>
        public Dictionary<int, float> Numbers
        {
            get
            {
                lock (this)
                {
                    if (null == _Numbers)
                    {
                        _Numbers = EntityUtility.GetArrayWithPower(NumbersString).GroupBy(c => c.Item1).ToDictionary(c => c.Key, c => c.Sum(subc => subc.Item2));
                    }
                }
                return _Numbers;
            }
        }


        /// <summary>
        /// 阈值。
        /// </summary>
        [TextFieldName("阈值")]
        [DataMember]
        public float Thresholds { get; set; }

        /// <summary>
        /// 最低阈值。
        /// </summary>
        [DataMember]
        [TextFieldName("最低阈值")]
        public float ThresholdsOfLowest { get; set; }

        /// <summary>
        /// 药物。
        /// </summary>
        [TextFieldName("方药")]
        public string CnDrugString { get; set; }

        /// <summary>
        /// 对症药。
        /// </summary>
        [TextFieldName("对症药")]
        public string DuiZhengCnDrugString { get; set; }

        private List<Tuple<string, decimal>> _CnDrugs;

        /// <summary>
        /// 所有药物的列表。
        /// </summary>
        [DataMember]
        public List<Tuple<string, decimal>> CnDrugs
        {
            get
            {
                lock (this)
                    if (null == _CnDrugs)
                    {
                        _CnDrugs = new List<Tuple<string, decimal>>();
                        if (null != CnDrugString)
                            _CnDrugs.AddRange(EntityUtility.GetTuples(CnDrugString));
                        if (null != DuiZhengCnDrugString)
                            _CnDrugs.AddRange(EntityUtility.GetTuples(DuiZhengCnDrugString));
                    }
                return _CnDrugs;
            }
            set
            {
                _CnDrugs = value.ToList();
            }
        }

    }

    /// <summary>
    /// 经间期出血经络辩证表。
    /// </summary>
    public class JingJianQiChuXueJingLuoBianZheng
    {
        /*
            病位	编号	分组	优先度	药物
            少阳经	1510，1511。	1	11	柴胡15  小茴香12
         */

        static Lazy<List<JingJianQiChuXueJingLuoBianZheng>> _DefaultCollection = new Lazy<List<JingJianQiChuXueJingLuoBianZheng>>(() =>
        {
            List<JingJianQiChuXueJingLuoBianZheng> result;
            var path = System.Web.HttpContext.Current.Server.MapPath("~/content/经间期出血");

            using (var tdb = new TextFileContext(path) { IgnoreQuotes = true, })
            {
                result = tdb.GetList<JingJianQiChuXueJingLuoBianZheng>("经间期出血-经络辩证表.txt");
            }
            return result;
        }, true);

        /// <summary>
        /// 默认的集合。
        /// </summary>
        public static List<JingJianQiChuXueJingLuoBianZheng> DefaultCollection
        {
            get
            {
                return _DefaultCollection.Value;
            }
        }

        /// <summary>
        /// 分组。
        /// </summary>
        [TextFieldName("分组")]
        public int GroupNumber { get; set; }

        /// <summary>
        /// 病位（六经）。
        /// </summary>
        [TextFieldName("病位（六经）")]
        public string BingWei { get; set; }

        /// <summary>
        /// 编号1。
        /// </summary>
        [TextFieldName("编号1")]
        public string NumersString1 { get; set; }

        private List<int> _Numbers1;

        /// <summary>
        /// 编号的数组。
        /// </summary>
        public List<int> Numbers1
        {
            get
            {
                lock (this)
                {
                    if (null == _Numbers1)
                    {
                        _Numbers1 = EntityUtility.GetArray(NumersString1).Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => int.Parse(c)).ToList();
                    }
                }
                return _Numbers1;
            }
        }

        /// <summary>
        /// 编号2。
        /// </summary>
        [TextFieldName("编号2")]
        public string NumersString2 { get; set; }

        private List<int> _Numbers2;

        /// <summary>
        /// 编号的数组。
        /// </summary>
        public List<int> Numbers2
        {
            get
            {
                lock (this)
                {
                    if (null == _Numbers2)
                    {
                        _Numbers2 = EntityUtility.GetArray(NumersString2).Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => int.Parse(c)).ToList();
                    }
                }
                return _Numbers2;
            }
        }

        /// <summary>
        /// 优先度。
        /// </summary>
        [TextFieldName("优先度")]
        public int Priority { get; set; }

        /// <summary>
        /// 对症药。
        /// </summary>
        [TextFieldName("对症药")]
        public string DuiZhengCnDrugString { get; set; }

        List<Tuple<string, decimal>> _CnDrugs;

        /// <summary>
        /// 所有药物的列表。
        /// </summary>
        public List<Tuple<string, decimal>> CnDrugs
        {
            get
            {
                lock (this)
                    if (null == _CnDrugs)
                    {
                        _CnDrugs = EntityUtility.GetTuples(DuiZhengCnDrugString);
                    }
                return _CnDrugs;
            }
        }
    }

    /// <summary>
    /// 经间期出血症状归类表。
    /// </summary>
    public class JingJianQiChuXueZhengZhuangGuiLei
    {
        /*
         * 归类编号	逻辑	阈值	说明
         * 1001	813，814，815，816，817。	0.20 	睡眠
         */

        static Lazy<List<JingJianQiChuXueZhengZhuangGuiLei>> _DefaultCollection = new Lazy<List<JingJianQiChuXueZhengZhuangGuiLei>>(() =>
        {
            List<JingJianQiChuXueZhengZhuangGuiLei> result;
            var path = System.Web.HttpContext.Current.Server.MapPath("~/content/经间期出血");

            using (var tdb = new TextFileContext(path) { IgnoreQuotes = true, })
            {
                result = tdb.GetList<JingJianQiChuXueZhengZhuangGuiLei>("症状归类表.txt");
            }
            return result;
        }, true);

        /// <summary>
        /// 默认的集合。
        /// </summary>
        public static List<JingJianQiChuXueZhengZhuangGuiLei> DefaultCollection
        {
            get
            {
                return _DefaultCollection.Value;
            }
        }

        /// <summary>
        /// 归类编号。
        /// </summary>
        [TextFieldName("归类编号")]
        public int Number { get; set; }

        /// <summary>
        /// 逻辑。
        /// </summary>
        [TextFieldName("逻辑")]
        public string NumbersString { get; set; }

        private List<int> _Numbers;

        /// <summary>
        /// 编号的数组。
        /// </summary>
        public List<int> Numbers
        {
            get
            {
                lock (this)
                {
                    if (null == _Numbers)
                    {
                        _Numbers = EntityUtility.GetArray(NumbersString).Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => int.Parse(c)).ToList();
                    }
                }
                return _Numbers;
            }
        }

        /// <summary>
        /// 阈值。
        /// </summary>
        [TextFieldName("阈值")]
        public float Thresholds { get; set; }

        /// <summary>
        /// 说明。
        /// </summary>
        [TextFieldName("说明")]
        public string Description { get; set; }

    }

    /// <summary>
    /// 经间期出血-分型加减表。
    /// </summary>
    public class JingJianQiChuXueFenXingCorrection
    {
        static Lazy<List<JingJianQiChuXueFenXingCorrection>> _DefaultCollection = new Lazy<List<JingJianQiChuXueFenXingCorrection>>(() =>
        {
            List<JingJianQiChuXueFenXingCorrection> result;
            var path = System.Web.HttpContext.Current.Server.MapPath("~/content/经间期出血");

            using (var tdb = new TextFileContext(path) { IgnoreQuotes = true, })
            {
                result = tdb.GetList<JingJianQiChuXueFenXingCorrection>("经间期出血-分型加减表.txt");
            }
            return result;
        }, true);

        /// <summary>
        /// 经间期出血-分型加减表默认集合。
        /// </summary>
        public static List<JingJianQiChuXueFenXingCorrection> DefaultCollection
        {
            get
            {
                return _DefaultCollection.Value;
            }
        }

        /*
          * 编号	阈值	加药	减药	类型号
          * 10，1510。	1	小茴香12 龙眼肉6 五灵脂10		1
          * */

        [TextFieldName("编号")]
        public string NumbersString { get; set; }

        private List<int> _Numbers;

        /// <summary>
        /// 编号数组。
        /// </summary>
        public List<int> Numbers
        {
            get
            {
                lock (this)
                    if (null == _Numbers)
                    {
                        _Numbers = EntityUtility.GetArray(NumbersString).Where(c => int.TryParse(c, out int i)).Select(c => int.Parse(c)).ToList();
                    }
                return _Numbers;
            }
        }

        /// <summary>
        /// 阈值。
        /// </summary>
        [TextFieldName("阈值")]
        public float Thresholds { get; set; }

        /// <summary>
        /// 加药。
        /// </summary>
        [TextFieldName("加药")]
        public string CnDrugOfAddString { get; set; }

        List<Tuple<string, decimal>> _CnDrugOfAdd;

        /// <summary>
        /// 加药数组。
        /// </summary>
        public List<Tuple<string, decimal>> CnDrugOfAdd
        {
            get
            {
                lock (this)
                    if (null == _CnDrugOfAdd)
                    {
                        _CnDrugOfAdd = EntityUtility.GetTuples(CnDrugOfAddString);
                    }
                return _CnDrugOfAdd;
            }
        }

        /// <summary>
        /// 减药。
        /// </summary>
        [TextFieldName("减药")]
        public string CnDrugOfSubString { get; set; }

        List<string> _CnDrugOfSub;

        /// <summary>
        /// 减药数组。
        /// </summary>
        public List<string> CnDrugOfSub
        {
            get
            {
                lock (this)
                    if (null == _CnDrugOfSub)
                    {
                        _CnDrugOfSub = EntityUtility.GetArray(CnDrugOfSubString);
                    }
                return _CnDrugOfSub;
            }
        }

        /// <summary>
        /// 类型号。
        /// </summary>
        [TextFieldName("类型号")]
        public int TypeNumber { get; set; }
    }
    #endregion 经间期出血

}
