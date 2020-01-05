using OW.Data.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace CnMedicineServer.Models
{
    /// <summary>
    /// 评分表1。
    /// </summary>
    [DataContract]
    [DebuggerDisplay("编号 = {CnSymptomNumber} ,脏腑 = ({CnVisceralScore}) ,证型 = ({CnPhenomenonScore})")]
    public class InsomniaConversion11 : ThingEntityBase
    {
        static List<InsomniaConversion11> _DefaultCollection;
        static bool _IsInit = false;

        public static List<InsomniaConversion11> DefaultCollection
        {
            get
            {
                if (null == _DefaultCollection)
                {
                    var path = System.Web.HttpContext.Current.Server.MapPath("~/content");
                    using (var tdb = new TextFileContext(path))
                    {
                        _DefaultCollection = tdb.GetList<InsomniaConversion11>("评分表1-失眠.txt");
                    }

                }
                return _DefaultCollection;
            }
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        public InsomniaConversion11()
        {

        }

        public override string ToString()
        {
            return $"编号 = {CnSymptomNumber} ,脏腑评分 = ({CnVisceralScore}) ,证型评分 = ({CnPhenomenonScore})";
        }

        /// <summary>
        /// 所属专病项Id。
        /// </summary>
        [DataMember]
        [ForeignKey(nameof(SpecialCasesItem))]
        public Guid SpecialCasesItemId { get; set; }

        /// <summary>
        /// 所属专病项导航属性。
        /// </summary>
        public virtual InsomniaCasesItem SpecialCasesItem { get; set; }

        /// <summary>
        /// 症候编号。例如:"3"。
        /// </summary>
        [DataMember]
        [TextFieldName("编号")]
        public string CnSymptomNumber { get; set; }

        /// <summary>
        /// 症候。例如：“入睡困难，精神亢奋”。
        /// </summary>
        [DataMember]
        [TextFieldName("症候")]
        public string CnSymptom { get; set; }

        /// <summary>
        /// 脏腑评分。例如:"心+1，肝+1，胆+1，脾+1"。
        /// </summary>
        [DataMember]
        [TextFieldName("脏腑评分")]
        public string CnVisceralScore { get; set; }

        List<Tuple<string, decimal>> _CnVisceralProperties;

        /// <summary>
        /// 内脏评分。
        /// 可并发访问。
        /// </summary>
        [NotMapped]
        public List<Tuple<string, decimal>> CnVisceralProperties
        {
            get
            {
                lock (this)
                    if (null == _CnVisceralProperties)
                    {
                        _CnVisceralProperties = EntityUtil.GetTuples(CnVisceralScore);
                    }
                return _CnVisceralProperties;
            }
        }

        /// <summary>
        /// 证型评分。例如:"火亢+1，气郁+1，湿热+1，脾胃不和+1，少阳枢机不利+1，痰热+1"。
        /// </summary>
        [DataMember]
        [TextFieldName("证型评分")]
        public string CnPhenomenonScore { get; set; }

        List<Tuple<string, decimal>> _CnPhenomenonProperties;

        /// <summary>
        /// 证型评分。
        /// 可并发访问。
        /// </summary>
        [NotMapped]
        public List<Tuple<string, decimal>> CnPhenomenonProperties
        {
            get
            {
                lock (this)
                    if (null == _CnPhenomenonProperties)
                    {
                        _CnPhenomenonProperties = EntityUtil.GetTuples(CnPhenomenonScore);
                    }
                return _CnPhenomenonProperties;
            }
        }

    }

    /// <summary>
    /// 评分表2。
    /// </summary>
    [DataContract]
    public class InsomniaConversion12 : ThingEntityBase
    {
        static List<InsomniaConversion12> _DefaultCollection;
        static bool _IsInit = false;

        public static List<InsomniaConversion12> DefaultCollection
        {
            get
            {
                if (null == _DefaultCollection)
                {
                    var path = System.Web.HttpContext.Current.Server.MapPath("~/content");
                    using (var tdb = new TextFileContext(path))
                    {
                        _DefaultCollection = tdb.GetList<InsomniaConversion12>("评分表2-失眠.txt");
                    }
                }
                return _DefaultCollection;
            }
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        public InsomniaConversion12()
        {

        }

        public override string ToString()
        {
            return $"症候编号 = {MatchString} ;规则 = {CnScore} ";
        }

        /// <summary>
        /// 所属专病项Id。
        /// </summary>
        [DataMember]
        [ForeignKey(nameof(SpecialCasesItem))]
        public Guid SpecialCasesItemId { get; set; }

        /// <summary>
        /// 所属专病项导航属性。
        /// </summary>
        public virtual InsomniaCasesItem SpecialCasesItem { get; set; }

        /// <summary>
        /// 症候编号。例如:"3，100，104，113，122"。
        /// </summary>
        [DataMember]
        [TextFieldName("症候编号")]
        public string MatchString { get; set; }


        private List<string> _Numbers;

        /// <summary>
        /// 编号列表。
        /// 可并发访问。
        /// </summary>
        [NotMapped]
        public List<string> Numbers
        {
            get
            {
                lock (this)
                    if (null == _Numbers)
                        _Numbers = EntityUtil.GetArray(MatchString);
                return _Numbers;
            }
        }

        /// <summary>
        /// 规则。例如:"肝+8；气郁+8"。
        /// </summary>
        [DataMember]
        [TextFieldName("规则")]
        public string CnScore { get; set; }

        private List<Tuple<string, decimal>> _CnScoreProperties;

        /// <summary>
        /// 规则列表。
        /// 可并发访问。
        /// </summary>
        [NotMapped]
        public List<Tuple<string, decimal>> CnScoreProperties
        {
            get
            {
                lock (this)
                    if (null == _CnScoreProperties)
                        _CnScoreProperties = EntityUtil.GetTuples(CnScore);
                return _CnScoreProperties;
            }
        }

    }

    /// <summary>
    /// 药物输出表。
    /// </summary>
    [DataContract]
    public class InsomniaCnDrugConversion : ThingEntityBase
    {
        static List<InsomniaCnDrugConversion> _DefaultCollection;
        static bool _IsInit = false;

        public static List<InsomniaCnDrugConversion> DefaultCollection
        {
            get
            {
                if (null == _DefaultCollection)
                {
                    var path = System.Web.HttpContext.Current.Server.MapPath("~/content");
                    using (var tdb = new TextFileContext(path))
                    {
                        _DefaultCollection = tdb.GetList<InsomniaCnDrugConversion>("药物输出-失眠.txt");
                    }
                }
                return _DefaultCollection;
            }
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        public InsomniaCnDrugConversion()
        {

        }

        /// <summary>
        /// 所属专病项Id。
        /// </summary>
        [DataMember]
        [ForeignKey(nameof(SpecialCasesItem))]
        public Guid SpecialCasesItemId { get; set; }

        /// <summary>
        /// 所属专病项导航属性。
        /// </summary>
        public virtual InsomniaCasesItem SpecialCasesItem { get; set; }

        /// <summary>
        /// 脏腑。如:心
        /// </summary>
        [DataMember]
        [TextFieldName("脏腑结论")]
        public string CnMedicineVisceral { get; set; }

        /// <summary>
        /// 证型结论。如:火亢
        /// </summary>
        [DataMember]
        [TextFieldName("证型结论")]
        public string CnMedicinePhenomenon { get; set; }

        /// <summary>
        /// 输出诊断。
        /// 例如:"心火亢盛"。
        /// </summary>
        [DataMember]
        [TextFieldName("输出诊断")]
        public string CnMedicineConclusions { get; set; }

        /// <summary>
        /// 药物组合1。当其为第一诊断时。
        /// 例如:"淡豆豉9栀子9黄连3黄芩9炒酸枣仁15远志9合欢皮15夜交藤30"。
        /// </summary>
        [DataMember]
        [TextFieldName("当其为第一诊断时")]
        public string CnDrugString1 { get; set; }

        List<Tuple<string, decimal>> _CnDrugProperties1;

        /// <summary>
        /// 药物剂量1。
        /// 可并发访问。
        /// </summary>
        [NotMapped]
        public List<Tuple<string, decimal>> CnDrugProperties1
        {
            get
            {
                lock (this)
                    if (null == _CnDrugProperties1)
                    {
                        _CnDrugProperties1 = EntityUtil.GetTuples(CnDrugString1);
                    }
                return _CnDrugProperties1;
            }
        }


        /// <summary>
        /// 药物组合2。当其为第二诊断时。
        /// 例如:"淡豆豉9栀子9黄连3黄芩9炒酸枣仁15远志9合欢皮15夜交藤30"。
        /// </summary>
        [DataMember]
        [TextFieldName("当其为第二诊断时")]
        public string CnDrugString2 { get; set; }

        List<Tuple<string, decimal>> _CnDrugProperties2;

        /// <summary>
        /// 当其为第二诊断时药物剂量。
        /// 可并发访问。
        /// </summary>
        [NotMapped]
        public List<Tuple<string, decimal>> CnDrugProperties2
        {
            get
            {
                lock (this)
                    if (null == _CnDrugProperties2)
                    {
                        _CnDrugProperties2 = EntityUtil.GetTuples(CnDrugString2);
                    }
                return _CnDrugProperties2;
            }
        }

        /// <summary>
        /// 药物组合3。当其为并列第一诊断时。
        /// 例如:"淡豆豉9栀子9黄连3黄芩9炒酸枣仁15远志9合欢皮15夜交藤30"。
        /// </summary>
        [DataMember]
        [TextFieldName("当其为并列第一诊断时")]
        public string CnDrugString3 { get; set; }

        List<Tuple<string, decimal>> _CnDrugProperties3;

        /// <summary>
        /// 并列第一诊断时药物剂量。
        /// 可并发访问。
        /// </summary>
        [NotMapped]
        public List<Tuple<string, decimal>> CnDrugProperties3
        {
            get
            {
                lock (this)
                    if (null == _CnDrugProperties3)
                    {
                        _CnDrugProperties3 = EntityUtil.GetTuples(CnDrugString3);
                    }
                return _CnDrugProperties3;
            }
        }

    }

    /// <summary>
    /// 药物剂量矫正。
    /// </summary>
    public class InsomniaCnDrugCorrection : ThingEntityBase
    {
        static List<InsomniaCnDrugCorrection> _DefaultCollection;
        static bool _IsInit = false;

        public static List<InsomniaCnDrugCorrection> DefaultCollection
        {
            get
            {
                if (null == _DefaultCollection)
                {
                    var path = System.Web.HttpContext.Current.Server.MapPath("~/content");
                    using (var tdb = new TextFileContext(path))
                    {
                        _DefaultCollection = tdb.GetList<InsomniaCnDrugCorrection>("剂量矫正-失眠.txt");
                    }
                }
                return _DefaultCollection;
            }
        }

        public InsomniaCnDrugCorrection()
        {

        }

        /// <summary>
        /// 所属专病项Id。
        /// </summary>
        [DataMember]
        [ForeignKey(nameof(SpecialCasesItem))]
        public Guid SpecialCasesItemId { get; set; }

        /// <summary>
        /// 所属专病项导航属性。
        /// </summary>
        public virtual InsomniaCasesItem SpecialCasesItem { get; set; }

        /// <summary>
        /// 年龄阈值。按此字段升序排序，最后一个小于该阈值的项被认为匹配。
        /// </summary>
        [DataMember]
        [TextFieldName("年龄")]
        public decimal Age { get; set; }

        /// <summary>
        /// 药物剂量矫正因子。如:0.666666。
        /// </summary>
        [DataMember]
        [TextFieldName("因子")]
        public decimal Factor { get; set; }
    }

    /// <summary>
    /// 药物加味表。
    /// </summary>
    [DataContract]
    public class InsomniaCnDrugConversion2 : ThingEntityBase
    {
        static List<InsomniaCnDrugConversion2> _DefaultCollection;
        static bool _IsInit = false;

        public static List<InsomniaCnDrugConversion2> DefaultCollection
        {
            get
            {
                if (null == _DefaultCollection)
                {
                    var path = System.Web.HttpContext.Current.Server.MapPath("~/content");
                    using (var tdb = new TextFileContext(path))
                    {
                        _DefaultCollection = tdb.GetList<InsomniaCnDrugConversion2>("药物加味-失眠.txt");
                    }
                }
                return _DefaultCollection;
            }
        }

        public InsomniaCnDrugConversion2()
        {

        }

        public override string ToString()
        {
            return $"编号 = {MatchString} ,药物 = { CnDrugString} ";
        }

        /// <summary>
        /// 所属专病项Id。
        /// </summary>
        [DataMember]
        [ForeignKey(nameof(SpecialCasesItem))]
        public Guid SpecialCasesItemId { get; set; }

        /// <summary>
        /// 所属专病项导航属性。
        /// </summary>
        public virtual InsomniaCasesItem SpecialCasesItem { get; set; }

        /// <summary>
        /// 症候编号。例如:"3，100，104，113，122"。
        /// </summary>
        [DataMember]
        [TextFieldName("编号")]
        public string MatchString { get; set; }

        private List<string> _Numbers;

        /// <summary>
        /// 编号列表。
        /// 可并发访问。
        /// </summary>
        [NotMapped]
        public List<string> Numbers
        {
            get
            {
                lock (this)
                    if (null == _Numbers)
                        _Numbers = EntityUtil.GetArray(MatchString);
                return _Numbers;
            }
        }


        /// <summary>
        /// 脏腑。目前这个字段冗余无用。
        /// </summary>
        [DataMember]
        public string CnVisceral { get; set; }

        /// <summary>
        /// 证型结论。目前该字段冗余无用。
        /// </summary>
        [DataMember]
        public string CnPhenomenon { get; set; }

        /// <summary>
        /// 输出诊断。
        /// 例如:"心火亢盛"。
        /// </summary>
        [DataMember]
        [TextFieldName("症候")]
        public string CnMedicineConclusions { get; set; }

        /// <summary>
        /// 药物组合1。当其为第一诊断时。
        /// 例如:"淡豆豉9栀子9黄连3黄芩9炒酸枣仁15远志9合欢皮15夜交藤30"。
        /// </summary>
        [DataMember]
        [TextFieldName("药物")]
        public string CnDrugString { get; set; }

        List<Tuple<string, decimal>> _CnDrugProperties;

        /// <summary>
        /// 药物剂量。
        /// 可并发访问。
        /// </summary>
        [NotMapped]
        public List<Tuple<string, decimal>> CnDrugProperties
        {
            get
            {
                lock (this)
                    if (null == _CnDrugProperties)
                    {
                        _CnDrugProperties = EntityUtil.GetTuples(CnDrugString);
                    }
                return _CnDrugProperties;
            }
        }

        /// <summary>
        /// 药物组合2。当其为第二诊断时。
        /// 例如:"淡豆豉9栀子9黄连3黄芩9炒酸枣仁15远志9合欢皮15夜交藤30"。
        /// </summary>
        [DataMember]
        public string CnDrugString2 { get; set; }

        /// <summary>
        /// 药物组合3。当其为并列第一诊断时。
        /// 例如:"淡豆豉9栀子9黄连3黄芩9炒酸枣仁15远志9合欢皮15夜交藤30"。
        /// </summary>
        [DataMember]
        public string CnDrugString3 { get; set; }
    }


}