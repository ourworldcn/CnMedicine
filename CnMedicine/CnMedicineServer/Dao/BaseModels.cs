using System;
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
    /// <summary>
    /// 指定属性，字段从文本文件映射。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = true)]
    public sealed class TextFieldNameAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        private readonly string _FieldName;

        // This is a positional argument
        public TextFieldNameAttribute(string fieldName)
        {
            _FieldName = fieldName;

            // TODO: Implement code here

        }

        public string FieldName
        {
            get { return _FieldName; }
        }

        // This is a named argument
        //public int NamedInt { get; set; }
    }

    /// <summary>
    /// 管理文本文件的上下文。
    /// </summary>
    public class TextFileContext : IDisposable
    {
        public static void Fill(System.IO.TextReader reader, DataTable dataTable, string fieldSeparator, bool hasHeader = false)
        {
            Debug.Assert(hasHeader);    //暂时不管无标头
            var separator = new string[] { fieldSeparator };
            byte[] bof = new byte[4];
            //var b = stream.Read(bof,0,4);
            string[] header;
            string headerString = reader.ReadLine();
            header = headerString.Split(separator, StringSplitOptions.None);
            foreach (var item in header)
            {
                dataTable.Columns.Add(item, typeof(string));

            }
            for (string line = reader.ReadLine(); null != line; line = reader.ReadLine())
            {
                var objArray = line.Split(separator, StringSplitOptions.None);
                dataTable.Rows.Add(objArray);
            }
        }

        public TextFileContext()
        {

        }

        public TextFileContext(string path)
        {
            _Path = path;
        }

        string _Path;

        public virtual List<T> GetList<T>(string fileName) where T : new()
        {
            string fullPath = Path.Combine(_Path, fileName);
            using (var stream = File.OpenRead(fullPath))
            using (var reader = new StreamReader(stream, Encoding.Default))
                return GetList<T>(reader);
        }

        public virtual List<T> GetList<T>(TextReader reader) where T : new()
        {
            List<T> result = new List<T>();
            DataTable dt = new DataTable();
            Task task = Task.Run(() => Fill(reader, dt, "\t", true));
            var pis = TypeDescriptor.GetProperties(typeof(T)).OfType<PropertyDescriptor>();
            task.Wait();
            var mapping = pis.Select(c =>
                {
                    var name = (c.Attributes[typeof(TextFieldNameAttribute)] as TextFieldNameAttribute)?.FieldName;
                    if (string.IsNullOrWhiteSpace(name))
                        name = c.Name;
                    var column = dt.Columns[name];
                    if (null == column)
                        return null;
                    int index = column.Ordinal;

                    return new
                    {
                        pi = c,
                        Index = index,
                    };
                }).Where(c => null != c).ToArray();

            foreach (DataRow row in dt.Rows)
            {
                var tmp = new T();
                foreach (var item in mapping)
                {
                    var val = ConvertEx(row[item.Index], item.pi.PropertyType);
                    item.pi.SetValue(tmp, val);
                }
                result.Add(tmp);
            }
            return result;
        }

        private object ConvertEx(object obj, Type type)
        {
            object result = null;
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Empty:
                    break;
                case TypeCode.Object:
                    var mi = type.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy, null, new Type[] { typeof(string) }, null);
                    if (null != mi)
                        result = mi.Invoke(null, new object[] { obj });
                    break;
                case TypeCode.DBNull:
                    break;
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.DateTime:
                case TypeCode.String:
                    result = Convert.ChangeType(obj, type);
                    break;
                default:
                    break;
            }
            return result;
        }



        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~TextFileContext() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion


    }

    public class EntityUtil
    {
        public const string KvPatternString = @"[\p{P}\s]*(?<name>.*?)[\s]*(?<value>[\+\-]?\d+)";

        public const string ListPatternString = @"[\p{P}\s]*(?<name>[^\p{P}\s]*)[\s]*";

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
        /// 
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
        /// 如果Id是空则生成新Id。
        /// </summary>
        /// <returns>true生成了新Id,false没有生成新Id。</returns>
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
        [DataMember(IsRequired = false)]
        public Guid ThingEntityId { get; set; }

        /// <summary>
        /// 排序号。同一个实体的多个扩展属性按此字段升序排序。序号不必连续，相等序号顺序随机。
        /// </summary>
        [DataMember]
        public int OrderNum { get; set; } = 0;
    }

}

