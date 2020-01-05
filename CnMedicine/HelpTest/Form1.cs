using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.OleDb;
using System.Drawing;
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
            string pattern = @"[\p{P}\s]*(?<name>[^\p{P}\s]*)[\s]*";
            var matches = Regex.Matches(textBox1.Text, pattern);
            var coll = InsomniaMethod.GetTuples(textBox1.Text);
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

        private void Form1_Load(object sender, EventArgs e)
        {
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

    public class InsomniaMethod
    {
        public const string KvPatternString = @"[\p{P}\s]*(?<name>.*?)[\s]*(?<value>[\+\-]?\d+)";

        public const string ListPatternString = @"[\p{P}\s]*(?<name>[^\p{P}\s]*)[\s]*";

        public static List<Tuple<string, float>> GetTuples(string guts)
        {
            List<Tuple<string, float>> result = new List<Tuple<string, float>>();
            var matches = Regex.Matches(guts, KvPatternString);

            foreach (Match match in matches)
            {
                var group = match.Groups["name"];
                if (!group.Success)
                    continue;
                string name = group.Value;
                group = match.Groups["value"];
                if (!group.Success || !float.TryParse(group.Value, out float tmp))
                    continue;
                result.Add(Tuple.Create(name, tmp));
            }
            return result;
        }

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
    }

}
