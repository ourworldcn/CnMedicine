using OW.Data.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace CnMedicineServer.Models
{
    /// <summary>
    /// 鼻炎评分表。
    /// </summary>
    [DataContract]
    [DebuggerDisplay("编号 = {CnSymptomNumber} ,脏腑 = ({CnVisceralScore}) ,证型 = ({CnPhenomenonScore}),病机评分 = ({CnPathogenScore})")]
    public class RhinitisConversion : ThingEntityBase
    {
        static List<RhinitisConversion> _DefaultCollection;
        static bool _IsInit = false;

        public static List<RhinitisConversion> DefaultCollection
        {
            get
            {
                if (null == _DefaultCollection)
                {
                    var path = System.Web.HttpContext.Current.Server.MapPath("~/content/鼻炎");
                    using (var tdb = new TextFileContext(path))
                    {
                        _DefaultCollection = tdb.GetList<RhinitisConversion>("评分表-鼻炎.txt");
                    }

                }
                return _DefaultCollection;
            }
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        public RhinitisConversion()
        {

        }

        public override string ToString()
        {
            return $"编号 = {CnSymptomNumber} ,脏腑评分 = ({CnVisceralScore}) ,证型评分 = ({CnPhenomenonScore}),病机评分 = ({CnPathogenScore})";
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
        public virtual RhinitisCasesItem SpecialCasesItem { get; set; }

        /// <summary>
        /// 症候编号。例如:"3"。
        /// </summary>
        [DataMember]
        [TextFieldName("编号")]
        public string CnSymptomNumber { get; set; }

        /// <summary>
        /// 症候。例如：“嗅觉减退”。
        /// </summary>
        [DataMember]
        [TextFieldName("症候")]
        public string CnSymptom { get; set; }

        /// <summary>
        /// 脏腑评分。例如:"肺+3、、脾+3、肝+2、胆+2、脾+3、胃+2、肾+3、膀胱+1"鼻炎脏腑共涉及8脏腑。
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
        /// 证型评分。例如:"气郁+3、痰+3、湿+3、湿热+2、阴虚+2"。
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


        /// <summary>
        /// 病机评分。例如:"排脓+1、止流+1、积食+1、软坚化结+1、安神+3、通窍+1"。
        /// 此处尤其注意通窍、安神的输出时要做很多单独判断
        /// </summary>
        [DataMember]
        [TextFieldName("病机评分")]
        public string CnPathogenScore { get; set; }

        List<Tuple<string, decimal>> _CnPathogenProperties;

        /// <summary>
        /// 病机评分。
        /// 可并发访问。
        /// </summary>
        [NotMapped]
        public List<Tuple<string, decimal>> CnPathogenProperties
        {
            get
            {
                lock (this)
                    if (null == _CnPathogenProperties)
                    {
                        _CnPathogenProperties = EntityUtil.GetTuples(CnPathogenScore);
                    }
                return _CnPathogenProperties;
            }
        }

    }

    
    /// <summary>
    ///鼻炎 药物输出表。
    /// </summary>
    [DataContract]
    public class RhinitisCnDrugConversion : ThingEntityBase
    {
        static List<RhinitisCnDrugConversion> _DefaultCollection;
        static bool _IsInit = false;

        public static List<RhinitisCnDrugConversion> DefaultCollection
        {
            get
            {
                if (null == _DefaultCollection)
                {
                    var path = System.Web.HttpContext.Current.Server.MapPath("~/content/鼻炎");
                    using (var tdb = new TextFileContext(path))
                    {
                        _DefaultCollection = tdb.GetList<RhinitisCnDrugConversion>("药物输出-鼻炎.txt");
                    }
                }
                return _DefaultCollection;
            }
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        public RhinitisCnDrugConversion()
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
        public virtual RhinitisCasesItem SpecialCasesItem { get; set; }

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
        /// 输出诊断,鼻炎诊断输出暂时未空。
        /// </summary>
        [DataMember]
        [TextFieldName("输出诊断")]
        public string CnMedicineConclusions { get; set; }

        /// <summary>
        /// 药物组合。当其为所有诊断时。
        /// 例如:"杏仁10，生苡仁30，白蔻仁5（后下），滑石10（包煎），通草5，淡竹叶10"。
        /// </summary>
        [DataMember]
        [TextFieldName("当其为所有诊断时")]
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

    }

    
    /// <summary>
    /// 鼻炎病机药物输出表。
    /// </summary>
    [DataContract]
    public class RhinitisCnDrugPathogen : ThingEntityBase
    {
        static List<RhinitisCnDrugPathogen> _DefaultCollection;
        static bool _IsInit = false;

        public static List<RhinitisCnDrugPathogen> DefaultCollection
        {
            get
            {
                if (null == _DefaultCollection)
                {
                    var path = System.Web.HttpContext.Current.Server.MapPath("~/content/鼻炎");
                    using (var tdb = new TextFileContext(path))
                    {
                        _DefaultCollection = tdb.GetList<RhinitisCnDrugPathogen>("病机药物输出-鼻炎.txt");
                    }
                }
                return _DefaultCollection;
            }
        }

        public RhinitisCnDrugPathogen()
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
        public virtual RhinitisCasesItem SpecialCasesItem { get; set; }

        /// <summary>
        /// 编号此处编号为内部定义，不和症候做任何关联。例如:编号为"3"代表通窍》=6的输出。
        /// 例如调用时判断基础输出中有脏腑心的输出，并且安神>0，即可输出编号为15的药物
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
        /// 例如:"辛夷5，白芷5"。
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
       
    }


}