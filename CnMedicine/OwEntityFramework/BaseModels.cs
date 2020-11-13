using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#pragma warning disable CS3021 // 由于程序集没有 CLSCompliant 特性，因此类型或成员不需要 CLSCompliant 特性

namespace OW.Data.Entity
{
    public class EntityUtility
    {
        /// <summary>
        /// 捕获模式字符串。如：生地黄-9,玄参9g，天冬15;麦冬(醋熏）15;丹参（后下）9。当归9、党参9茯神15炒酸枣仁15远志6五味子6龙骨（醅)-30
        /// </summary>
        public const string KvPatternString = @"[\p{Po}\s]*(?<name>.*?)[\s]*(?<value>[\+\-]?\d+)[g]?";

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
            if (string.IsNullOrEmpty(guts)) //若是空字符串
                return new List<string>();
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

        public const string ArrayWithPowerPatternString = @"[\s\p{Po}-[\*]]*?(?<name>[\d\s\*\.]+)";

        /// <summary>
        /// 获取类似以下字符串中数组，"3104,1101,1202*3,1301,1502,5202,4107*2,4415,1703。"
        /// </summary>
        /// <returns></returns>
        public static List<Tuple<int, float>> GetArrayWithPower(string inputs)
        {
            List<Tuple<int, float>> result = new List<Tuple<int, float>>();
            var matches = Regex.Matches(inputs, ArrayWithPowerPatternString);
            foreach (Match match in matches)
            {
                var group = match.Groups["name"];
                if (!group.Success)
                    continue;
                string name = group.Value.Trim();
                var arys = name.Split('*');
                if (2 < arys.Length)    //若数据非法
                    continue;
                if (!int.TryParse(arys[0], out int i))
                    continue;
                float power = 1;
                if (2 != arys.Length || !float.TryParse(arys[1], out power))
                    power = 1;
                result.Add(Tuple.Create(i, power));
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

        /// <summary>
        /// 將對象轉化爲Json格式字符串。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToJson<T>(T obj)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings()
            {
                DateTimeFormat = new DateTimeFormat("s"),
            });
            using (var ms = new MemoryStream())
            {
                using (var writer = JsonReaderWriterFactory.CreateJsonWriter(ms, Encoding.UTF8, false, true))
                {
                    ser.WriteObject(writer, obj);
                }
                ms.Position = 0;
                using (StreamReader sr = new StreamReader(ms))
                    return sr.ReadToEnd();
            }

        }

