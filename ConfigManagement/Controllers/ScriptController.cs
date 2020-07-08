using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConfigManagement.Common;
using ConfigManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace ConfigManagement.Controllers
{
    public class ScriptController : Controller
    {
        [Route("scriptIndex")]
        public IActionResult Index()
        {
          
            ViewBag.UserInfo = HttpContext.Session.GetData<UserInfo>("user");
            return View();
        }


        public IFreeSql _sqliteSql;
        private readonly SqlSugarClient _dbContext;


        public ScriptController(IFreeSql<SqlLiteFlag> sqliteSql, IDbFactory factory)
        {
            _dbContext = factory.GetDbContext("sqlite");
            _sqliteSql = sqliteSql;
        }

        [Route("/AddScriptInfo")]
        public IActionResult AddScriptInfo()
        {
            ViewBag.BusinessCategory = _dbContext.Queryable<ConfigInfo>().ToList();
            return View();
        }


        [Route("/Api/GetScriptList")]
        public async Task<LayuiData> GetScriptList(string code = "", string name = "")
        {
            var items = string.IsNullOrEmpty(code) && string.IsNullOrEmpty(name)
               ? await _sqliteSql.Select<Script>().ToListAsync()
               : await _sqliteSql.Select<Script>().Where(o => o.Name.Contains(name) || o.Guid == int.Parse(code)
           ).ToListAsync();

            return LayuiData.CreateResult(items.Count, items);
        }


        [Route("/UpdateScriptInfo")]
        public async Task<IActionResult> UpdateScriptInfo(int guid)
        {
            ViewBag.IsScript = "1";
            var orgInfo = await _sqliteSql.Select<Script>().Where(o => o.Guid == guid).FirstAsync();
            return View(orgInfo);
        }


        [HttpPost]
        [Route("/Api/AddScriptInfo")]
        public async Task<ResultData> AddScriptInfo([FromBody] Script script)
        {
            var insetresult = await _sqliteSql.Insert(new Script
            {
                ScriptContent = script.ScriptContent,
                Description = script.Description,
                Name = script.Name
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
        [Route("/Api/UpdateScriptInfo")]
        public async Task<ResultData> UpdateScriptInfo([FromBody] Script script)
        {
            var insetresult = await _sqliteSql.Update<Script>().SetSource(script).ExecuteAffrowsAsync();

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
        [Route("/Api/DeleteScriptInfo")]
        public async Task<ResultData> DeleteScriptInfo([FromBody] List<Script> scripts)
        {
            if (scripts == null || scripts.Count == 0)
            {
                return ResultData.CreateResult("-1", "删除失败", null);
            }

            int result = 0;

            var orgCodes = new int[scripts.Count];
            for (int i = 0; i < scripts.Count; i++)
            {
                orgCodes[i] = scripts[i].Guid;
            }
            result = await _sqliteSql.Delete<Script>().Where(o => orgCodes.Contains(o.Guid)).ExecuteAffrowsAsync();
            await _sqliteSql.Delete<TasksDetail>().Where(o => orgCodes.Contains(o.ScriptCode)).ExecuteAffrowsAsync();


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
        [Route("/Api/GetFileContext")]
        public async Task<ResultData> GetFileContext(IFormFile file)
        {
            await Task.Run(()=>
                Console.WriteLine(file.FileName)   
            );


            string content = string.Empty;
            using (var fileContent = file.OpenReadStream())
            {
                fileContent.Position = 0;
                StreamReader reader = new StreamReader(fileContent);
                content = reader.ReadToEnd();
            }
           return ResultData.CreateSuccessResult("", content);
            
        }


    }
}
