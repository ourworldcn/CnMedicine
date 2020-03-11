using OW.Data.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CnMedicineServer.Models
{
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
                        _Numbers = EntityUtil.GetArray(Number.Trim('\"')).Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => int.Parse(c)).ToList();
                    }
                }
                return _Numbers;
            }
        }



        /// <summary>
        /// 阈值。
        /// </summary>
        [TextFieldName("阈值")]
        public float Fact { get; set; }

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
                        _Yaowu = EntityUtil.GetTuples(Yaowu);
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
                        _Numbers2 = EntityUtil.GetArray(NumersString2).Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => int.Parse(c)).ToList();
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
                        _JingqiyinjingyaoList = EntityUtil.GetTuples(Jingqiyinjingyao).ToList();
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
                        _DuizhengyaoList = EntityUtil.GetTuples(Duizhengyao);
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
         * 证型	症状	编号	阈值	加减药
         * 气滞血瘀	痛经剧烈伴有恶心呕吐者	115	1	吴茱萸5、法半夏9、莪术15
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
        /// 症状。
        /// </summary>
        [TextFieldName("编号")]
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
                        _Numbers = EntityUtil.GetArray(NumbersString).Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => int.Parse(c)).ToList();
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
                        _Drugs = EntityUtil.GetTuples(DrugsString);
                    }
                return _Drugs;
            }
        }

    }

    /// <summary>
    /// 经间期出血症状归类表。
    /// </summary>
    public class TongJingZhengZhuangGuiLei
    {
        /*
         * 归类编号	逻辑	阈值	说明
         * 1001	813，814，815，816，817。	0.20 	睡眠
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