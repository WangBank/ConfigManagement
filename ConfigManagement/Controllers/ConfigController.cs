using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ConfigManagement.Common;
using ConfigManagement.Models;
using ConfigManagement.Models.Config;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Renci.SshNet;
using SqlSugar;

namespace ConfigManagement.Controllers
{
    [UserLogin]
    public class ConfigController : Controller
    {
        private readonly SqlSugarClient _dbContext;
        public ConfigController(IDbFactory factory)
        {
            _dbContext = factory.GetDbContext("sqlite");
        }

        #region 配置文件管理

        /// <summary>
        /// 配置文件列表页面
        /// </summary>
        /// <returns></returns>
        [Route("ConfigList")]
        public IActionResult ConfigList()
        {
            return View();
        }

        /// <summary>
        /// 添加配置文件信息页面
        /// </summary>
        /// <returns></returns>
        [Route("/AddConfigInfo")]
        public IActionResult AddConfigInfo()
        {
            ViewBag.BusinessCategory = _dbContext.Queryable<ConfigInfo>().ToList();
            return View();
        }

        /// <summary>
        /// 更新配置文件信息页面
        /// </summary>
        /// <returns></returns>
        [Route("/UpdateConfigInfo")]
        public IActionResult UpdateConfigInfo(string guid)
        {
            ConfigInfo configInfo = _dbContext.Queryable<ConfigInfo>().Where(it => it.Guid == guid).First();
            return View(configInfo);
        }

        /// <summary>
        /// 获取配置文件列表
        /// </summary>
        /// <returns></returns>
        [Route("/Api/GetConfigList")]
        public LayuiData GetConfigList()
        {
            List<ConfigInfo> data = _dbContext.Queryable<ConfigInfo>().ToList();
            return LayuiData.CreateResult(data.Count, data);
        }

        /// <summary>
        /// 添加服务器信息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/Api/AddConfigInfo")]
        public async Task<ResultData> AddConfigInfo([FromBody]ConfigInfo info)
        {
            KeyValuePair<bool, string> keyValue = ApiUtils.CheckFuncParams(info, "ConfigName", "ConfigPath", "LocalPath", "CategoryType");
            if (!keyValue.Key)
            {
                return ResultData.CreateResult("-1", $"验证失败，字段{keyValue.Value}不能为空", null);
            }

            info.CreateDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            info.IsSystem = "0";
            return await ApiUtils.InsertData(_dbContext, info);
        }

