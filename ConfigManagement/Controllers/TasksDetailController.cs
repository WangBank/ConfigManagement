using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConfigManagement.Common;
using ConfigManagement.Models;
using ConfigManagement.Models.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;
namespace ConfigManagement.Controllers
{
    public class TasksDetailController : Controller
    {
        [Route("tasksDetailIndex")]
        public IActionResult Index()
        {
            return View();
        }


        public IFreeSql _sqliteSql;
        private readonly SqlSugarClient _dbContext;


        public TasksDetailController(IFreeSql<SqlLiteFlag> sqliteSql, IDbFactory factory)
        {
            _dbContext = factory.GetDbContext("sqlite");
            _sqliteSql = sqliteSql;
        }

        [Route("/AddTasksDetailInfo")]
        public async Task<IActionResult> AddTasksDetailInfo()
        {
            ViewBag.BusinessCategory = _dbContext.Queryable<ConfigInfo>().ToList();
            ViewBag.TasksList = await _sqliteSql.Select<Tasks>().ToListAsync();
            ViewBag.OrgList = await _sqliteSql.Select<Organization>().ToListAsync();
            ViewBag.ScriptList = await _sqliteSql.Select<Script>().ToListAsync();
            return View();
        }


        [Route("/Api/GetTasksDetailList")]
        public async Task<LayuiData> GetTasksDetailList(string guid ="")
        {
            var items = string.IsNullOrEmpty(guid)? await _sqliteSql.Select<TasksDetail>().ToListAsync(): await _sqliteSql.Select<TasksDetail>().Where(o=>o.TaskCode==int.Parse(guid)).ToListAsync();
            var TaskDetailViewModelList = new List<TaskDetailViewModel>();
            foreach (var item in items)
            {
                TaskDetailViewModelList.Add(new TaskDetailViewModel { 
                    ScriptCode = item.ScriptCode,
                    Guid = item.Guid,
                    OrgCode = item.OrgCode,
                    OrgName = await _sqliteSql.Select<Organization>().Where(o=>o.Guid ==item.OrgCode).FirstAsync(o=>o.Name),
                    TaskCode = item.TaskCode,
                    TaskName = await _sqliteSql.Select<Tasks>().Where(o => o.Guid == item.TaskCode).FirstAsync(o => o.Name),
                    ScriptName= await _sqliteSql.Select<Script>().Where(o => o.Guid == item.ScriptCode).FirstAsync(o => o.Name)
                });
            }
            return LayuiData.CreateResult(TaskDetailViewModelList.Count, TaskDetailViewModelList);
        }


        [Route("/UpdateTasksDetailInfo")]
        public async Task<IActionResult> UpdateTasksDetailInfo(int guid)
        {
            ViewBag.IsTaskDetail = "1";
            ViewBag.TasksList = await _sqliteSql.Select<Tasks>().ToListAsync();
            ViewBag.OrgList = await _sqliteSql.Select<Organization>().ToListAsync();
            ViewBag.ScriptList = await _sqliteSql.Select<Script>().ToListAsync();
            var orgInfo = await _sqliteSql.Select<TasksDetail>().Where(o => o.Guid == guid).FirstAsync();
            return View(orgInfo);
        }


        [HttpPost]
        [Route("/Api/AddTasksDetailInfo")]
        public async Task<ResultData> AddTaskDetailInfo([FromBody] TasksDetail Task)
        {
            var insetresult = await _sqliteSql.Insert(new TasksDetail
            {
                ScriptCode = Task.ScriptCode,
                OrgCode = Task.OrgCode,
                TaskCode = Task.TaskCode
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
        [Route("/Api/UpdateTasksDetailInfo")]
        public async Task<ResultData> UpdateTaskDetailInfo([FromBody] TasksDetail Task)
        {
            var insetresult = await _sqliteSql.Update<TasksDetail>().SetSource(Task).ExecuteAffrowsAsync();

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
        [Route("/Api/DeleteTasksDetailInfo")]
        public async Task<ResultData> DeleteTaskDetailInfo([FromBody] List<TasksDetail> Tasks)
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
            result = await _sqliteSql.Delete<TasksDetail>().Where(o => orgCodes.Contains(o.Guid)).ExecuteAffrowsAsync();

            if (result > 0)
            {
                return ResultData.CreateSuccessResult();
            }
            else
            {
                return ResultData.CreateResult("-1", "删除失败", null);
            }
        }
    }
}
