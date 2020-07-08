using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConfigManagement.Common;
using ConfigManagement.Models;
using FreeSql;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace ConfigManagement.Controllers
{
    public class TaskController : Controller
    {
        [Route("taskIndex")]
        public IActionResult Index()
        {
            ViewBag.IsTask = "1";
            return View();
        }

        public IFreeSql _sqliteSql;
        private readonly SqlSugarClient _dbContext;


        public TaskController(IFreeSql<SqlLiteFlag> sqliteSql, IDbFactory factory)
        {
            _dbContext = factory.GetDbContext("sqlite");
            _sqliteSql = sqliteSql;
        }

        [Route("/AddTaskInfo")]
        public IActionResult AddTaskInfo()
        {
            ViewBag.BusinessCategory = _dbContext.Queryable<ConfigInfo>().ToList();
            return View();
        }


        [Route("/Api/GetTaskList")]
        public async Task<LayuiData> GetTaskList(string code = "", string name = "")
        {
            var items = string.IsNullOrEmpty(code) && string.IsNullOrEmpty(name)
               ? await _sqliteSql.Select<Tasks>().ToListAsync()
               : await _sqliteSql.Select<Tasks>().Where(o => o.Name.Contains(name) || o.Guid == int.Parse(code)
           ).ToListAsync();

            return LayuiData.CreateResult(items.Count, items);
        }


        [Route("/UpdateTaskInfo")]
        public async Task<IActionResult> UpdateTaskInfo(int guid)
        {
            ViewBag.IsTask = "1";
            var orgInfo = await _sqliteSql.Select<Tasks>().Where(o => o.Guid == guid).FirstAsync();
            return View(orgInfo);
        }


        [HttpPost]
        [Route("/Api/AddTaskInfo")]
        public async Task<ResultData> AddTaskInfo([FromBody] Tasks Task)
        {
            var insetresult = await _sqliteSql.Insert(new Tasks
            {
                Description = Task.Description,
                Name = Task.Name
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
        [Route("/Api/UpdateTaskInfo")]
        public async Task<ResultData> UpdateTaskInfo([FromBody] Tasks Task)
        {
            var insetresult = await _sqliteSql.Update<Tasks>().SetSource(Task).ExecuteAffrowsAsync();

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
        [Route("/Api/DeleteTaskInfo")]
        public async Task<ResultData> DeleteTaskInfo([FromBody] List<Tasks> Tasks)
        {
            if (Tasks == null || Tasks.Count == 0)
            {
                return ResultData.CreateResult("-1", "删除失败", null);
            }

            int result = 0;

            var orgCodes = new int[Tasks.Count];
            for (int i = 0; i < Tasks.Count; i++)
            {
                orgCodes[i] = Tasks[i].Guid;
            }
            result = await _sqliteSql.Delete<Tasks>().Where(o => orgCodes.Contains(o.Guid)).ExecuteAffrowsAsync();
            await _sqliteSql.Delete<TasksDetail>().Where(o => orgCodes.Contains(o.TaskCode)).ExecuteAffrowsAsync();
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
        [Route("/Api/ExecTask")]
        public async Task<ResultData> ExecTask([FromBody] List<Tasks> Tasks)
        {
          try
          {
             if (Tasks == null || Tasks.Count == 0)
            {
                return ResultData.CreateResult("-1", "执行失败", null);
            }
            foreach(var item in Tasks){
               //取出账套中连接字符串
               var taskDetails =  await _sqliteSql.Select<TasksDetail>().Where(o=>o.TaskCode==item.Guid).ToListAsync();
               foreach(var taskdetail in taskDetails){
                 var organization =await _sqliteSql.Select<Organization>().Where(o=>o.Guid== taskdetail.OrgCode).ToOneAsync();
                 var databaseInfo = organization.GetConnString(organization);
                 var scriptInfo =await _sqliteSql.Select<Script>().Where(s=>s.Guid == taskdetail.ScriptCode).ToOneAsync();
                  using (var dataConn = new FreeSqlBuilder()
                        .UseConnectionFactory(databaseInfo.dataType, () => databaseInfo.dbConnection)
                        .UseAutoSyncStructure(false)
                        .Build())
                {
                           // await Task.Delay(5000);
                            var execResult = await dataConn.Ado.ExecuteScalarAsync(scriptInfo.ScriptContent);
                    Console.WriteLine($"任务名称:{item.Name},执行的语句:{scriptInfo.ScriptContent}\r\n返回结果:{execResult}");
                }
               }
            }
              return ResultData.CreateSuccessResult();
          }
          catch(Exception ex)
          {
             return ResultData.CreateResult("-1", "执行失败"+ex.Message, null);
          }
        }
    }
}