        [HttpPost]
        [Route("/Api/UpdateConfigInfo")]
        public async Task<ResultData> UpdateConfigInfo([FromBody]ConfigInfo info)
        {
            return await ApiUtils.UpdateData(_dbContext, info);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/Api/DeleteConfigInfo")]
        public async Task<ResultData> DeleteConfigInfo([FromBody]List<ConfigInfo> infos)
        {
            if (infos.Where(x => x.IsSystem == "1").ToList().Count > 0)
            {
                return ResultData.CreateResult("-1", "删除失败！存在不可删除的系统文件！", null);
            }
            return await ApiUtils.DeleteData(_dbContext, infos);
        }

        #endregion

        #region FTP

        /// <summary>
        /// 测试ftp是否能链接成功
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/Api/FTPTestConnection")]
        public ResultData FTPTestConnection(string guid)
        {
            try
            {
                ServerInfo info = _dbContext.Queryable<ServerInfo>().Where(x => x.Guid == guid).First();
                using var client = new SftpClient(info.ServerAddress, Convert.ToInt32(info.Port), info.UserName, info.Password);
                client.Connect();
                client.Dispose();
                return ResultData.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return ResultData.CreateResult("-1", ex.Message, null);
            }
        }

        /// <summary>
        /// 配置文件同步
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/Api/ConfigSynchronous")]
        public async Task<ResultData> ConfigSynchronous([FromBody]ConfigSynchronousRequest info)
        {
            try
            {
                if (info == null)
                {
                    return ResultData.CreateResult("-1", "同步失败", null);
                }
                List<ServerInfo> serverInfos = info.ServerInfos;
                List<ConfigInfo> configInfos = info.ConfigInfos;


                //获取服务器信息
                List<ServerInfo> servers = _dbContext.Queryable<ServerInfo>().In(x => x.Guid, configInfos.Select(it => it.Guid).ToArray()).ToList();
                if (servers == null || servers.Count == 0)
                {
                    return ResultData.CreateResult("-1", "未找到服务器信息", null);
                }

                //获取要同步的配置文件信息
                List<ConfigInfo> configs = _dbContext.Queryable<ConfigInfo>().In(x => x.Guid, serverInfos.Select(it => it.Guid).ToArray()).ToList();
                if (configs == null || configs.Count == 0)
                {
                    return ResultData.CreateResult("-1", "未找到配置文件信息", null);
                }

                StringBuilder stringBuilder = new StringBuilder();

                foreach (var server in servers)
                {
                    foreach (var config in configs)
                    {
                        string localPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Configs{config.LocalPath}{config.ConfigName}".ToSystemPath();
                        string remotePath = $"{config.ConfigPath}{config.ConfigName}";

                        ResultData result = await SftpHelp.UpLoad(server.ServerAddress, server.Port.ToInt32(), server.UserName, server.Password, remotePath, localPath);
                        if (result.Code != "0")
                        {
                            stringBuilder.Append($"配置文件:{config.ConfigName}同步至服务器:{server.ServerAddress}失败 错误原因{result.Msg} <br>");
                        }
                    }
                }

                if (stringBuilder.Length > 0)
                {
                    return ResultData.CreateResult("1", stringBuilder.ToString(), null);
                }

                return ResultData.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return ResultData.CreateResult("-1", $"系统异常:{ex.Message}", null);
            }
        }

        [HttpPost]
        [Route("/Api/ConfigSynchronousByFileName")]
        public async Task<ResultData> ConfigSynchronousByFileName([FromBody]ConfigSynchronousByFileNameRequest info)
        {
            try
            {
                if (info == null)
                {
                    return ResultData.CreateResult("-1", "同步失败", null);
                }
                string fileName = info.FileName;
                List<ConfigInfo> configInfos = info.ConfigInfos;


                //获取服务器信息
                List<ServerInfo> servers = _dbContext.Queryable<ServerInfo>().In(x => x.Guid, configInfos.Select(it => it.Guid).ToArray()).ToList();
                if (servers == null || servers.Count == 0)
                {
                    return ResultData.CreateResult("-1", "未找到服务器信息", null);
                }

                //获取要同步的配置文件信息
                List<ConfigInfo> configs = _dbContext.Queryable<ConfigInfo>().Where(x => x.ConfigName == fileName).ToList();
                if (configs == null || configs.Count == 0)
                {
                    return ResultData.CreateResult("-1", "未找到配置文件信息", null);
                }

                StringBuilder stringBuilder = new StringBuilder();

                foreach (var server in servers)
                {
                    foreach (var config in configs)
                    {
                        string localPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Configs{config.LocalPath}{config.ConfigName}".ToSystemPath();
                        string remotePath = $"{config.ConfigPath}{config.ConfigName}";

                        ResultData result = await SftpHelp.UpLoad(server.ServerAddress, server.Port.ToInt32(), server.UserName, server.Password, remotePath, localPath);
                        if (result.Code != "0")
                        {
                            stringBuilder.Append($"配置文件:{config.ConfigName}同步至服务器:{server.ServerAddress}失败 错误原因{result.Msg} <br>");
                        }
                    }
                }

                if (stringBuilder.Length > 0)
                {
                    return ResultData.CreateResult("1", stringBuilder.ToString(), null);
                }

                return ResultData.CreateSuccessResult();
            }
            catch (Exception ex)
            {
                return ResultData.CreateResult("-1", $"系统异常:{ex.Message}", null);
            }
        }


        #endregion

        #region 同步文件时选择服务器页面

        [Route("SelectServer")]
        public IActionResult SelectServer()
        {
            return View();
        }

        [Route("SelectServers")]
        public IActionResult SelectServers()
        {
            return View();
        }

        #endregion

        #region 配置文件表单

        /// <summary>
        /// 修改Redis.json界面
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        [Route("UpdateRedis")]
        public async Task<IActionResult> UpdateRedis(string guid)
        {
            string localPath = ApiUtils.GetLocalPath(_dbContext, guid, out string errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return Content(errorMessage);
            }

            using StreamReader reader = new StreamReader(localPath);
            string json = await reader.ReadToEndAsync();
            List<RedisInfo> info = JsonConvert.DeserializeObject<List<RedisInfo>>(json);
            ViewBag.Guid = guid;
            return View(info);
        }

        /// <summary>
        /// 修改Redis.json接口
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("/Api/UpdateRedis")]
        public async Task<ResultData> UpdateRedis()
        {
            using StreamReader bodyReader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8);
            var data = await bodyReader.ReadToEndAsync();
            JObject jObject = JObject.Parse(data);
            string localPath = ApiUtils.GetLocalPath(_dbContext, jObject["Guid"].ToString(), out string errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return ResultData.CreateResult("-1", errorMessage, null);
            }

