namespace CnMedicineServer.Migrations
{
    using CnMedicineServer.Models;
    using OW.Data.Entity;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Data.OleDb;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    internal sealed class Configuration : DbMigrationsConfiguration<CnMedicineServer.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        static void Fill(System.IO.TextReader reader, DataTable dataTable, string fieldSeparator, bool hasHeader = false)
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

        /// <summary>
        /// 读取一个文本文件到内存中。
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="fieldSeparator"></param>
        /// <param name="skipLine"></param>
        /// <returns></returns>
        static List<string[]> GetFromText(System.IO.TextReader reader, string fieldSeparator, int skipLine = 0)
        {
            List<string[]> result = new List<string[]>();
            var separator = new string[] { fieldSeparator };
            for (int i = 0; i < skipLine; i++)
            {
                reader.ReadLine();
            }
            for (string line = reader.ReadLine(); null != line; line = reader.ReadLine())
            {
                var objArray = line.Split(separator, StringSplitOptions.None);
                result.Add(objArray);
            }
            return result;
        }

        protected override void Seed(Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.

            Init(context);
        }

        private void Init(ApplicationDbContext context)
        {
            InitSurveysTemplates(context);
            Init1Async(context);
            InitRhinitisAsync(context);
        }

        private void InitSurveysTemplates(ApplicationDbContext context)
        {
            Dictionary<string, SurveysTemplate> oldValues;
            try
            {
                oldValues = context.SurveysTemplates.ToDictionary(c => c.Name);
            }
            catch (ArgumentException err)   // keySelector 为两个元素产生了重复键。
            {
                oldValues = new Dictionary<string, SurveysTemplate>();
            }
            Dictionary<string, SurveysTemplate> dic = new Dictionary<string, SurveysTemplate>()
            {
                {
                    "鼻炎",
                    new  SurveysTemplate()
                    {
                        Id = Guid.Parse("A7458E0D-2BB9-4F99-9913-0B978F6E0CD2"),
                        Name = "鼻炎",

                    }
                },
                {
                    "失眠",
                    new  SurveysTemplate()
                    {
                        Id = Guid.Parse("6987331B-CB93-4ABA-9B18-E777FBCA2B15"),
                        Name = "失眠",
                       UserState= "支持复诊1",
                    }
                }
            };
            //foreach (var key in dic.Keys)
            //{
            //    if (oldValues.TryGetValue(key, out SurveysTemplate template))
            //        EntityUtil.CopyTo(template, dic[key], $"{nameof(SurveysTemplate.Questions)}");
            //}
            context.SurveysTemplates.AddOrUpdate(dic.Values.ToArray());
            context.SaveChanges();
        }

        /// <summary>
        /// 生成专病数据。
        /// </summary>
        /// <param name="context"></param>
        private void Init1Async(Models.ApplicationDbContext context)
        {
            Task task = null;
            var path = System.Web.HttpContext.Current.Server.MapPath("~/content/问题定义-失眠.txt");
            DataTable dt = new DataTable();
            task = Task.Run(() =>
              {
                  using (var stream = System.IO.File.OpenRead(path))
                  using (var reader = new System.IO.StreamReader(stream, System.Text.Encoding.Default, true))
                      Fill(reader, dt, "\t", true);
              });
            var coll = context.Set<InsomniaCasesItem>();
            //添加专病项
            var sci = coll.Where(c => c.Name == "失眠").Include(c => c.CnDrugCorrection).Include(c => c.CnDrugCorrections).Include(c => c.Conversion11s).Include(c => c.Conversion12s)
                .Include(c => c.CnDrugConversion2s).FirstOrDefault();
            if (null == sci)    //若没有
            {
                sci = coll.Create();
                sci.Name = "失眠";
                sci = coll.Add(sci);
                task?.Wait();
                task = context.SaveChangesAsync();
            }
            //添加调查问卷项
            task?.Wait();
            var st = context.Set<SurveysTemplate>().Include(c=>c.Questions).Where(c => c.Name == "失眠").FirstOrDefault();
            if (null == st)
            {
                st = context.Set<SurveysTemplate>().Create();
                st.Name = "失眠";
                context.Set<SurveysTemplate>().Add(st);
                task?.Wait();
                task = context.SaveChangesAsync();
            }
            //生成调查问卷数据。
            task?.Wait();
            var questionTemplates = st.Questions ?? new List<SurveysQuestionTemplate>();
            var rows = dt.Rows.OfType<DataRow>().Where(c => !c.HasErrors);
            //编号,问题,症候,类型,脏腑评分,证型评分,UserState
            var items = rows.Select(c => new { 编号 = Convert.ToString(c["编号"]), 问题 = c["问题"].ToString(), 症候 = c["症候"].ToString(), 类型 = (QuestionsKind)Convert.ToInt32(c["类型"]), 脏腑评分 = c["脏腑评分"].ToString(), 证型评分 = c["证型评分"].ToString() });
            var groups = items.GroupBy(c => c.问题);
            var addTemplateQuestions = groups.Select(c => c.Key).Except(questionTemplates.Select(c => c.QuestionTitle));
            var addQuestions = groups.Where(c => addTemplateQuestions.Contains(c.Key)).Select(c =>
            {
                var kind = c.First().类型;
                var result = new SurveysQuestionTemplate()
                {
                    OrderNum = int.Parse(c.First().编号),
                    QuestionTitle = c.Key,
                    SurveysTemplateId = st.Id,
                    UserState = $"",
                    Kind = kind,
                };
                if ((kind & QuestionsKind.Choice) != 0)
                    result.Answers = new List<SurveysAnswerTemplate>(c.Select(subc =>
                    {
                        return new SurveysAnswerTemplate()
                        {
                            OrderNum = int.Parse(subc.编号),
                            AnswerTitle = subc.症候,
                            UserState = $"编号{subc.编号}",
                        };
                    }));
                return result;
            }).ToList();
            questionTemplates.AddRange(addQuestions);

            context.SaveChanges();
        }


