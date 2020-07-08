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
    public class RabbitMQController : Controller
    {
        private readonly SqlSugarClient _dbContext;
        public RabbitMQController(IDbFactory factory)
        {
            _dbContext = factory.GetDbContext("sqlite");
        }

        /// <summary>
        /// 修改RabbitMQ.json界面
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        [Route("UpdateRabbitMQConfig")]
        public async Task<IActionResult> UpdateRabbitMQ()
        {
            List<string> localPath = ApiUtils.GetLocalPathByFileName(_dbContext, "RabbitMQ.json", out string errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return Content(errorMessage);
            }

            using StreamReader reader = new StreamReader(localPath[0]);
            string json = await reader.ReadToEndAsync();
            List<RabbitMQ> info = JsonConvert.DeserializeObject<List<RabbitMQ>>(json);
            return View(info);
        }

        /// <summary>
        /// 修改RabbitMQ.json接口
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("/Api/UpdateRabbitMQConfig")]
        public async Task<ResultData> UpdateRabbitMQApi()
        {
            using StreamReader bodyReader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8);
            var data = await bodyReader.ReadToEndAsync();
            JObject jObject = JObject.Parse(data);

            List<string> localPath = ApiUtils.GetLocalPathByFileName(_dbContext, "RabbitMQ.json", out string errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return ResultData.CreateResult("-1", errorMessage, null);
            }

            List<RabbitMQ> info;
            using (StreamReader reader = new StreamReader(localPath[0]))
            {
                string json = await reader.ReadToEndAsync();
                info = JsonConvert.DeserializeObject<List<RabbitMQ>>(json);
                info[0].ServerUrl = jObject["ServerUrl"].ToString();
                info[0].ServerPort = jObject["ServerPort"].ToString();
                info[0].UserName = jObject["UserName"].ToString();
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