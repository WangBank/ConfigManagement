using System;
using System.Data;
using ConfigManagement.Common;
using ConfigManagement.Models;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace ConfigManagement.Controllers
{
    public class TestController : Controller
    {
        private readonly SqlSugarClient _dbContext;
        public TestController(IDbFactory factory)
        {
            _dbContext = factory.GetDbContext("sqlite");
        }

        /// <summary>
        /// 测试用 创建数据库和表
        /// </summary>
        /// <returns></returns>
        [Route("test")]
        public string Test()
        {
            //数据库不存在则创建数据库
            _dbContext.DbMaintenance.CreateDatabase();
            //建表
            _dbContext.CodeFirst.InitTables(typeof(DataBaseInfo));
            //_dbContext.CodeFirst.InitTables(typeof(ServerInfo));
            return "";
        }

        [Route("TessT")]
        public string TessT()
        {
            try
            {
                DataTable dataTable = _dbContext.Ado.GetDataTable("select * from gl_hy");
                Console.WriteLine("");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            return "";
        }
    }
}