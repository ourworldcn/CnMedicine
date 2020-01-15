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
            Debug.Assert(hasHeader);    //��ʱ�����ޱ�ͷ
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
        /// ��ȡһ���ı��ļ����ڴ��С�
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
            catch (ArgumentException err)   // keySelector Ϊ����Ԫ�ز������ظ�����
            {
                oldValues = new Dictionary<string, SurveysTemplate>();
            }
            Dictionary<string, SurveysTemplate> dic = new Dictionary<string, SurveysTemplate>()
            {
                {
                    "����",
                    new  SurveysTemplate()
                    {
                        Id = Guid.Parse("A7458E0D-2BB9-4F99-9913-0B978F6E0CD2"),
                        Name = "����",

                    }
                },
                {
                    "ʧ��",
                    new  SurveysTemplate()
                    {
                        Id = Guid.Parse("6987331B-CB93-4ABA-9B18-E777FBCA2B15"),
                        Name = "ʧ��",
                       UserState= "֧�ָ���1",
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
        /// ����ר�����ݡ�
        /// </summary>
        /// <param name="context"></param>
        private void Init1Async(Models.ApplicationDbContext context)
        {
            Task task = null;
            var path = System.Web.HttpContext.Current.Server.MapPath("~/content/���ⶨ��-ʧ��.txt");
            DataTable dt = new DataTable();
            task = Task.Run(() =>
              {
                  using (var stream = System.IO.File.OpenRead(path))
                  using (var reader = new System.IO.StreamReader(stream, System.Text.Encoding.Default, true))
                      Fill(reader, dt, "\t", true);
              });
            var coll = context.Set<InsomniaCasesItem>();
            //���ר����
            var sci = coll.Where(c => c.Name == "ʧ��").Include(c => c.CnDrugCorrection).Include(c => c.CnDrugCorrections).Include(c => c.Conversion11s).Include(c => c.Conversion12s)
                .Include(c => c.CnDrugConversion2s).FirstOrDefault();
            if (null == sci)    //��û��
            {
                sci = coll.Create();
                sci.Name = "ʧ��";
                sci = coll.Add(sci);
                task?.Wait();
                task = context.SaveChangesAsync();
            }
            //��ӵ����ʾ���
            task?.Wait();
            var st = context.Set<SurveysTemplate>().Include(c=>c.Questions).Where(c => c.Name == "ʧ��").FirstOrDefault();
            if (null == st)
            {
                st = context.Set<SurveysTemplate>().Create();
                st.Name = "ʧ��";
                context.Set<SurveysTemplate>().Add(st);
                task?.Wait();
                task = context.SaveChangesAsync();
            }
            //���ɵ����ʾ����ݡ�
            task?.Wait();
            var questionTemplates = st.Questions ?? new List<SurveysQuestionTemplate>();
            var rows = dt.Rows.OfType<DataRow>().Where(c => !c.HasErrors);
            //���,����,֢��,����,�อ����,֤������,UserState
            var items = rows.Select(c => new { ��� = Convert.ToString(c["���"]), ���� = c["����"].ToString(), ֢�� = c["֢��"].ToString(), ���� = (QuestionsKind)Convert.ToInt32(c["����"]), �อ���� = c["�อ����"].ToString(), ֤������ = c["֤������"].ToString() });
            var groups = items.GroupBy(c => c.����);
            var addTemplateQuestions = groups.Select(c => c.Key).Except(questionTemplates.Select(c => c.QuestionTitle));
            var addQuestions = groups.Where(c => addTemplateQuestions.Contains(c.Key)).Select(c =>
            {
                var kind = c.First().����;
                var result = new SurveysQuestionTemplate()
                {
                    OrderNum = int.Parse(c.First().���),
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
                            OrderNum = int.Parse(subc.���),
                            AnswerTitle = subc.֢��,
                            UserState = $"���{subc.���}",
                        };
                    }));
                return result;
            }).ToList();
            questionTemplates.AddRange(addQuestions);

            context.SaveChanges();
        }


        /// <summary>
        /// ���ɱ���ר�����ݡ�
        /// </summary>
        /// <param name="context"></param>
        private void InitRhinitisAsync(Models.ApplicationDbContext context)
        {
            Task task = null;
            var path = System.Web.HttpContext.Current.Server.MapPath("~/content/����/���ⶨ��-����.txt");
            DataTable dt = new DataTable();
            task = Task.Run(() =>
            {
                using (var stream = System.IO.File.OpenRead(path))
                using (var reader = new System.IO.StreamReader(stream, System.Text.Encoding.Default, true))
                    Fill(reader, dt, "\t", true);
            });
            var coll = context.Set<RhinitisCasesItem>();
            //���ר����
            //���ֱ�ҩ�����������ҩ�����
            var sci = coll.Where(c => c.Name == "����").Include(c => c.Conversions).Include(c => c.CnDrugCorrections)
                .Include(c => c.CnDrugConversion2s).FirstOrDefault();
            if (null == sci)    //��û��
            {
                sci = coll.Create();
                sci.Name = "����";
                sci = coll.Add(sci);
                task?.Wait();
                task = context.SaveChangesAsync();
            }
            //��ӵ����ʾ���
            task?.Wait();
            var st = context.Set<SurveysTemplate>().Where(c => c.Name == "����").FirstOrDefault();
            if (null == st)
            {
                st = context.Set<SurveysTemplate>().Create();
                st.Name = "����";
                context.Set<SurveysTemplate>().Add(st);
                task?.Wait();
                task = context.SaveChangesAsync();
            }
            //���ɵ����ʾ����ݡ�
            task?.Wait();
            var questionTemplates = st.Questions;
            var rows = dt.Rows.OfType<DataRow>().Where(c => !c.HasErrors);
            //���,����,֢��,����,�อ����,֤������,UserState
            var items = rows.Select(c => new { ��� = Convert.ToString(c["���"]), ���� = c["����"].ToString(), ֢�� = c["֢��"].ToString(), ���� = (QuestionsKind)Convert.ToInt32(c["����"]), �อ���� = c["�อ����"].ToString(), ֤������ = c["֤������"].ToString() });
            var groups = items.GroupBy(c => c.����);
            var addTemplateQuestions = groups.Select(c => c.Key).Except(questionTemplates.Select(c => c.QuestionTitle));
            var addQuestions = groups.Where(c => addTemplateQuestions.Contains(c.Key)).Select(c =>
            {
                var kind = c.First().����;
                var result = new SurveysQuestionTemplate()
                {
                    OrderNum = int.Parse(c.First().���),
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
                            OrderNum = int.Parse(subc.���),
                            AnswerTitle = subc.֢��,
                            UserState = $"���{subc.���}",
                        };
                    }));
                return result;
            });
            questionTemplates.AddRange(addQuestions);

            context.SaveChanges();
        }
    }
}
