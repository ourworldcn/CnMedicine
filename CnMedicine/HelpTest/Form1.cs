using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HelpTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string pattern = @"[\p{Po}\s]*(?<name>[^\p{Po}\s]*)[\s]*";
            //var matches = Regex.Matches(textBox1.Text, pattern);
            var coll = EntityUtil.GetTuples(textBox1.Text);
            SuspendLayout();
            try
            {
                listView1.Items.Clear();
                foreach (var tuple in coll)
                {
                    listView1.Items.Add($"{tuple.Item1}:{tuple.Item2.ToString()}");
                }
            }
            finally
            {
                ResumeLayout();
            }
        }

        [Flags, Serializable]
        enum MyEnum
        {
            多重 = 1,
            选择 = 512,
        }

        void PreTest()
        {
        }

        void Test()
        {
            var _Seq1 = Enumerable.Range(0, 1000);
            var _Seq2 = Enumerable.Range(0, 1000);
            var t = _Seq1.Join(_Seq2, c => c, c => c, (c1, c2) => c1).ToArray();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            PreTest();
            var st = Stopwatch.StartNew();
            Test();
            st.Stop();
            MessageBox.Show(st.ElapsedMilliseconds.ToString());
            var td1 = TypeDescriptor.GetConverter(typeof(int));
            var ii = td1.ConvertFrom("55");
           var i1= Convert.ChangeType("32", typeof(int));
            string patt = @"[\s\p{Po}-[\*]]*?(?<name>[\d\s\*\.]+)";
            var ms = Regex.Matches("3104,1101,1202*3,1301,1502,5202,4107*2,4415,1703。", patt);
            foreach (var item in ms.OfType<Match>())
            {
                Debug.WriteLine(item.Groups["name"]);
            }
            var f1 = decimal.Parse(".3");
            //var xx = (MyEnum)Convert.ChangeType("选择 , 多重", typeof(MyEnum));
            var td = TypeDescriptor.GetConverter(typeof(MyEnum));
            var xx = (MyEnum)td.ConvertFrom("选择 , 多重");
            var en = (MyEnum)Enum.Parse(typeof(MyEnum), "选择 , 多重", true);
            var cat1 = char.GetUnicodeCategory('—');
            var cat2 = char.GetUnicodeCategory('.');
            var cat3 = char.GetUnicodeCategory('＄');
            var str = "2";
            var i = Convert.ChangeType(str, typeof(int));
            //var mi = typeof(Guid).GetMethod("Parse", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy, null, new Type[] { typeof(string) }, null);
            //object id;
            //if (null != mi)
            //    id = mi.Invoke(null, new object[] { str });

            int[] ary1 = new int[] { 1, 2 };
            int[] ary2 = Enumerable.Range(3, 2).ToArray();
            var coll = from tmp1 in ary1
                       from tmp2 in ary2
                       select new { tmp1, tmp2 };
            var ary = coll.ToArray();
        }
    }

    public class EntityUtil
    {
        public const string KvPatternString = @"[\p{Po}\s]*(?<name>.*?)[\s]*(?<value>[\+\-]?\d+)";

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
}
