
using OW.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace CnMedicineServer.Models
{
    /// <summary>
    /// 经行乳房胀痛-分型表。
    /// </summary>
    [DataContract]
    public class JingXingRuFangZhangTongFenXing
    {
        /*
         * 分型组号	分型号	编号	阈值	最低阈值	方药
        10	101	"1602,1301,3104,5205,2202,3706,3703,3502,3113,4407,4416。"	0.65	0.3	陈皮20，柴胡18，川芎15，香附20，枳壳12，白芍12，炙甘草6，川楝子10，郁金10，木香20，酸枣仁10。
         * */
        static Lazy<List<JingXingRuFangZhangTongFenXing>> _DefaultCollection = new Lazy<List<JingXingRuFangZhangTongFenXing>>(() =>
        {
            List<JingXingRuFangZhangTongFenXing> result;
            var path = System.Web.HttpContext.Current.Server.MapPath("~/content/经行乳房胀痛");

            using (var tdb = new TextFileContext(path) { IgnoreQuotes = true, })
            {
                result = tdb.GetList<JingXingRuFangZhangTongFenXing>("乳胀-辨证分型表.txt");
            }
            return result;
        }, true);

        public static List<JingXingRuFangZhangTongFenXing> DefaultCollection
        {
            get
            {
                return _DefaultCollection.Value;
            }
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        public JingXingRuFangZhangTongFenXing()
        {

        }

        /// <summary>
        /// 分型组号。
        /// </summary>
        [TextFieldName("分组号")]
        public int GroupNumber { get; set; }

        /// <summary>
        /// 分型号。
        /// </summary>
        [TextFieldName("分型号")]
        public int Number { get; set; }

        /// <summary>
        /// 编号。
        /// </summary>
        [TextFieldName("症状编号")]
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
    /// 经行乳房胀痛-经络辩证表。
    /// </summary>
    public class JingXingRuFangZhangTongJingLuoBianZheng
    {
        /*
            病位	编号	阈值	分组号	优先度	药物
            少阳	1103，1406，1206，5306。	0.25	20	11	柴胡15 茯苓15
         */

        static Lazy<List<JingXingRuFangZhangTongJingLuoBianZheng>> _DefaultCollection = new Lazy<List<JingXingRuFangZhangTongJingLuoBianZheng>>(() =>
        {
            List<JingXingRuFangZhangTongJingLuoBianZheng> result;
            var path = System.Web.HttpContext.Current.Server.MapPath("~/content/经行乳房胀痛");

            using (var tdb = new TextFileContext(path) { IgnoreQuotes = true, })
            {
                result = tdb.GetList<JingXingRuFangZhangTongJingLuoBianZheng>("乳胀-经络辨证表.txt");
            }
            return result;
        }, true);

        /// <summary>
        /// 默认的集合。
        /// </summary>
        public static List<JingXingRuFangZhangTongJingLuoBianZheng> DefaultCollection
        {
            get
            {
                return _DefaultCollection.Value;
            }
        }

        /// <summary>
        /// 分组号。
        /// </summary>
        [TextFieldName("分组号")]
        public int GroupNumber { get; set; }

        /// <summary>
        /// 病位（六经）。
        /// </summary>
        [TextFieldName("病位")]
        public string BingWei { get; set; }

        /// <summary>
        /// 编号1。
        /// </summary>
        [TextFieldName("编号")]
        public string NumersString { get; set; }

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
                        _Numbers = EntityUtility.GetArray(NumersString).Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => int.Parse(c)).ToList();
                    }
                }
                return _Numbers;
            }
        }

        /// <summary>
        /// 优先度。
        /// </summary>
        [TextFieldName("优先度")]
        public int Priority { get; set; }

        /// <summary>
        /// 药物。
        /// </summary>
        [TextFieldName("药物")]
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

        /// <summary>
        /// 阈值。
        /// </summary>
        [TextFieldName("阈值")]
        [DataMember]
        public float Thresholds { get; set; }

    }

    /// <summary>
    /// 经行乳房胀痛-症状归类表。
    /// </summary>
    public class JingXingRuFangZhangTongZhengZhuangGuiLei
    {
        /*
         * 归类编号	逻辑	阈值	说明
         * 1001	813，814，815，816，817。	0.20 	睡眠
         */

        static Lazy<List<JingXingRuFangZhangTongZhengZhuangGuiLei>> _DefaultCollection = new Lazy<List<JingXingRuFangZhangTongZhengZhuangGuiLei>>(() =>
        {
            List<JingXingRuFangZhangTongZhengZhuangGuiLei> result;
            var path = System.Web.HttpContext.Current.Server.MapPath("~/content/经行乳房胀痛");

            using (var tdb = new TextFileContext(path) { IgnoreQuotes = true, })
            {
                result = tdb.GetList<JingXingRuFangZhangTongZhengZhuangGuiLei>("乳胀-症状归类表.txt");
            }
            return result;
        }, true);

        /// <summary>
        /// 默认的集合。
        /// </summary>
        public static List<JingXingRuFangZhangTongZhengZhuangGuiLei> DefaultCollection
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
    /// 经行乳房胀痛-分型加减表。
    /// </summary>
    public class JingXingRuFangZhangTongCorrection
    {
        static Lazy<List<JingXingRuFangZhangTongCorrection>> _DefaultCollection = new Lazy<List<JingXingRuFangZhangTongCorrection>>(() =>
        {
            List<JingXingRuFangZhangTongCorrection> result;
            var path = System.Web.HttpContext.Current.Server.MapPath("~/content/经行乳房胀痛");

            using (var tdb = new TextFileContext(path) { IgnoreQuotes = true, })
            {
                result = tdb.GetList<JingXingRuFangZhangTongCorrection>("乳胀-加减表.txt");
            }
            return result;
        }, true);

        /// <summary>
        /// 经间期出血-分型加减表默认集合。
        /// </summary>
        public static List<JingXingRuFangZhangTongCorrection> DefaultCollection
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

}