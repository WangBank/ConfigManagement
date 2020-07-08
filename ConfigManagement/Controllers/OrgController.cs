using System;
using System.Collections.Generic;

using System.Linq;
using System.Threading.Tasks;
using ConfigManagement.Common;
using ConfigManagement.Models;
using FreeSql;
using Microsoft.AspNetCore.Mvc;

using SqlSugar;

namespace ConfigManagement.Controllers
{
    public class OrgController : Controller
    {
        public IFreeSql _sqliteSql;
        private readonly SqlSugarClient _dbContext;


        public OrgController(IFreeSql<SqlLiteFlag> sqliteSql, IDbFactory factory)
        {
            _dbContext = factory.GetDbContext("sqlite");
            _sqliteSql = sqliteSql;
        }

        [Route("orgIndex")]
        public IActionResult Index()
        {
           
            ViewBag.UserInfo = HttpContext.Session.GetData<UserInfo>("user");
            return View(); ;
        }

        [Route("/AddOrgInfo")]
        public IActionResult AddOrgInfo()
        {
            ViewBag.IsOrg = "1";
            ViewBag.BusinessCategory = _dbContext.Queryable<ConfigInfo>().ToList();
            return View();
        }


        [Route("/Api/GetOrgList")]
        public async Task<LayuiData> GetOrgList(string code = "", string name = "")
        {
            var items = string.IsNullOrEmpty(code) && string.IsNullOrEmpty(name)
               ? await _sqliteSql.Select<Organization>().ToListAsync()
               : await _sqliteSql.Select<Organization>().Where(o => o.Name.Contains(name) || o.Guid == int.Parse(code)
           ).ToListAsync();

            return LayuiData.CreateResult(items.Count, items);
        }


        [Route("/UpdateOrgInfo")]
        public async Task<IActionResult> UpdateOrgInfo(int guid)
        {
            ViewBag.IsOrg = "1";
            var orgInfo = await _sqliteSql.Select<Organization>().Where(o => o.Guid == guid).FirstAsync();
            return View(orgInfo);
        }


        [HttpPost]
        [Route("/Api/AddOrgInfo")]
        public async Task<ResultData> AddOrgInfo([FromBody] Organization organization)
        {
            var databaseInfo = organization.GetConnString(organization);
            var insetresult = await _sqliteSql.Insert(new Organization
            {
                ServerName = organization.ServerName,
                ConnectingString = databaseInfo.connString,
                DataBaseName = organization.DataBaseName,
                DbType = organization.DbType,
                Description = organization.Description,
                Name = organization.Name,
                Password = organization.Password,
                UserName = organization.UserName

            }).ExecuteAffrowsAsync();
            if (insetresult > 0)
            {
                return ResultData.CreateSuccessResult();
            }
            else
            {
                return ResultData.CreateResult("-1", "添加失败", null);
            }

        }

        [HttpPost]
        [Route("/Api/UpdateOrgInfo")]
        public async Task<ResultData> UpdateOrgInfo([FromBody] Organization organization)
        {
            var databaseInfo = organization.GetConnString(organization);
            organization.ConnectingString = databaseInfo.connString;
            var insetresult = await _sqliteSql.Update<Organization>().SetSource(organization).ExecuteAffrowsAsync();

            if (insetresult > 0)
            {
                return ResultData.CreateSuccessResult();
            }
            else
            {
                return ResultData.CreateResult("-1", "更新失败", null);
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/Api/DeleteOrgInfo")]
        public async Task<ResultData> DeleteOrgInfo([FromBody] List<Organization> organizations)
        {
            if (organizations == null || organizations.Count == 0)
            {
                return ResultData.CreateResult("-1", "删除失败", null);
            }

            int result = 0;
           
            var orgCodes = new int[organizations.Count];
            for (int i = 0; i < organizations.Count; i++)
            {
                orgCodes[i] = organizations[i].Guid;
            }
            result = await _sqliteSql.Delete<TasksDetail>().Where(o => orgCodes.Contains(o.TaskCode)).ExecuteAffrowsAsync();
            await _sqliteSql.Delete<Organization>().Where(o => orgCodes.Contains(o.Guid)).ExecuteAffrowsAsync();

            if (result > 0)
            {
                return ResultData.CreateSuccessResult();
            }
            else
            {
                return ResultData.CreateResult("-1", "删除失败", null);
            }
        }

        [HttpPost]
        [Route("/org/TestConn")]
        public async Task<ResultData> TestConn([FromBody] Organization organization)
        {
            string conn = string.Empty;
            try
            {
                var databaseInfo = organization.GetConnString(organization);
                using (var dataConn = new FreeSqlBuilder()
                        .UseConnectionFactory(databaseInfo.dataType, () => databaseInfo.dbConnection)
                        .UseAutoSyncStructure(false)
                        .Build())
                {
                    _ = await dataConn.Ado.ExecuteScalarAsync(databaseInfo.testSql);
                }
                return ResultData.CreateSuccessResult("测试连接成功");
            }
            catch (Exception ex)
            {
                
                return ResultData.CreateResult("-1", "测试连接失败，请检查账套信息！"+ex.Message, null);
            }
            
        }
    }
}
