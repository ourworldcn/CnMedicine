using System;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using OW.Data.Entity;

namespace CnMedicineServer.Models
{
    // 可以通过将更多属性添加到 ApplicationUser 类来为用户添加配置文件数据，请访问 https://go.microsoft.com/fwlink/?LinkID=317594 了解详细信息。
    public class ApplicationUser : IdentityUser
    {

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // 请注意，authenticationType 必须与 CookieAuthenticationOptions.AuthenticationType 中定义的相应项匹配
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // 在此处添加自定义用户声明
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private static string _ConnectionString;

        /// <summary>
        /// 生产环境下数据库连接字符串。
        /// </summary>
        private static string _DefaultConnectionString = @"Data Source=.;Initial Catalog=CnMedicineServer;Integrated Security=True";

        /// <summary>
        /// 开发环境下数据库连接字符串。
        /// </summary>
        private static string _LocalConnectionString = @"Data Source=DESKTOP-AI1JMCB\SQLEXPRESS2016,31433;Initial Catalog=CnMedicineServer;Integrated Security=True";

        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_ConnectionString))
                {
                    //  var conns = System.Web.Configuration.WebConfigurationManager.ConnectionStrings;
                    SqlConnectionStringBuilder scsb;
                    if (Environment.MachineName == "DESKTOP-AI1JMCB") //若在开发环境下
                    {
                        scsb = new SqlConnectionStringBuilder(_LocalConnectionString);
                    }
                    else
                    {
                        scsb = new SqlConnectionStringBuilder(_DefaultConnectionString);
                    }
                    scsb.Pooling = true;    //确保池化，不再额外使用其它的连接优化手段
                    _ConnectionString = scsb.ToString();
                }
                return _ConnectionString;
            }
        }

        static ApplicationDbContext()
        {
            //自动升级数据库架构
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Migrations.Configuration>());
            //初始化

        }

        public ApplicationDbContext()
            : base(ConnectionString, throwIfV1Schema: false)
        {
        }

        public ApplicationDbContext(string nameOrConnectionString) : base(nameOrConnectionString, throwIfV1Schema: false)
        {

        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<SpecialCases> SpecialCases { get; set; }

        public DbSet<SpecialCasesItem> SpecialCasesItems { get; set; }

        public DbSet<ThingPropertyItem> ThingPropertyItem { get; set; }

        public DbSet<SurveysTemplate> SurveysTemplates { get; set; }

        public DbSet<SurveysConclusion> SurveysConclusions { get; set; }
    }
}