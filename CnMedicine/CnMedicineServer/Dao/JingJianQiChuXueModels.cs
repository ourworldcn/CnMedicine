using OW.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CnMedicineServer.Models
{
    /// <summary>
    /// 经间期出血-分型表。
    /// </summary>
    public class JingJianQiChuXueFenXing
    {
        // 分型	编号	阈值	药物			对症药
        //肝郁气滞	"219，233，1002，324，329，224，349，1001，1005，505，506，1006，607,616，620。"	0.7	陈皮20、柴胡18，川芎15、香附20、枳壳12、白芍12，炙甘草6g			

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
        /// 分型。
        /// </summary>
        [TextFieldName("分型")]
        public string FenXing { get; set; }

        /// <summary>
        /// 编号。
        /// </summary>
        [TextFieldName("编号")]
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
                        _Numbers = EntityUtil.GetArray(NumbersString).Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => int.Parse(c)).ToList();
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
        /// 药物。
        /// </summary>
        [TextFieldName("药物")]
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
        public List<Tuple<string, decimal>> CnDrugs
        {
            get
            {
                lock (this)
                    if (null == _CnDrugs)
                    {
                        _CnDrugs = EntityUtil.GetTuples(CnDrugString);
                        _CnDrugs.AddRange(EntityUtil.GetTuples(DuiZhengCnDrugString));
                    }
                return _CnDrugs;
            }
        }

    }

    /// <summary>
    /// 经间期出血经络辩证表。
    /// </summary>
    public class JingJianQiChuXueJingLuoBianZheng
    {
        /*
         * 病位（六经）	编号1	编号2	优先度	对症药	
         * 少阳经	"6301,6302,6303,6304,6305,6306,6307。"	351，982，945，946，337，338，720。	11	小茴香12 荔枝肉10 五灵脂10柴胡15g	
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
                        _Numbers1 = EntityUtil.GetArray(NumersString1).Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => int.Parse(c)).ToList();
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
                        _Numbers2 = EntityUtil.GetArray(NumersString2).Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => int.Parse(c)).ToList();
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
                        _CnDrugs = EntityUtil.GetTuples(DuiZhengCnDrugString);
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
                        _Numbers = EntityUtil.GetArray(NumbersString).Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => int.Parse(c)).ToList();
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
}