            List<RedisInfo> info;
            using (StreamReader reader = new StreamReader(localPath))
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


            using StreamWriter writer = new StreamWriter(localPath, false, Encoding.UTF8);
            await writer.WriteAsync(ApiUtils.ConvertJsonString(JsonConvert.SerializeObject(info)));

            return ResultData.CreateSuccessResult();
        }

        /// <summary>
        /// 修改RabbitMQ.json界面
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        [Route("UpdateRabbitMQ")]
        public async Task<IActionResult> UpdateRabbitMQ(string guid)
        {
            string localPath = ApiUtils.GetLocalPath(_dbContext, guid, out string errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return Content(errorMessage);
            }

            using StreamReader reader = new StreamReader(localPath);
            string json = await reader.ReadToEndAsync();
            List<RabbitMQ> info = JsonConvert.DeserializeObject<List<RabbitMQ>>(json);
            ViewBag.Guid = guid;
            return View(info);
        }

        /// <summary>
        /// 修改RabbitMQ.json接口
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("/Api/UpdateRabbitMQ")]
        public async Task<ResultData> UpdateRabbitMQ()
        {
            using StreamReader bodyReader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8);
            var data = await bodyReader.ReadToEndAsync();
            JObject jObject = JObject.Parse(data);

            string localPath = ApiUtils.GetLocalPath(_dbContext, jObject["Guid"].ToString(), out string errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return ResultData.CreateResult("-1", errorMessage, null);
            }

            List<RabbitMQ> info;
            using (StreamReader reader = new StreamReader(localPath))
            {
                string json = await reader.ReadToEndAsync();
                info = JsonConvert.DeserializeObject<List<RabbitMQ>>(json);
                info[0].ServerUrl = jObject["ServerUrl"].ToString();
                info[0].ServerPort = jObject["ServerPort"].ToString();
                info[0].UserName = jObject["UserName"].ToString();
                info[0].Password = jObject["Password"].ToString();
            }


            using StreamWriter writer = new StreamWriter(localPath, false, Encoding.UTF8);
            await writer.WriteAsync(ApiUtils.ConvertJsonString(JsonConvert.SerializeObject(info)));

            return ResultData.CreateSuccessResult();
        }

        /// <summary>
        /// 修改erplog的appsettings.json界面
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        [Route("UpdateErpLog")]
        public async Task<IActionResult> UpdateErpLog(string guid)
        {
            string localPath = ApiUtils.GetLocalPath(_dbContext, guid, out string errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return Content(errorMessage);
            }

            using StreamReader reader = new StreamReader(localPath);
            string json = await reader.ReadToEndAsync();
            ErpLogConfig info = JsonConvert.DeserializeObject<ErpLogConfig>(json);
            ViewBag.Guid = guid;
            return View(info);
        }

        /// <summary>
        /// 修改erplog的appsettings.json接口
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/Api/UpdateErpLog")]
        public async Task<ResultData> UpdateErpLog()
        {
            using StreamReader bodyReader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8);
            var data = await bodyReader.ReadToEndAsync();
            JObject jObject = JObject.Parse(data);

            string localPath = ApiUtils.GetLocalPath(_dbContext, jObject["Guid"].ToString(), out string errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return ResultData.CreateResult("-1", errorMessage, null);
            }

            ErpLogConfig info;
            using (StreamReader reader = new StreamReader(localPath))
            {
                string json = await reader.ReadToEndAsync();
                info = JsonConvert.DeserializeObject<ErpLogConfig>(json);
                info.MQConnModel.ConnectionModel.Host = jObject["Host"].ToString();
                info.MQConnModel.ConnectionModel.Port = jObject["Port"].ToString();
                info.MQConnModel.ConnectionModel.UserName = jObject["UserName"].ToString();
                info.MQConnModel.ConnectionModel.Password = jObject["Password"].ToString();
            }


            using StreamWriter writer = new StreamWriter(localPath, false, Encoding.UTF8);
            await writer.WriteAsync(ApiUtils.ConvertJsonString(JsonConvert.SerializeObject(info)));

            return ResultData.CreateSuccessResult();
        }

        /// <summary>
        /// 修改Logger.json界面
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        [Route("/UpdateLogger")]
        public async Task<IActionResult> UpdateLogger(string guid)
        {
            string localPath = ApiUtils.GetLocalPath(_dbContext, guid, out string errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return Content(errorMessage);
            }

            using StreamReader reader = new StreamReader(localPath);
            string json = await reader.ReadToEndAsync();
            List<Logger> info = JsonConvert.DeserializeObject<List<Logger>>(json);
            ViewBag.Guid = guid;
            return View(info);
        }

        /// <summary>
        /// 修改Logger.json接口
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("/Api/UpdateLogger")]
        public async Task<ResultData> UpdateLogger()
        {
            using StreamReader bodyReader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8);
            var data = await bodyReader.ReadToEndAsync();
            JObject jObject = JObject.Parse(data);

            string localPath = ApiUtils.GetLocalPath(_dbContext, jObject["Guid"].ToString(), out string errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return ResultData.CreateResult("-1", errorMessage, null);
            }

            List<Logger> info;
            using (StreamReader reader = new StreamReader(localPath))
            {
                string json = await reader.ReadToEndAsync();
                info = JsonConvert.DeserializeObject<List<Logger>>(json);
                info[0].DataAdapterType = jObject["DataAdapterType"].ToString();
                info[0].DbInfo = jObject["DbInfo"].ToString();
                info[0].LogLevel = jObject["LogLevel"].ToString();
            }

            using StreamWriter writer = new StreamWriter(localPath, false, Encoding.UTF8);
            await writer.WriteAsync(ApiUtils.ConvertJsonString(JsonConvert.SerializeObject(info)));

            return ResultData.CreateSuccessResult();
        }

        /// <summary>
        /// PublicDataAdapters.json里的信息列表
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        [Route("/UpdatePublicDataAdaptersList")]
        public IActionResult UpdatePublicDataAdaptersList(string guid)
        {
            ViewBag.Guid = guid;
            return View();
        }

        /// <summary>
        /// 获取PublicDataAdapters.json的json信息
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        [Route("/Api/GetPublicDataAdaptersList")]
        public async Task<LayuiData> GetPublicDataAdaptersList(string guid)
        {
            string localPath = ApiUtils.GetLocalPath(_dbContext, guid, out string errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return LayuiData.CreateErrorResult(errorMessage);
            }

            using StreamReader reader = new StreamReader(localPath);
            string json = await reader.ReadToEndAsync();
            List<PublicDataAdapters> info = JsonConvert.DeserializeObject<List<PublicDataAdapters>>(json);
            ViewBag.Guid = guid;
            return LayuiData.CreateResult(info.Count,info);
        }

        /// <summary>
        /// 新增数据库连接信息页面
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        [Route("/AddPublicDataAdapters")]
        public IActionResult AddPublicDataAdapters(string guid)
        {
            ViewBag.Guid = guid;
            return View();
        }

        /// <summary>
        /// 更新数据库连接信息页面
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="dataAdapterAlias"></param>
        /// <returns></returns>
        [Route("/UpdatePublicDataAdapters")]
        public async Task<IActionResult> UpdatePublicDataAdapters(string  guid,string dataAdapterAlias)
        {
            string localPath = ApiUtils.GetLocalPath(_dbContext, guid, out string errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return Content(errorMessage);
            }

            using StreamReader reader = new StreamReader(localPath);
            string json = await reader.ReadToEndAsync();
            List<PublicDataAdapters> infos = JsonConvert.DeserializeObject<List<PublicDataAdapters>>(json);
            PublicDataAdapters info = infos.Where(x => x.DataAdapterAlias == dataAdapterAlias).FirstOrDefault();
            ViewBag.Guid = guid;
            return View(info);
        }

        /// <summary>
        /// 新增数据库连接信息接口
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/Api/AddPublicDataAdapters")]
        public async Task<ResultData> AddPublicDataAdapters([FromBody]PublicDataAdapters info)
        {
            string guid = Request.Query["guid"];
            string localPath = ApiUtils.GetLocalPath(_dbContext, guid, out string errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return ResultData.CreateResult("-1", errorMessage, null);
            }

            List<PublicDataAdapters> infos;

            using (StreamReader reader = new StreamReader(localPath))
            {
                string json = await reader.ReadToEndAsync();
                infos = JsonConvert.DeserializeObject<List<PublicDataAdapters>>(json);
                if (infos.Where(x => x.DataAdapterAccountName == info.DataAdapterAccountName || x.DataAdapterAlias == info.DataAdapterAlias).ToList().Count > 0)
                {
                    return ResultData.CreateResult("-1", "账套名称或账套号已存在", null);
                }
                infos.Add(info);
            }

            using StreamWriter writer = new StreamWriter(localPath, false, Encoding.UTF8);
            await writer.WriteAsync(ApiUtils.ConvertJsonString(JsonConvert.SerializeObject(infos)));

            return ResultData.CreateSuccessResult();
        }

        /// <summary>
        /// 更新数据库连接信息接口
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("/Api/UpdatePublicDataAdapters")]
        public async Task<ResultData> UpdatePublicDataAdapters()
        {
            string guid = Request.Query["guid"];
            string localPath = ApiUtils.GetLocalPath(_dbContext, guid, out string errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return ResultData.CreateResult("-1", errorMessage, null);
            }

            List<PublicDataAdapters> infos;

            using (StreamReader reader = new StreamReader(localPath))
            {
                string json = await reader.ReadToEndAsync();
                infos = JsonConvert.DeserializeObject<List<PublicDataAdapters>>(json);
            }

            using StreamReader bodyReader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8);
            var data = await bodyReader.ReadToEndAsync();
            JObject info = JObject.Parse(data);

            if (infos.Where(x => x.DataAdapterAlias != info["DataAdapterAlias"].ToNullString() && x.DataAdapterAccountName != info["DataAdapterAccountName"].ToNullString()).Where(x => x.DataAdapterAlias == info["NewDataAdapterAlias"].ToNullString() || x.DataAdapterAccountName == info["NewDataAdapterAccountName"].ToNullString()).ToList().Count > 0)
            {
                return ResultData.CreateResult("-1", "账套名称或账套号已存在", null);
            }

            foreach (var item in infos)
            {
                if (item.DataAdapterAlias != info["DataAdapterAlias"].ToNullString())
                {
                    continue;
                }

                item.DataAdapterAlias = info["NewDataAdapterAlias"].ToNullString();
                item.DataAdapterType = info["NewDataAdapterType"].ToNullString();
                item.DataAdapterInfo = info["NewDataAdapterInfo"].ToNullString();
                item.DataAdapterAccountName = info["NewDataAdapterAccountName"].ToNullString();
            }

            using StreamWriter writer = new StreamWriter(localPath, false, Encoding.UTF8);
            await writer.WriteAsync(ApiUtils.ConvertJsonString(JsonConvert.SerializeObject(infos)));

            return ResultData.CreateSuccessResult();
        }

        /// <summary>
        /// 删除数据库连接信息接口
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/Api/DeletePublicDataAdapters")]
        public async Task<ResultData> DeletePublicDataAdapters([FromBody]List<PublicDataAdapters> data)
        {
            string guid = Request.Query["guid"];
            string localPath = ApiUtils.GetLocalPath(_dbContext, guid, out string errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return ResultData.CreateResult("-1", errorMessage, null);
            }

            List<PublicDataAdapters> infos;

            using (StreamReader reader = new StreamReader(localPath))
            {
                string json = await reader.ReadToEndAsync();
                infos = JsonConvert.DeserializeObject<List<PublicDataAdapters>>(json);
            }

            string[] dataAdapterAlias = data.Select(x => x.DataAdapterAlias).ToArray();

            List<PublicDataAdapters> deleteData = infos.Where(x => dataAdapterAlias.Contains(x.DataAdapterAlias)).ToList();

            foreach (var item in deleteData)
            {
                infos.Remove(item);
            }

            using StreamWriter writer = new StreamWriter(localPath, false, Encoding.UTF8);
            await writer.WriteAsync(ApiUtils.ConvertJsonString(JsonConvert.SerializeObject(infos)));

            return ResultData.CreateSuccessResult();
        }

        #endregion
    }
}