        /// <summary>
        /// 從Json格式字符串獲取對象。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static T FromJson<T>(string jsonString)
        {
            T result;
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings()
            {
                DateTimeFormat = new DateTimeFormat("s"),
            });
            using (var ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms))
                    sw.Write(jsonString);
                var buffer = ms.ToArray();
                using (var reader = JsonReaderWriterFactory.CreateJsonReader(buffer, 0, buffer.Length, Encoding.UTF8, new System.Xml.XmlDictionaryReaderQuotas() { }, null))
                {
                    result = (T)ser.ReadObject(reader);
                }
            }
            return result;
        }
    }

    /// <summary>
    /// <see cref="EntityWithGuid.Load"/>事件的参数。
    /// </summary>
    public class EntityLoadEventArgs : EventArgs
    {
        public EntityLoadEventArgs()
        {

        }

        public EntityLoadEventArgs(DbContext context)
        {
            DbContext = context;
        }

        /// <summary>
        /// 获取或设置数据上下文。实体对象用此上下文调入内存。
        /// </summary>
        public DbContext DbContext { get; set; }
    }

    public class EntitySavingEventArgs : EventArgs
    {
        public EntitySavingEventArgs()
        {

        }

        public EntitySavingEventArgs(DbContext context)
        {
            DbContext = context;
        }

        /// <summary>
        /// 获取或设置数据上下文。实体对象用此上下文调入内存。
        /// </summary>
        public DbContext DbContext { get; set; }
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

        /// <summary>
        /// 引发 EntityLoad 事件。应用代码应该在将数据读入内存形成对象后尽快调用一次该方法以保证数据的完整调入。
        /// </summary>
        public void InvokeOnEntityLoad(EntityLoadEventArgs e)
        {
            OnEntityLoad(e);
        }

        /// <summary>
        /// 引发 EntitySaving 事件。应用代码应该在写入数据库之前调用该方法以保证数据完整写入。
        /// </summary>
        /// <param name="e"></param>
        public void InvokeOnEntitySaving(EntitySavingEventArgs e)
        {
            OnEntitySaving(e);
        }

        /// <summary>
        /// 实体对象刚刚调入内存时发生。
        /// </summary>
        public event EventHandler<EntityLoadEventArgs> EntityLoad;

        /// <summary>
        /// 引发<see cref="EntityLoad"/>事件。
        /// 允许派生类对事件进行处理而不必附加委托。 重写此方法是用于处理在派生类中的事件的首选的技术。
        /// 在派生类中重写 此方法 时，一定要调用基类的 此方法 方法，以便已注册的委托对事件进行接收。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnEntityLoad(EntityLoadEventArgs e)
        {
            EntityLoad?.Invoke(this, e);
        }

        /// <summary>
        /// 实体对象即将保存到数据库时发生。
        /// </summary>
        public event EventHandler<EntitySavingEventArgs> EntitySaving;

        /// <summary>
        /// 引发<see cref="EntitySaving"/>事件。
        /// 允许派生类对事件进行处理而不必附加委托。 重写此方法是用于处理在派生类中的事件的首选的技术。
        /// 在派生类中重写 此方法 时，一定要调用基类的 此方法 方法，以便已注册的委托对事件进行接收。
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnEntitySaving(EntitySavingEventArgs e)
        {
            EntitySaving?.Invoke(this, e);
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
        /// 异步加载一组 <see cref="ThingPropertyItem"/> 对象到指定的 <see cref="ThingEntityBase"/> 对象中。
        /// 這個函數會讀取數據庫用於加載數據。
        /// </summary>
        /// <param name="context">使用的数据上线文。</param>
        /// <param name="collection">要加载子项数据的 <see cref="ThingEntityBase"/> 对象集合。其中 Id属性为全零的对象将被忽略。</param>
        /// <returns></returns>
        public static Task<IEnumerable<ThingPropertyItem>> LoadThingPropertyItemsAsync(DbContext context, IEnumerable<ThingEntityBase> collection)
        {
            return Task.Run(() =>
            {
                IEnumerable<ThingPropertyItem> result;
                List<ThingPropertyItem> propertyItems;
                if (typeof(IDbAsyncEnumerable).IsAssignableFrom(collection.GetType())) //若可能是數據庫查詢
                {
                    result = context.Set<ThingPropertyItem>().Where(c => collection.Any(d => d.Id != Guid.Empty && d.Id == c.ThingEntityId));
                    try
                    {
                        propertyItems = result.ToList();
                    }
                    catch (Exception)   //若無法獲取
                    {
                        var ids = collection.Select(c => c.Id).ToArray();
                        result = context.Set<ThingPropertyItem>().Where(c => ids.Contains(c.ThingEntityId));
                        propertyItems = result.ToList();
                    }
                }
                else
                {
                    var ids = collection.Select(c => c.Id).ToArray();
                    result = context.Set<ThingPropertyItem>().Where(c => ids.Contains(c.ThingEntityId));
                    propertyItems = result.ToList();
                }

                var coll = propertyItems.GroupBy(c => c.ThingEntityId) //按实体Id分组
                    .Join(collection, c => c.Key, c => c.Id, (outer, inner) => new { outer, inner });
                foreach (var item in coll)
                {
                    item.inner.MergeThingPropertyItems(item.outer);
                }
                return propertyItems.Cast<ThingPropertyItem>();
            });
        }

        /// <summary>
        /// 异步加载一组 <see cref="ThingPropertyItem"/> 对象到指定的 <see cref="ThingEntityBase"/> 对象中。
        /// </summary>
        /// <param name="context">使用的数据上线文。</param>
        /// <param name="collection">要加载子项数据的 <see cref="ThingEntityBase"/> 对象集合。其中 Id属性为全零的对象将被忽略。</param>
        /// <returns></returns>
        [CLSCompliant(false)]
        public static Task<IEnumerable<ThingPropertyItem>> LoadThingPropertyItemsAsync(DbContext context, params ThingEntityBase[] collection)
        {
            return LoadThingPropertyItemsAsync(context, collection.Cast<ThingEntityBase>());
        }

        /// <summary>
        /// 將指定的實體對象中擴展屬性對象合并到數據上下文中。
        /// 這個函數不會調用上下文的保存方法。但是這個函數會發生訪問數據庫的操作。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="collection"></param>
        /// <returns>返回保存的數據對象。</returns>
        public static Task<IEnumerable<ThingPropertyItem>> SaveThingPropertyItemsAsync(DbContext context, IEnumerable<ThingEntityBase> collection)
        {
            return Task.Run(() =>
            {
                foreach (var entity in collection)
                {
                    foreach (var item in entity.ThingPropertyItems)
                    {
                        if (Guid.Empty == item.ThingEntityId)
                            item.ThingEntityId = entity.Id;
                        if (Guid.Empty == item.Id)
                            item.Id = Guid.NewGuid();
                    }
                }
                var coll = collection.SelectMany(c => c.ThingPropertyItems).ToArray();
                context.Set<ThingPropertyItem>().AddOrUpdate(coll);
                return coll.Cast<ThingPropertyItem>();
            });
        }

        /// <summary>
        /// 將指定的實體對象中擴展屬性對象合并到數據上下文中。
        /// 這個函數不會調用上下文的保存方法。但是這個函數會發生訪問數據庫的操作。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="collection"></param>
        /// <returns>返回保存的數據對象。</returns>
        [CLSCompliant(false)]
        public static Task<IEnumerable<ThingPropertyItem>> SaveThingPropertyItemsAsync(DbContext context, params ThingEntityBase[] collection)
        {
            return SaveThingPropertyItemsAsync(context, collection);
        }

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

        List<ThingPropertyItem> _ThingPropertyItems;


        /// <summary>
        /// 描述事物某个属性的对象。这个对象记录的信息服务器不加理解，仅供使用接口的程序使用。
        /// </summary>
        [NotMapped]
        [DataMember]
        [Description("描述事物某个属性的对象。这个对象记录的信息服务器不加理解，仅供使用接口的程序使用。")]
        public List<ThingPropertyItem> ThingPropertyItems
        {
            get
            {
                return _ThingPropertyItems ?? (_ThingPropertyItems = new List<ThingPropertyItem>());
            }
            set
            {
                _ThingPropertyItems = value;
            }
        }

        /// <summary>
        /// 使用指定集合数据合并到 <see cref="ThingPropertyItems"/> 属性中，用Id合并，如果不存在则添加，否则替换。
        /// 当前对象 Id 全零时会立即返回，此情况将不会合并任何数据。
        /// </summary>
        /// <param name="collection"><see cref="ThingPropertyItem.ThingEntityId"/>与当前对象Id不同的将本忽略。</param>
        /// <exception cref="ArgumentException"><paramref name="collection"/>有重复Id的对象。</exception>
        public void MergeThingPropertyItems(IEnumerable<ThingPropertyItem> collection)
        {
            if (Guid.Empty == Id)
                return;
            var dic = collection.Where(c => c.ThingEntityId == Id).ToDictionary(c => c.Id);
            var items = ThingPropertyItems;
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var item = items[i];
                if (dic.TryGetValue(item.Id, out var tmp))    //若已经有相同Id的对象
                {
                    items[i] = tmp;
                    dic.Remove(item.Id);
                }
            }
            items.AddRange(dic.Values);
        }

        /// <summary>
        /// 异步获取该实体对象的附属扩展信息。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="bForce">是否强制调入。</param>
        /// <returns></returns>
        public Task LoadThingPropertyItemsAsync(DbContext context, bool bForce = false)
        {
            return context.Set<ThingPropertyItem>().Where(c => c.ThingEntityId == Id).ToListAsync().ContinueWith(c =>
            {
                ThingPropertyItems = c.Result;
            }, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
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
                foreach (var item in ThingPropertyItems)
                {
                    item.GeneratedIdIfEmpty();
                    item.ThingEntityId = Id;
                }
                return Task.Run(() => { context.Set<ThingPropertyItem>().AddOrUpdate(ThingPropertyItems.ToArray()); });
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
        /// 属性名称。只能是字符串。最长64字符。
        /// </summary>
        [DataMember]
        [MaxLength(64)]
        [Description("属性名称。只能是字符串。最长64字符。")]
        [Index]
        public string Name { get; set; }

        /// <summary>
        /// 属性的值。只能是字符串。
        /// </summary>
        [DataMember]
        [Description("属性的值。只能是字符串。")]
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
        public int OrderNumber { get; set; } = 0;
    }

}
#pragma warning restore CS3021 // 由于程序集没有 CLSCompliant 特性，因此类型或成员不需要 CLSCompliant 特性

