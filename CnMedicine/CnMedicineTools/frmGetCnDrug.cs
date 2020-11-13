using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CnMedicineTools
{
    public partial class frmGetCnDrug : Form
    {
        public frmGetCnDrug()
        {
            InitializeComponent();
        }

        private void btOpen_Click(object sender, EventArgs e)
        {
            var result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                tbFileName.Text = openFileDialog1.FileName;
            }
        }

        private void frmGetCnDrug_Load(object sender, EventArgs e)
        {

        }

        private void btGetResult_Click(object sender, EventArgs e)
        {
            if (!File.Exists(tbFileName.Text))
            {
                MessageBox.Show($"指定文件不存在，请重新指定有效文件！文件名{tbFileName.Text}");
                return;
            }
            using (FileStream stream = new FileStream(tbFileName.Text, FileMode.Open, FileAccess.Read))
            {
                var result = GetCnDrug(stream);
                tbCnDrug.Text = string.Join(Environment.NewLine, result);
            }
        }

        /// <summary>
        /// 获取无重复药物列表。
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private HashSet<string> GetCnDrug(Stream stream)
        {
            HashSet<string> result = new HashSet<string>();
            Regex regex = new Regex("");
            using (StreamReader sr = new StreamReader(stream, Encoding.Default, true, 8192, leaveOpen: true))
            {
                while (sr.Peek() >= 0)
                {
                    string tmp = sr.ReadLine();
                    var tmpList = GetTuples(tmp);
                    foreach (var item in tmpList)
                    {
                        result.Add(item.Item1);
                    }
                }
            }
            return result;
        }

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
        /// 捕获模式字符串。如：生地黄-9,玄参9g，天冬15;麦冬(醋熏）15;丹参（后下）9。当归9、党参9茯神15炒酸枣仁15远志6五味子6龙骨（醅)-30
        /// </summary>
        public const string KvPatternString = @"[\p{Po}\s]*(?<name>.*?)[\s]*(?<value>[\+\-]?\d+)[g]?";

    }
}
