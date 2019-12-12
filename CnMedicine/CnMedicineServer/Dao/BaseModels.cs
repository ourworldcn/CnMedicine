using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text;

namespace OW.Data.Entity
{

    /// <summary>
    /// 以 Guid 为主键的实体类基类。
    /// </summary>
    [DataContract]
    public class EntityWithGuid
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        public EntityWithGuid()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="id"></param>
        public EntityWithGuid(Guid id)
        {
            Id = id;
        }

        /// <summary>
        /// 实体类的主键。
        /// </summary>
        [Key]
        [DataMember(IsRequired = false)]    //默认全零
        [Description("实体类的主键。")]
        public Guid Id { get; set; }
    }

    /// <summary>
    /// 标识事物的实体基类。封装一些共有属性。
    /// </summary>
    [DataContract]
    public class ThingEntityBase : EntityWithGuid
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        public ThingEntityBase()
        {

        }

        /// <summary>
        /// 名称。
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// 一般性简称。最多8个字符。
        /// </summary>
        [MaxLength(8)]
        [DataMember]
        public string ShortName { get; set; }

        /// <summary>
        /// 一般性描述信息。
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// 创建的时间。注意使用UTC时间！
        /// </summary>
        [DataMember]
        public DateTime CreateUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 描述事物某个属性的对象。这个对象记录的信息服务器不加理解，仅供使用接口的程序使用。
        /// </summary>
        [DataMember]
        [NotMapped]
        public virtual List<ThingPropertyItem> ThingPropertyItems { get; set; }
    }

    /// <summary>
    /// 描述事物某个属性的对象。
    /// 需要注意：这个对象记录的信息服务器不加理解，仅供使用接口的程序使用，换言之服务器不能使用该属性处理逻辑。该属性不受权限控制请注意不要放置敏感信息。
    /// </summary>
    [DataContract]
    public class ThingPropertyItem : EntityWithGuid
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        public ThingPropertyItem()
        {

        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="id"></param>
        public ThingPropertyItem(Guid id) : base(id)
        {

        }

        /// <summary>
        /// 属性名称。只能是字符串。最长32字符。
        /// </summary>
        [DataMember]
        [MaxLength(32)]
        public string Name { get; set; }

        /// <summary>
        /// 属性的值。只能是字符串。最长256字符
        /// </summary>
        [DataMember]
        [MaxLength(256)]
        public string Value { get; set; }

        /// <summary>
        /// 所附属的实体的Id。
        /// 在实体中传送可忽略。
        /// </summary>
        [Index]
        [DataMember(IsRequired =false)]
        public Guid ThingEntityId { get; set; }

        /// <summary>
        /// 排序号。同一个实体的多个扩展属性按此字段升序排序。序号不必连续，相等序号顺序随机。
        /// </summary>
        [DataMember]
        public int OrderNum { get; set; } = 0;
    }

    /// <summary>
    /// 行政区域。展平，即每个村都有一条独立记录，完整的记录其省市县乡村；北京市也独立占一条，除NameOfLv1之外均为空。
    /// 该表很少变更，会大量查询。之所以这么设计也为了对应在数据超市里面购买数据导入方便，已知18万数据规模的约￥600。
    /// 截至到2013年上半年，全国范围行政村总数为691510个 ，14677个乡，19531个镇。
    /// 全国县级以上行政区划共有：23个省，5个自治区，4个直辖市，2个特别行政区；43个地区（州、盟）；290个地级市；1636个县（自治县、旗、自治旗、特区和林区），374个县级市，852个市辖区。总计：省级34个，地级333个，县级2862个。
    /// 全国乡镇级以下行政区划共有：2个区公所，19531个镇，14677个乡，181个苏木，1092 个民族乡，1个民族苏木，6152个街道。总计：41636个。
    /// </summary>
    [DataContract]
    public class AddressDictionary : EntityWithGuid
    {

        /// <summary>
        /// 邮政编码。最多10个字符，略作余量。
        /// </summary>
        [MaxLength(10)]
        [Index]
        [Display(Name = "邮政编码")]
        [DataMember]
        public string PostalCode { get; set; }

        /// <summary>
        /// 简称。对辽宁省这条记录简称是辽，对辽宁省沈阳市这条记录的简称是沈，没有简称的应空出。最多10个字符，略作余量。
        /// </summary>
        [MaxLength(8)]
        [Display(Name = "简称")]
        [DataMember]
        public string ShortName { get; set; }

        /// <summary>
        /// 一级行政区的名字。如：内蒙古自治区。
        /// </summary>
        [MaxLength(32)]
        [Index]
        [DataMember]
        public string NameOfLv1 { get; set; }

        /// <summary>
        /// 二级行政区的名字。如：福州市。
        /// </summary>
        [MaxLength(32)]
        [Index]
        [DataMember]
        public string NameOfLv2 { get; set; }

        /// <summary>
        /// 三级行政区的名字。如：龙泉驿区。
        /// </summary>
        [MaxLength(32)]
        [Index]
        [DataMember]
        public string NameOfLv3 { get; set; }

        /// <summary>
        /// 四级行政区的名字。一般没有。也能对应到有独立邮政编码的乡镇。如龙潭镇,或XX街道/地区(对应行政区划设置)。
        /// </summary>
        [MaxLength(32)]
        [Index]
        [DataMember]
        public string NameOfLv4 { get; set; }

        /// <summary>
        /// 五级行政区的名字。罕见且正在变少。也能对应到有独立邮政编码的自然村。如王家沟村。
        /// </summary>
        [MaxLength(32)]
        [Index]
        [DataMember]
        public string NameOfLv5 { get; set; }

        [NonSerialized]
        private string _ToString;
        /// <summary>
        ///  返回该地址的字符串。
        /// </summary>
        /// <returns>地址字符串。</returns>
        public override string ToString()
        {
            if (null == _ToString)
            {
                StringBuilder result = new StringBuilder(NameOfLv1);

                if (!string.IsNullOrWhiteSpace(NameOfLv2))
                    result.Append(NameOfLv2);
                if (!string.IsNullOrWhiteSpace(NameOfLv3))
                    result.Append(NameOfLv3);
                if (!string.IsNullOrWhiteSpace(NameOfLv4))
                    result.Append(NameOfLv4);
                if (!string.IsNullOrWhiteSpace(NameOfLv5))
                    result.Append(NameOfLv5);
                _ToString = result.ToString();
            }
            return _ToString;
        }
    }

    /// <summary>
    /// 详细地址。
    /// </summary>
    [DataContract]
    public class DetailedAddress : EntityWithGuid
    {
        private string _PostalCode;

        /// <summary>
        /// 行政区域的导航属性。接口不要使用此属性设置数据。
        /// </summary>
        [ForeignKey(nameof(AddressDictionaryId))]
        public virtual AddressDictionary AddressDictionary { get; set; }

        /// <summary>
        /// 行政区域的Id。
        /// </summary>
        [DataMember]
        public Guid AddressDictionaryId { get; set; }

        /// <summary>
        /// 详细地址。不要重复行政区域地址已经包含的部分。
        /// </summary>
        [DataMember]
        public string Detailed { get; set; }

        private string _DisplayName;

        /// <summary>
        /// 显示地址，这个属性可以为空，自动生成为 行政区域地址 详细地址。但一些情况可能需要强制设置显示地址。
        /// </summary>
        [DataMember]
        public string DisplayName
        {
            get
            {
                return _DisplayName ?? AddressDictionary.ToString() + Detailed;
            }
            set
            {
                _DisplayName = value;
            }
        }

        /// <summary>
        /// 邮政编码。最多10个字符，略作余量。特殊情况可以用这个属性覆盖行政区的邮政编码。一般空着就可以。如果不填写则使用行政区域的邮政编码填充。
        /// </summary>
        [MaxLength(10)]
        [DataMember]
        public string PostalCode
        {
            get { return _PostalCode ?? AddressDictionary.PostalCode; }
            set { _PostalCode = value; }
        }

        public override string ToString()
        {
            return $"{DisplayName}({PostalCode})";
        }
    }

    /// <summary>
    /// 法人团体。
    /// </summary>
    [DataContract]
    public class DbCompany : ThingEntityBase
    {
        public DbCompany()
        {

        }

        /// <summary>
        /// 银行基本户信息。
        /// </summary>
        public virtual BankAccount MainBankAccount { get; set; }

        /// <summary>
        /// 银行非基本户信息。
        /// </summary>
        [ForeignKey(nameof(BankAccount.CompanyId))]
        public virtual List<BankAccount> BankAccounts { get; set; }

        /// <summary>
        /// 注册地。
        /// </summary>
        public virtual DetailedAddress RegisteredAddress { get; set; }

        /// <summary>
        /// 法人团体的类型，如"有限责任公司"，可选。
        /// </summary>
        [DataMember]
        public string TypeString { get; set; }

        /// <summary>
        /// 法人的Id。可选。
        /// </summary>
        [MaxLength(128)]
        [DataMember]
        public string CorporateId { get; set; }

        /// <summary>
        /// 注册地址Id。可选。
        /// </summary>
        [ForeignKey(nameof(RegisteredAddress))]
        [DataMember]
        public Guid? RegisteredAddressId { get; set; }

        /// <summary>
        /// 注册资本。可选。
        /// </summary>
        [DataMember]
        public string Capital { get; set; }

        /// <summary>
        /// 营业期限中起始时间。可选。
        /// </summary>
        [DataMember]
        public DateTime OpenDateTime { get; set; }

        /// <summary>
        /// 营业期限中终止时间。空着标识永久。
        /// </summary>
        [DataMember]
        public DateTime? ClosedDateTime { get; set; }

        /// <summary>
        /// 经营范围的描述。可选。
        /// </summary>
        [DataMember]
        public string Scope { get; set; }
    }

    /// <summary>
    /// 银行账号。
    /// </summary>
    [DataContract]
    public class BankAccount : EntityWithGuid
    {
        /// <summary>
        /// 银行名称。
        /// </summary>
        [DataMember]
        public string BankName { get; set; }

        /// <summary>
        /// 开户行名称。
        /// </summary>
        [DataMember]
        public string ManangerBankName { get; set; }

        /// <summary>
        /// 账号代码。
        /// </summary>
        [DataMember]
        public string AccountCode { get; set; }

        /// <summary>
        /// 所属公司Id。
        /// </summary>
        [DataMember]
        public Guid? CompanyId { get; set; }
    }

    /// <summary>
    /// 联系人。该数据结构记录了人员简要信息，他们不是系统用户，如社会关系等。
    /// 新增时 Id 设置为:00000000-0000-0000-0000-000000000000。
    /// </summary>
    [DataContract]
    public class Contact : ThingEntityBase
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        public Contact()
        {

        }

        /// <summary>
        /// 关系名。与用户的关系，如 夫妻，证明人，父亲 等，因上下文而不同。最长16字符。
        /// </summary>
        [DataMember]
        [MaxLength(16)]
        public string RelationshipName { get; set; }

        /// <summary>
        /// 工作单位/所处组织名称。最长32字符。
        /// </summary>
        [DataMember]
        [MaxLength(32)]
        public string OrganizationName { get; set; }

        /// <summary>
        /// 联系方式。最长16字符。
        /// </summary>
        [DataMember]
        [MaxLength(16)]
        public string Communication { get; set; }

        /// <summary>
        /// 所属的用户Id。可选，最长128字符。
        /// </summary>
        [DataMember]
        [MaxLength(128)]
        [Index]
        public string UserId { get; set; }

        /// <summary>
        /// 所属简单机构的Id。可能是证明人。可选。
        /// </summary>
        [DataMember]
        [Index]
        public Guid? SimpleOrganizationId { get; set; }

    }

    /// <summary>
    /// 简单组织机构的类型。
    /// </summary>
    public enum SimpleOrganizationCatalog : short
    {
        Unknown = 0,

        /// <summary>
        /// 工作经历。
        /// </summary>
        Job = 1,

        /// <summary>
        /// 教育经历。
        /// </summary>
        Education = 2,
    }

    /// <summary>
    /// 曾经所从属的组织机构。可能是员工的职业经历也可能是教育经历。
    /// 新增时 Id 设置为:00000000-0000-0000-0000-000000000000。
    /// </summary>
    [DataContract]
    public class SimpleOrganization : ThingEntityBase
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        public SimpleOrganization()
        {
        }

        /// <summary>
        /// 职务/岗位。可选，最长32字符。
        /// </summary>
        [DataMember]
        [MaxLength(32)]
        public string RoleName { get; set; }

        /// <summary>
        /// 起始时间。可选，空表示未知。
        /// 若无法区分起止日期，可以使用 Description 记录。
        /// </summary>
        [DataMember]
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// 终止时间。可选。
        /// 若无法区分起止日期，可以使用 Description 记录。
        /// </summary>
        [DataMember]
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// 所属的用户Id。可选，最长128字符。
        /// </summary>
        [DataMember]
        [MaxLength(128)]
        [Index]
        public string UserId { get; set; }

        /// <summary>
        /// 简单组织机构的类型。
        /// </summary>
        [Index]
        public SimpleOrganizationCatalog Catalog { get; set; }
    }

    /// <summary>
    /// 作息时间表。
    /// </summary>
    public class RestSchedule
    {

    }

    /// <summary>
    /// 标识日历每天的类型如是否上班。
    /// </summary>
    [Serializable]
    public enum CalendarItemKind : short
    {

        /// <summary>
        /// 上班。
        /// </summary>
        上班 = 1,

        /// <summary>
        /// 休息。
        /// </summary>
        休息 = 2,

    }


    /// <summary>
    /// 日历的数据项详细信息。
    /// </summary>
    [ComplexType]
    [DataContract]
    public class CalendarItem : ICloneable
    {
        /// <summary>
        /// 获取工作时长。如果是休息日，则返回 TimeSpan.Zero。
        /// </summary>
        [NotMapped]
        public TimeSpan Duration
        {
            get
            {
                if (Kind == CalendarItemKind.休息)
                    return TimeSpan.Zero;
                return End.Value - Start.Value;
            }
        }

        /// <summary>
        /// 日历项的类型。
        /// 具体参加所属类，在不同类中时有不同的解释。
        /// </summary>
        [DataMember]
        public CalendarItemKind Kind { get; set; }

        /// <summary>
        /// 上班时间。
        /// 具体参加所属类，在不同类中时有不同的解释。
        /// </summary>
        [DataMember]
        [Index]
        public DateTime? Start { get; set; }

        /// <summary>
        /// 下班时间。
        /// 具体参加所属类，在不同类中时有不同的解释。
        /// </summary>
        [DataMember]
        [DataType(DataType.Time)]
        public DateTime? End { get; set; }

        /// <summary>
        /// 弹性工作偏移时间，默认是空表示不允许，如果设置2:0:0表示允许最晚迟两小时，此时只要上下班的总工作时间没有少则就算正常出勤。
        /// </summary>
        [DataMember]
        public TimeSpan? FlexibleTime { get; set; }

        /// <summary>
        /// 迟到容忍度。不准迟到则设置null或为:0:0:0,接受最多5分钟的迟到不算迟到则设置 0:5:0,默认无容忍度。
        /// </summary>
        [DataMember]
        public TimeSpan? LatetoLerance { get; set; }

        /// <summary>
        /// 薪酬因子，平时默认是1，周末可能是2，节假日是3。免费加班时，设置0。
        /// </summary>
        [DataMember]
        public float? PayFactor { get; set; } = 1;

        /// <summary>
        /// 最大容忍迟到早退的时长，当日迟到早退合计大于该时长则算矿工。
        /// </summary>
        [DataMember]
        public TimeSpan MaxLerance { get; set; }

        /// <summary>
        /// 获得该对象的深表副本。
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var me = this;
            var result = new CalendarItem()
            {
                Kind = me.Kind,
                Start = me.Start,
                End = me.End,
                FlexibleTime = me.FlexibleTime,
                LatetoLerance = me.LatetoLerance,
                PayFactor = me.PayFactor,
                MaxLerance = me.MaxLerance,
            };
            return result;
        }
    }

    /// <summary>
    /// 默认的工作日历。
    /// </summary>
    [DataContract]
    public class DefaultCalendar : ThingEntityBase
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        public DefaultCalendar()
        {
        }

        /// <summary>
        /// 获取指定星期天数的日历。
        /// </summary>
        /// <param name="dayOfWeek">获取哪天的默认日历。</param>
        /// <returns>日历。</returns>
        [NotMapped]
        public CalendarItem this[DayOfWeek dayOfWeek]
        {
            get
            {
                CalendarItem result = null;
                switch (dayOfWeek)
                {
                    case DayOfWeek.Sunday:
                        result = Sunday;
                        break;
                    case DayOfWeek.Monday:
                        result = Monday;
                        break;
                    case DayOfWeek.Tuesday:
                        result = Tuesday;
                        break;
                    case DayOfWeek.Wednesday:
                        result = Wednesday;
                        break;
                    case DayOfWeek.Thursday:
                        result = Thursday;
                        break;
                    case DayOfWeek.Friday:
                        result = Friday;
                        break;
                    case DayOfWeek.Saturday:
                        result = Saturday;
                        break;
                    default:
                        break;
                }
                return result;
            }
        }

        /// <summary>
        /// 行版本。表太宽一般就加上。
        /// </summary>
        [Timestamp]
        public byte[] RowVision { get; set; }

        /// <summary>
        /// 星期一的默认作息时间。
        /// 其中上班时间仅取Start的时间部分，而上班长度则是 End - Start的结果，这样可以应对跨自然天的作息。
        /// </summary>
        [DataMember]
        public CalendarItem Monday { get; set; }

        /// <summary>
        /// 星期二的默认作息时间。
        /// 其中上班时间仅取Start的时间部分，而上班长度则是 End - Start的结果，这样可以应对跨自然天的作息。
        /// </summary>
        [DataMember]
        public CalendarItem Tuesday { get; set; }

        /// <summary>
        /// 星期三的默认作息时间。
        /// 其中上班时间仅取Start的时间部分，而上班长度则是 End - Start的结果，这样可以应对跨自然天的作息。
        /// </summary>
        [DataMember]
        public CalendarItem Wednesday { get; set; }

        /// <summary>
        /// 星期四的默认作息时间。
        /// 其中上班时间仅取Start的时间部分，而上班长度则是 End - Start的结果，这样可以应对跨自然天的作息。
        /// </summary>
        [DataMember]
        public CalendarItem Thursday { get; set; }

        /// <summary>
        /// 星期五的默认作息时间。
        /// 其中上班时间仅取Start的时间部分，而上班长度则是 End - Start的结果，这样可以应对跨自然天的作息。
        /// </summary>
        [DataMember]
        public CalendarItem Friday { get; set; }

        /// <summary>
        /// 星期六的默认作息时间。
        /// 其中上班时间仅取Start的时间部分，而上班长度则是 End - Start的结果，这样可以应对跨自然天的作息。
        /// </summary>
        [DataMember]
        public CalendarItem Saturday { get; set; }

        /// <summary>
        /// 星期日的默认作息时间。
        /// 其中上班时间仅取Start的时间部分，而上班长度则是 End - Start的结果，这样可以应对跨自然天的作息。
        /// </summary>
        [DataMember]
        public CalendarItem Sunday { get; set; }

    }

    /// <summary>
    /// 特别日历，用于指定换休法定节假日。
    /// </summary>
    [DataContract]
    public class SpecialCalendar : ThingEntityBase, ICloneable
    {
        /// <summary>
        /// 从默认日历转换一个新的特别日历。日期是指定日期。
        /// </summary>
        /// <param name="defaultCalendar"></param>
        /// <param name="today">仅日期部分有效。</param>
        /// <returns></returns>
        public static SpecialCalendar FromDefaultCalendar(DefaultCalendar defaultCalendar, DateTime today)
        {
            today = today.Date;
            var defaultCalendarItem = defaultCalendar[today.DayOfWeek];
            var result = new SpecialCalendar()
            {
                CreateUtc = defaultCalendar.CreateUtc,
                Description = defaultCalendar.Description,
                Name = defaultCalendar.Name,
                ShortName = defaultCalendar.ShortName,
            };
            var ci = (CalendarItem)defaultCalendarItem.Clone();


            if (ci.Kind == CalendarItemKind.上班)   //若是工作日
            {
                var dur = ci.Duration;
                ci.Start = today + ci.Start.Value.TimeOfDay;
                ci.End = ci.Start.Value + dur;
            }
            else if (ci.Kind == CalendarItemKind.休息)  //若是休息日
            {
                var dur = ci.End.Value - ci.Start.Value;
                ci.Start = today + ci.Start.Value.TimeOfDay;
                ci.End = ci.Start + dur;
            }
            result.CalendarItem = ci;
            return result;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public SpecialCalendar()
        {

        }

        /// <summary>
        /// 具体设置，参见 CalendarItem 说明。是工作日时，其中Start是上班时间包含日期和时间，End - Start是工作时长。如果是休息日则Start仅日期部分有效。
        /// </summary>
        [DataMember]
        public CalendarItem CalendarItem { get; set; }

        /// <summary>
        /// 所属组织机构。
        /// </summary>
        [DataMember]
        public Guid OrganizationId { get; set; }

        /// <summary>
        /// 获取当前对象的深表副本。
        /// </summary>
        /// <returns>当前对象的深表副本。</returns>
        public object Clone()
        {
            var me = this;
            SpecialCalendar result = new SpecialCalendar()
            {
                CalendarItem = (CalendarItem)me.CalendarItem.Clone(),
                CreateUtc = me.CreateUtc,
                Id = me.Id,
                Name = me.Name,
                OrganizationId = me.OrganizationId,
                ShortName = me.ShortName,
            };
            return result;
        }
    }


}