        /// <summary>
        /// 生成鼻炎专病数据。
        /// </summary>
        /// <param name="context"></param>
        private void InitRhinitisAsync(Models.ApplicationDbContext context)
        {
            Task task = null;
            var path = System.Web.HttpContext.Current.Server.MapPath("~/content/鼻炎/问题定义-鼻炎.txt");
            DataTable dt = new DataTable();
            task = Task.Run(() =>
            {
                using (var stream = System.IO.File.OpenRead(path))
                using (var reader = new System.IO.StreamReader(stream, System.Text.Encoding.Default, true))
                    Fill(reader, dt, "\t", true);
            });
            var coll = context.Set<RhinitisCasesItem>();
            //添加专病项
            //评分表，药物输出表，病机药物输出
            var sci = coll.Where(c => c.Name == "鼻炎").Include(c => c.Conversions).Include(c => c.CnDrugCorrections)
                .Include(c => c.CnDrugConversion2s).FirstOrDefault();
            if (null == sci)    //若没有
            {
                sci = coll.Create();
                sci.Name = "鼻炎";
                sci = coll.Add(sci);
                task?.Wait();
                task = context.SaveChangesAsync();
            }
            //添加调查问卷项
            task?.Wait();
            var st = context.Set<SurveysTemplate>().Where(c => c.Name == "鼻炎").FirstOrDefault();
            if (null == st)
            {
                st = context.Set<SurveysTemplate>().Create();
                st.Name = "鼻炎";
                context.Set<SurveysTemplate>().Add(st);
                task?.Wait();
                task = context.SaveChangesAsync();
            }
            //生成调查问卷数据。
            task?.Wait();
            var questionTemplates = st.Questions;
            var rows = dt.Rows.OfType<DataRow>().Where(c => !c.HasErrors);
            //编号,问题,症候,类型,脏腑评分,证型评分,UserState
            var items = rows.Select(c => new { 编号 = Convert.ToString(c["编号"]), 问题 = c["问题"].ToString(), 症候 = c["症候"].ToString(), 类型 = (QuestionsKind)Convert.ToInt32(c["类型"]), 脏腑评分 = c["脏腑评分"].ToString(), 证型评分 = c["证型评分"].ToString() });
            var groups = items.GroupBy(c => c.问题);
            var addTemplateQuestions = groups.Select(c => c.Key).Except(questionTemplates.Select(c => c.QuestionTitle));
            var addQuestions = groups.Where(c => addTemplateQuestions.Contains(c.Key)).Select(c =>
            {
                var kind = c.First().类型;
                var result = new SurveysQuestionTemplate()
                {
                    OrderNum = int.Parse(c.First().编号),
                    QuestionTitle = c.Key,
                    SurveysTemplateId = st.Id,
                    UserState = $"",
                    Kind = kind,
                };
                if ((kind & QuestionsKind.Choice) != 0)
                    result.Answers = new List<SurveysAnswerTemplate>(c.Select(subc =>
                    {
                        return new SurveysAnswerTemplate()
                        {
                            OrderNum = int.Parse(subc.编号),
                            AnswerTitle = subc.症候,
                            UserState = $"编号{subc.编号}",
                        };
                    }));
                return result;
            });
            questionTemplates.AddRange(addQuestions);

            context.SaveChanges();
        }
    }
}
