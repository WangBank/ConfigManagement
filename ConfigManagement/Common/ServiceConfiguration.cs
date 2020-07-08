using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using ConfigManagement.Models;
using System.Runtime.InteropServices;
using FreeSql;

namespace ConfigManagement.Common
{
    public static class ServiceConfiguration
    {
        public static IServiceCollection ConfigServies(this IServiceCollection services, IConfiguration Configuration)
        {
            string SqlLiteConn = string.Empty;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                SqlLiteConn = Configuration.GetConnectionString("SqlLiteLinux");
            }
            else
            {
                SqlLiteConn = Configuration.GetConnectionString("SqlLiteWin");
            }
            var fsql1 = new FreeSqlBuilder().UseConnectionString(DataType.Sqlite, SqlLiteConn)
            .Build<SqlLiteFlag>();
            services.AddSingleton(fsql1);

            //添加appsettings.json
            services.AddOptions();
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            services.AddSession();
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
            });

            var os = Environment.OSVersion;
            string connectionString = string.Empty;
            if (os.Platform != PlatformID.Unix && os.Platform != PlatformID.MacOSX)
            {
                connectionString = $@"DataSource={Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\DataBase\ConfigManagement.sqlite";
            }
            else
            {
                connectionString = $@"DataSource={Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/DataBase/ConfigManagement.sqlite";
            }

            services.Configure<SqlSugarConfig>(Configuration.GetSection("DataAdapters"));
           
            services.AddSingleton<IDbFactory, DbFactory>();
            //services.AddSingleton(typeof(DbFactory));



            //加上这个返回的json不会自动驼峰
            //services.AddMvc().AddNewtonsoftJson(s => s.SerializerSettings.ContractResolver = new DefaultContractResolver());
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddMvc(options =>
            {
                options.Filters.Add<GlobalExceptionFilter>();
            });

            return services;
        }
    }
}
