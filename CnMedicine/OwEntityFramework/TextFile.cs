
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

        /// <summary>
        /// 设置或获取一个指示，是否忽略读取到文本两边的引号。
        /// </summary>
        public bool IgnoreQuotes { get; set; }

        string _Path;

        public virtual List<T> GetList<T>(string fileName) where T : new()
        {
            string fullPath = Path.Combine(_Path, fileName);
            using (var stream = File.OpenRead(fullPath))
            using (var reader = new StreamReader(stream, Encoding.Default))
                return GetList<T>(reader);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
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
                if (row.ItemArray.All(c => c == DBNull.Value || (c is string) && string.IsNullOrWhiteSpace(c as string)))  //若数据全空
                    continue;
                var tmp = new T();
                foreach (var item in mapping)
                {
                    var val = ConvertEx(IgnoreQuotes ? (row[item.Index] as string)?.Trim('\"') : row[item.Index], item.pi.PropertyType);
                    item.pi.SetValue(tmp, val);
                }
                result.Add(tmp);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="type"></param>
        /// <returns>对空引用立即返回空引用。</returns>
        private object ConvertEx(object obj, Type type)
        {
            object result = null;
            if (type.IsEnum) //若是枚举类型
            {
                var td = TypeDescriptor.GetConverter(type);
                result = td.ConvertFrom(obj);

            }
            else
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Empty:
                        break;
                    case TypeCode.Object:
                        {
                            var mi = type.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy, null, new Type[] { typeof(string) }, null);
                            if (null != mi)
                                result = mi.Invoke(null, new object[] { obj });
                        }
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


}