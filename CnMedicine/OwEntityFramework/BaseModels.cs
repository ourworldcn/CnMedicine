using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OW.Data.Entity
{
    public class EntityUtility
    {
        /// <summary>
        /// 捕获模式字符串。如：:生地黄-9,玄参9，天冬15;麦冬(醋熏）15;丹参（后下）9。当归9、党参9茯神15炒酸枣仁15远志6五味子6龙骨（醅)-30
        /// </summary>
        public const string KvPatternString = @"[\p{Po}\s]*(?<name>.*?)[\s]*(?<value>[\+\-]?\d+)";

        /// <summary>
        /// 分开数组的正则字符串。
        /// </summary>
        public const string ListPatternString = @"[\p{Po}\s]*(?<name>[^\p{Po}\s]*)[\s]*";

        /// <summary>
        /// 将字符串拆分为二元组。
        /// </summary>
        /// <param name="guts">如：xxx-1,sss+1。第三1。</param>
        /// <returns></returns>
        public static List<Tuple<string, decimal>> GetTuples(string guts)
        {
            if (string.IsNullOrWhiteSpace(guts))
                return new List<Tuple<string, decimal>>();
            List<Tuple<string, decimal>> result = new List<Tuple<string, decimal>>();
            var matches = Regex.Matches(guts, KvPatternString);

            foreach (Match match in matches)
            {
                var group = match.Groups["name"];
                if (!group.Success)
                    continue;
                string name = group.Value;
                group = match.Groups["value"];
                if (!group.Success || !decimal.TryParse(group.Value, out decimal tmp))
                    continue;
                result.Add(Tuple.Create(name, tmp));
            }
            return result;
        }

        /// <summary>
        /// 使用模式 <see cref="ListPatternString"/> 进行分组。
        /// </summary>
        /// <param name="guts"></param>
        /// <returns></returns>
        public static List<string> GetArray(string guts)
        {
            List<string> result = new List<string>();
            var matches = Regex.Matches(guts, ListPatternString);

            foreach (Match match in matches)
            {
                var group = match.Groups["name"];
                if (!group.Success)
                    continue;
                string name = group.Value;
                result.Add(name);
            }
            return result;
        }

        static ConcurrentDictionary<ValueTuple<Type, Type>, List<ValueTuple<PropertyDescriptor, PropertyDescriptor>>> CopyToDic = new ConcurrentDictionary<(Type, Type), List<(PropertyDescriptor, PropertyDescriptor)>>();

        /// <summary>
        /// 将源对象所有与目标对象同名的属性复制到目标对象。
        /// </summary>
        /// <param name="source">源对象。</param>
        /// <param name="dest">目标对象。</param>
        /// <param name="excludes">排除的属性。多个属性名用逗号分开。</param>
        public static void CopyTo(object source, object dest, string excludes = null)
        {
            var key = ValueTuple.Create(source.GetType(), dest.GetType());
            var list = CopyToDic.GetOrAdd(key, c =>
            {
                var l = TypeDescriptor.GetProperties(c.Item1).OfType<PropertyDescriptor>();
                var r = TypeDescriptor.GetProperties(c.Item2).OfType<PropertyDescriptor>().Where(subc => !subc.IsReadOnly);
                var result = l.Join(r, c1 => c1.Name, c1 => c1.Name, (c1, c2) => ValueTuple.Create(c1, c2));
                return result.ToList();
            });
            HashSet<string> hs = null;
            if (!string.IsNullOrWhiteSpace(excludes))
                hs = new HashSet<string>(excludes.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
            foreach (var item in list)
            {
                if (hs?.Contains(item.Item1.Name) ?? false)
                    continue;
                item.Item2.SetValue(dest, item.Item1.GetValue(source));
            }
        }

    }

    /// <summary>
    /// 以 Guid 为主键的实体类基类。
    /// </summary>
    [DataContract]
    public class EntityWithGuid
    {
        /// <summary>
        /// 表示一种不在数据库中建立外键关系的连接。使用<see cref="OwAdditionalAttribute"/>标注，这里是其Name的内容。
        /// </summary>
        public const string WeakAssociationName = "75A29B4D-A39E-46DA-8056-1F2FBF2A2A58";

        /// <summary>
        /// 加载隐式连接的属性。
        /// </summary>
        /// <param name="context"></param>
        public void LoadWeakAssociation(DbContext context)
        {
            var peoperties = TypeDescriptor.GetProperties(this).OfType<PropertyDescriptor>();
            var coll = from tmp in peoperties
                       where typeof(ICollection).IsAssignableFrom(tmp.PropertyType)
                       let attr = tmp.PropertyType.GetCustomAttributes<OwAdditionalAttribute>().Where(c => c.Name == WeakAssociationName).FirstOrDefault()
                       where null != attr
                       select tmp;

        }

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

        /// <summary>
        /// 如果Id是空（Guid.Empty）则生成新Id。
        /// </summary>
        /// <returns>true生成了新Id,false没有生成新Id。</returns>
        [Description("如果Id是空（Guid.Empty）则生成新Id。")]
        public bool GeneratedIdIfEmpty()
        {
            if (Id != Guid.Empty)
                return false;
            Id = Guid.NewGuid();
            return true;
        }
    }

    /// <summary>
    /// 标识事物的实体基类。封装一些共有属性。
    /// </summary>
    [DataContract]
    [Description("标识事物的实体基类。封装一些共有属性。")]
    public class ThingEntityBase : EntityWithGuid
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        public ThingEntityBase()
        {

        }

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="id">指定该实体对象的Id。</param>
        public ThingEntityBase(Guid id) : base(id)
        {

        }

        /// <summary>
        /// 名称。
        /// </summary>
        [DataMember]
        [Description("名称。")]
        public string Name { get; set; }

        /// <summary>
        /// 一般性简称。最多8个字符。
        /// </summary>
        [MaxLength(8)]
        [DataMember]
        [Description("一般性简称。最多8个字符。")]
        public string ShortName { get; set; }

        /// <summary>
        /// 一般性描述信息。
        /// </summary>
        [DataMember]
        [Description("一般性描述信息。")]
        public string Description { get; set; }

        /// <summary>
        /// 创建的时间。注意使用UTC时间！
        /// </summary>
        [DataMember]
        [Description("创建的时间。注意使用UTC时间！")]
        public DateTime CreateUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 描述事物某个属性的对象。这个对象记录的信息服务器不加理解，仅供使用接口的程序使用。
        /// </summary>
        [NotMapped]
        [DataMember]
        [Description("描述事物某个属性的对象。这个对象记录的信息服务器不加理解，仅供使用接口的程序使用。")]
        public virtual List<ThingPropertyItem> ThingPropertyItems { get; set; }

        /// <summary>
        /// 异步获取该实体对象的附属扩展信息。
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task LoadThingPropertyItemsAsync(DbContext context)
        {
            return context.Set<ThingPropertyItem>().Where(c => c.ThingEntityId == Id).ToListAsync().ContinueWith(c =>
            {
                ThingPropertyItems = c.Result;
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        /// <summary>
        /// 异步保存该实体对象的扩展信息。这不是真的向数据库写入，仅仅将对象追加到指定上下文。
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task SaveThingPropertyItemsAsync(DbContext context)
        {
            if (null != ThingPropertyItems)
            {
                return Task.Run(() => { context.Set<ThingPropertyItem>().AddRange(ThingPropertyItems); });
            }
            else
                return Task.CompletedTask;
        }
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
        [Description("属性名称。只能是字符串。最长32字符。")]
        public string Name { get; set; }

        /// <summary>
        /// 属性的值。只能是字符串。最长256字符。
        /// </summary>
        [DataMember]
        [MaxLength(256)]
        [Description("属性的值。只能是字符串。最长256字符。")]
        public string Value { get; set; }

        /// <summary>
        /// 所附属的实体的Id。
        /// 在实体中传送可忽略。
        /// </summary>
        [Index]
        [DataMember(IsRequired = false)]
        [Description("所附属的实体的Id。在实体中传送可忽略。")]
        public Guid ThingEntityId { get; set; }

        /// <summary>
        /// 排序号。同一个实体的多个扩展属性按此字段升序排序。序号不必连续，相等序号顺序随机。
        /// </summary>
        [DataMember]
        [Description("排序号。同一个实体的多个扩展属性按此字段升序排序。序号不必连续，相等序号顺序随机。")]
        public int OrderNum { get; set; } = 0;
    }

}

