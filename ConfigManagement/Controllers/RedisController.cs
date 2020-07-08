using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ConfigManagement.Common;
using ConfigManagement.Models;
using ConfigManagement.Models.Config;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;

namespace ConfigManagement.Controllers
{
    public class RedisController : Controller
    {
        private readonly SqlSugarClient _dbContext;
        public RedisController(IDbFactory factory)
        {
            _dbContext = factory.GetDbContext("sqlite");
        }

        /// <summary>
        /// 修改所有Redis.json界面
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        [Route("UpdateRedisConfig")]
        public async Task<IActionResult> UpdateRedis()
        {
            List<string> localPath = ApiUtils.GetLocalPathByFileName(_dbContext, "Redis.json", out string errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return Content(errorMessage);
            }

            using StreamReader reader = new StreamReader(localPath[0]);
            string json = await reader.ReadToEndAsync();
            List<RedisInfo> info = JsonConvert.DeserializeObject<List<RedisInfo>>(json);
            return View(info);
        }

        /// <summary>
        /// 修改所有Redis.json接口
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("/Api/UpdateRedisConfig")]
        public async Task<ResultData> UpdateRedisApi()
        {
            using StreamReader bodyReader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8);
            var data = await bodyReader.ReadToEndAsync();
            JObject jObject = JObject.Parse(data);

            List<string> localPath = ApiUtils.GetLocalPathByFileName(_dbContext, "Redis.json", out string errorMessage);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                return ResultData.CreateResult("-1", errorMessage, null);
            }

            List<RedisInfo> info;
            using (StreamReader reader = new StreamReader(localPath[0]))
            {
                string json = await reader.ReadToEndAsync();
                info = JsonConvert.DeserializeObject<List<RedisInfo>>(json);
                info[0].ServiceName = jObject["ServiceName"].ToString();
                info[0].Servers[0].IP = jObject["IP"].ToString();
                info[0].Servers[0].Port = jObject["Port"].ToString().ToInt32();
                info[0].Address = jObject["Address"].ToString();
                info[0].InstanceName = jObject["InstanceName"].ToString();
                info[0].Password = jObject["Password"].ToString();
            }

            foreach (var item in localPath)
            {
                using (StreamWriter writer = new StreamWriter(item, false, Encoding.UTF8))
                {
                    await writer.WriteAsync(ApiUtils.ConvertJsonString(JsonConvert.SerializeObject(info)));
                }  
            }

            return ResultData.CreateSuccessResult();
        }
    }
}