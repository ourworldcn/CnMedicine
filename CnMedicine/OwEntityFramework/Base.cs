using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace System
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OnPoolEnteringAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        //readonly string positionalString;

        // This is a positional argument
        public OnPoolEnteringAttribute(/*string positionalString*/)
        {
            //this.positionalString = positionalString;

            // TODO: Implement code here

            //throw new NotImplementedException();
        }

        //public string PositionalString
        //{
        //    get { return positionalString; }
        //}

        // This is a named argument
        //public int NamedInt { get; set; }
    }

    /// <summary>
    /// 批注在静态方法上，签名类似以下例子：
    /// <c>
    /// private static ObjectPoolSettingItem SetValuesOnSerialized()
    /// </c>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OnPoolRegisterAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        //readonly string positionalString;

        // This is a positional argument
        public OnPoolRegisterAttribute(/*string positionalString*/)
        {
            //this.positionalString = positionalString;

            // TODO: Implement code here

            //throw new NotImplementedException();
        }

        //public string PositionalString
        //{
        //    get { return positionalString; }
        //}

        // This is a named argument
        //public int NamedInt { get; set; }
    }

    public class ObjectPoolSettingItem : ICloneable
    {
        public ObjectPoolSettingItem()
        {

        }

        public Type PoolType { get; set; }

        public Func<object> Creator { get; set; }

        public Func<object, bool> EnterPool { get; set; }

        public Func<object, bool> LeavePool { get; set; }

        public int MinCount { get; set; } = Environment.ProcessorCount;

        public int MaxCount { get; set; } = Environment.ProcessorCount * 4;

        public object Clone()
        {
            return new ObjectPoolSettingItem()
            {
                Creator = (Func<object>)Creator.Clone(),
                EnterPool = (Func<object, bool>)EnterPool.Clone(),
                LeavePool = (Func<object, bool>)LeavePool.Clone(),
                MaxCount = MaxCount,
                MinCount = MinCount,
                PoolType = PoolType,
            };
        }

        ConcurrentStack<object> _Pool;
        internal ConcurrentStack<object> Pool => _Pool ?? (_Pool = new ConcurrentStack<object>());
    }

    /// <summary>
    /// 多种类型公用的池对象。所有成员均是线程安全的。
    /// 需要某个类的多个实例并且创建或销毁该类的成本很高的情况下，对象池可以改进应用程序性能。
    /// 某种新类型要使用此对象，需要调用 Register ，注册类型。
    /// 对象已经为以下类注册了：<see cref="MemoryStream"/>,<see cref="StringBuilderCache"/>
    /// </summary>
    /// <remarks>
    /// 共有前提是：对象不限于特定创建它的线程使用，如控件往往不能使用。
    /// 对象通过恢复器调用可以被多个目的使用,注意恢复器代价，
    /// 如 System.Collections.ArrayList 不太适合池化， 其 Clear() 的时间复杂度是 O(n),n是元素数量，此恢复器代价过大。
    /// 在以下情况之一应使用池。
    /// 对象创建代价较大，一般要大于数百cpu周期。
    /// 对象较为频繁创建，容易引起回收压力，此时要考虑付出的cpu周期，使用池化将付出约0.4us(2.8G cpu)的时间代价(约1000cpu周期)。245ms/百万次
    /// </remarks>
    public class ObjectPool
    {
        #region 静态成员及其附属

        static ObjectPool()
        {
            var setting = new ObjectPoolSettingItem()
            {
                PoolType = typeof(MemoryStream),
                Creator = () => new MemoryStream(),
                EnterPool = c =>
                {
                    ((MemoryStream)c).SetLength(0);
                    return true;
                }
            };
            _StaticRegisters.TryAdd(setting.PoolType, setting);
            setting = new ObjectPoolSettingItem()
            {
                PoolType = typeof(StringBuilder),
                Creator = () => new StringBuilder(),
                EnterPool = c =>
                {
                    ((StringBuilder)c).Clear();
                    return true;
                }
            };
            _StaticRegisters.TryAdd(setting.PoolType, setting);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public T TakeObject<T>() where T : class
        {
            return (T)Default.TakeObject(typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static public bool ReturnObject<T>(T obj) where T : class
        {
            return Default.ReturnObject((object)obj);
        }

        static public readonly ObjectPool Default = new ObjectPool();

        /// <summary>
        /// 记录静态注册的类型。
        /// </summary>
        static ConcurrentDictionary<Type, ObjectPoolSettingItem> _StaticRegisters = new ConcurrentDictionary<Type, ObjectPoolSettingItem>();

        static public ObjectPoolSettingItem GetDefaultSetting(Type type)
        {
            ObjectPoolSettingItem setting;
            var mi = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).SingleOrDefault(c => typeof(ObjectPoolSettingItem) == c.ReturnType && null != Attribute.GetCustomAttribute(c, typeof(OnPoolRegisterAttribute)));
            if (null != mi)
                setting = (ObjectPoolSettingItem)mi.Invoke(null, Array.Empty<object>());
            else
            {
                setting = new ObjectPoolSettingItem()
                {
                    PoolType = type,
                    Creator = () => TypeDescriptor.CreateInstance(null, type, null, null),
                };
                mi = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic).SingleOrDefault(c => typeof(bool) == c.ReturnType && null != Attribute.GetCustomAttribute(c, typeof(OnPoolEnteringAttribute)));
                if (null != mi)
                    setting.EnterPool = c => (bool)mi.Invoke(c, Array.Empty<object>());
            }
            return setting;
        }

        /// <summary>
        /// 提前静态注册的类型。
        /// </summary>
        /// <param name="settingItem">注册之后再修改此对象引用不会导致行为变化，此函数使用深表复制记录一个副本。</param>
        /// <returns>成功注册则返回true,已经注册则返回false。</returns>
        static public bool Register(ObjectPoolSettingItem settingItem)
        {
            var result = _StaticRegisters.TryAdd(settingItem.PoolType, (ObjectPoolSettingItem)settingItem.Clone());
            return result;
        }

        #region 运算符重载


        #endregion

        #endregion 静态成员及其附属

        #region 构造函数

        public ObjectPool()
        {
        }

        #endregion 构造函数

        #region 实例属性及其存储字段

        /// <summary>
        /// 默认键值。不需要使用键区分实例的可以使用该键。
        /// </summary>
        public const string DefaultKey = "";

        #endregion 实例属性及其存储字段

        #region 实例方法及其专用附属

        public object TakeObject(Type type, string key = null)
        {
            object result;
            var setting = _StaticRegisters.GetOrAdd(type, c => GetDefaultSetting(c));
            while (setting.Pool.TryPop(out result))
            {
                try
                {
                    if (setting.LeavePool?.Invoke(result) ?? true)
                        return result;
                }
                catch (Exception)
                {
                    Trace.WriteLine($"");
                }
            }
            return setting.Creator();
        }

        public bool ReturnObject(object obj)
        {
            var setting = _StaticRegisters.GetOrAdd(obj.GetType(), c => GetDefaultSetting(c));
            try
            {
                if (setting.Pool.Count >= setting.MaxCount)
                    return false;
                if (!(setting.EnterPool?.Invoke(obj) ?? true))
                    return false;
                setting.Pool.Push(obj); //略微超过一些数量无所谓
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        LinkedList<byte[]> _Buffer = new LinkedList<byte[]>();
        protected object ThisLocker => _Buffer;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bufferSize"></param>
        /// <param name="clearBuffer">是否获取buffer时，自动用0填充。默认不填充。</param>
        /// <returns></returns>
        public byte[] TakeBuffer(int bufferSize, bool clearBuffer = false)
        {
            LinkedListNode<byte[]> node;
            lock (ThisLocker)
            {
                for (node = _Buffer.First; null != node; node = node.Next)
                {
                    if (node.Value.Length >= bufferSize)
                        break;
                }
                if (null != node)
                {
                    _Buffer.Remove(node);
                }
            }
            if (null != node && clearBuffer)
                Array.Clear(node.Value, 0, node.Value.Length);
            return node?.Value ?? new byte[bufferSize];
        }

        public void ReturnBuffer(byte[] buffer)
        {
            if (null == buffer)
                throw new ArgumentNullException(nameof(buffer));
            LinkedListNode<byte[]> node;
            lock (ThisLocker)
            {
                for (node = _Buffer.First; null != node; node = node.Next)
                {
                    if (node.Value.Length >= buffer.Length)
                        break;
                }
                if (null == node)
                    _Buffer.AddLast(buffer);
                else
                    _Buffer.AddBefore(node, buffer);
                if (_Buffer.Count > 16)
                    _Buffer.RemoveFirst();
            }
        }

        #endregion 实例方法及其专用附属

        #region 重载及实现抽象成员及其附属



        #endregion 重载及实现抽象成员及其附属

        #region 事件及其相关成员



        #endregion 事件及其相关成员
    }

    public static class ClearHelperExtends
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder TakeStringBuilderAndPushReturn(this ClearHelper ch)
        {
            return ch.TakeObjectAndReturn<StringBuilder>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static MemoryStream TakeMemoryStreamAndReturn(this ClearHelper ch)
        {
            return ch.TakeObjectAndReturn<MemoryStream>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T TakeObjectAndReturn<T>(this ClearHelper ch) where T : class
        {
            var result = ObjectPool.TakeObject<T>();
            if (null != result)
                ch.Push(c => ObjectPool.ReturnObject<object>(c), result);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] TakeBufferAndReturn(this ClearHelper ch, int bufferSize)
        {
            var result = ObjectPool.Default.TakeBuffer(bufferSize);
            if (null != result)
                ch.Push(c => ObjectPool.Default.ReturnBuffer((byte[])c), result);
            return result;
        }
    }

    public class ClearHelper : IDisposable
    {
        [OnPoolRegister]
        static ObjectPoolSettingItem OnPoolRegister()
        {
            var result = new ObjectPoolSettingItem()
            {
                PoolType = typeof(ClearHelper),
                Creator = () => new ClearHelper(),
                EnterPool = c =>
                {
                    ClearHelper clearHelper = (ClearHelper)c;
                    clearHelper._Actions.Clear();
                    clearHelper.disposedValue = false;
                    return true;
                },
            };
            return result;
        }

        public ClearHelper()
        {

        }

        Stack<ValueTuple<Action<object>, object>> _Actions = new Stack<(Action<object>, object)>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(Action<object> action, object state = null)
        {
            _Actions.Push(ValueTuple.Create(action, state));
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
                    while (_Actions.Count > 0)
                    {
                        var action = _Actions.Pop();
                        try
                        {
                            action.Item1(action.Item2);
                        }
                        catch (Exception)
                        {

                        }
                    }
                    if (!Environment.HasShutdownStarted && ObjectPool.ReturnObject(this))
                    {
                        return;
                    }
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~ClearHelper() {
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

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class BackgroundCallAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        readonly string positionalString;

        // This is a positional argument
        public BackgroundCallAttribute(string positionalString)
        {
            this.positionalString = positionalString;

            // TODO: Implement code here

            throw new NotImplementedException();
        }

        public string PositionalString
        {
            get { return positionalString; }
        }

        // This is a named argument
        public int NamedInt { get; set; }
    }
    public class OwInitializer
    {
        static volatile bool _IsInited;
        static OwInitializer()
        {
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
        }

        static readonly ConcurrentDictionary<string, Assembly> dic = new ConcurrentDictionary<string, Assembly>();

        private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            if (dic.TryAdd(args.LoadedAssembly.FullName, args.LoadedAssembly) && _IsInited)
                Task.Factory.StartNew(c => EnumTypes(c as IEnumerable<Assembly>), new Assembly[] { args.LoadedAssembly });
        }

        private static void EnumTypes(IEnumerable<Assembly> assemblies)
        {
            foreach (var assm in assemblies)
            {
                Debug.WriteLine($"{Interlocked.Increment(ref count):d2}:扫描 {assm.FullName}");
                foreach (var type in assm.DefinedTypes.Where(c => c.IsClass || c.IsValueType))
                {
                    var mis = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                         .Where(c => c.GetParameters().Length == 0 && null != c.GetCustomAttribute<BackgroundCallAttribute>());
                    foreach (var item in mis)
                        methodInfos.Add(item);
                }
            }
        }
        static int count = 0;
        static readonly ConcurrentBag<MethodInfo> methodInfos = new ConcurrentBag<MethodInfo>();
        public static Task BeginInitialize()
        {
            var coll = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var item in coll)
            {
                dic.TryAdd(item.FullName, item);
            }

            return Task.Factory.StartNew(c =>
            {
                var ary = ((IEnumerable<Assembly>)c).ToArray();
                EnumTypes(ary);
                foreach (var item in ary)
                {
                    dic.TryRemove(item.FullName, out Assembly assm);
                }
                _IsInited = true;
            }, dic.Values);
        }
    }
}