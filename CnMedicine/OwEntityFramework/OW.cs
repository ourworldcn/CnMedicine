
using System;

namespace OW
{
    /// <summary>
    /// 为类及其相关成员指定追加的批注。应用根据自己的需要追加批注，而不必定义新的批注类。
    /// 为使Name不会意外重复，可以考虑使用生成的Guid。
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    public sealed class OwAdditionalAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        readonly string _Name;
        readonly string _Value;

        public OwAdditionalAttribute(string name)
        {
            _Name = name;
        }

        // This is a positional argument
        public OwAdditionalAttribute(string name, string val)
        {
            _Name = name;
            _Value = val;

        }

        public string Name
        {
            get { return _Name; }
        }

        public string Value => _Value;

        //// This is a named argument
        //public int NamedInt { get; set; }

    }


}