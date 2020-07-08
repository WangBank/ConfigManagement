using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConfigManagement.Common;
using ConfigManagement.Models;
using ConfigManagement.Models.Config;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SqlSugar;

namespace ConfigManagement.Controllers
{
    [UserLogin]
    public class DataBaseController : Controller
    {

        private readonly SqlSugarClient _dbContext;
        public DataBaseController(IDbFactory factory)
        {
            _dbContext = factory.GetDbContext("sqlite");
        }

        #region 数据库连接管理

        [Route("DataBaseList")]
        public IActionResult DataBaseList()
        {
            return View();
        }

        [Route("AddDataBaseInfo")]
        public IActionResult AddDataBaseInfo()
        {
            return View();
        }

        [Route("UpdateDataBaseInfo")]
        public IActionResult UpdateDataBaseInfo(string guid)
        {
            DataBaseInfo dataBaseInfo = _dbContext.Queryable<DataBaseInfo>().Where(it => it.Guid == guid).First();
            return View(dataBaseInfo);
        }

        /// <summary>
        /// 获取数据库连接列表
        /// </summary>
        /// <returns></returns>
        [Route("/Api/GetDataBaseList")]
        public LayuiData GetDataBaseList()
        {
            List<DataBaseInfo> data = _dbContext.Queryable<DataBaseInfo>().OrderBy(x => x.BusinessType).ToList();
            return LayuiData.CreateResult(data.Count, data);
        }


        /// <summary>
        /// 添加数据库连接
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/Api/AddDataBaseInfo")]
        public async Task<ResultData> AddDataBaseInfo([FromBody]DataBaseInfo info)
        {
            //检查重复
            if (_dbContext.Queryable<DataBaseInfo>().Where(x => x.BusinessType == info.BusinessType && (x.AccountSetName == info.AccountSetName || x.AccountSetNumber == info.AccountSetNumber)).Count() > 0)
            {
                return ResultData.CreateResult("-1", "账套号或账套名称已存在", null);
            }

            //获取配置文件信息
            ConfigInfo configInfo = _dbContext.Queryable<ConfigInfo>().Where(x => x.ConfigName == "PublicDataAdapters.json" && x.CategoryType == info.BusinessType).First();
            if (configInfo == null)
            {
                return ResultData.CreateResult("-1", "配置文件不存在", null);
            }

            //判断配置文件是否存在
            string localPath = ApiUtils.GetLocalPath(_dbContext, configInfo.Guid, out string errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return ResultData.CreateResult("-1", errorMessage, null);
            }

            //生成加密信息
            PublicDataAdapters publicDataAdapter = new PublicDataAdapters
            {
                DataAdapterType = info.DataBaseType,
                DataAdapterAlias = info.AccountSetNumber,
                IsDefaultDataAdapter = "0",
                DataAdapterAccountName = info.AccountSetName,
                DataAdapterInfo = $"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={info.ServerAddress})(PORT={info.Port})))(CONNECT_DATA=(SERVICE_NAME={info.DataBaseName})));User Id={info.UserName};Password={info.UserPassword};{info.ConnectionStringOther}".ToEncodeString()
            };

            List<PublicDataAdapters> infos;
            using (StreamReader reader = new StreamReader(localPath))
            {
                string json = await reader.ReadToEndAsync();
                infos = JsonConvert.DeserializeObject<List<PublicDataAdapters>>(json);
                if (infos.Where(x => x.DataAdapterAccountName == publicDataAdapter.DataAdapterAccountName || x.DataAdapterAlias == publicDataAdapter.DataAdapterAlias).ToList().Count > 0)
                {
                    return ResultData.CreateResult("-1", "配置文件中账套名称或账套号已存在", null);
                }
                infos.Add(publicDataAdapter);
            }

            //插入数据库
            ResultData result = await ApiUtils.InsertData(_dbContext, info);

            //修改本地文件
            using StreamWriter writer = new StreamWriter(localPath, false, Encoding.UTF8);
            await writer.WriteAsync(ApiUtils.ConvertJsonString(JsonConvert.SerializeObject(infos)));

            return result;
        }

        /// <summary>
        /// 更新数据库连接
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/Api/UpdateDataBaseInfo")]
        public async Task<ResultData> UpdateDataBaseInfo([FromBody]DataBaseInfo info)
        {
            //检查重复
            if (_dbContext.Queryable<DataBaseInfo>().Where(x => x.Guid != info.Guid && x.BusinessType == info.BusinessType && (x.AccountSetName == info.AccountSetName || x.AccountSetNumber == info.AccountSetNumber)).Count() > 0)
            {
                return ResultData.CreateResult("-1", "账套号或账套名称已存在", null);
            }

            //获取配置文件信息
            ConfigInfo configInfo = _dbContext.Queryable<ConfigInfo>().Where(x => x.ConfigName == "PublicDataAdapters.json" && x.CategoryType == info.BusinessType).First();
            if (configInfo == null)
            {
                return ResultData.CreateResult("-1", "配置文件不存在", null);
            }

            //判断配置文件是否存在
            string localPath = ApiUtils.GetLocalPath(_dbContext, configInfo.Guid, out string errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return ResultData.CreateResult("-1", errorMessage, null);
            }

            //生成加密信息
            PublicDataAdapters publicDataAdapter = new PublicDataAdapters
            {
                DataAdapterType = info.DataBaseType,
                DataAdapterAlias = info.AccountSetNumber,
                IsDefaultDataAdapter = "0",
                DataAdapterAccountName = info.AccountSetName,
                DataAdapterInfo = $"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={info.ServerAddress})(PORT={info.Port})))(CONNECT_DATA=(SERVICE_NAME={info.DataBaseName})));User Id={info.UserName};Password={info.UserPassword};{info.ConnectionStringOther}".ToEncodeString()
            };

            List<PublicDataAdapters> infos;

            //获取旧的数据库连接信息
            DataBaseInfo oldinfo = _dbContext.Queryable<DataBaseInfo>().Where(x => x.Guid == info.Guid).Single();

            using (StreamReader reader = new StreamReader(localPath))
            {
                string json = await reader.ReadToEndAsync();
                infos = JsonConvert.DeserializeObject<List<PublicDataAdapters>>(json);
            }

            if (infos.Where(x => x.DataAdapterAlias != oldinfo.AccountSetNumber && x.DataAdapterAccountName != oldinfo.AccountSetName).Where(x => x.DataAdapterAlias == info.AccountSetNumber || x.DataAdapterAccountName == info.AccountSetName).ToList().Count > 0)
            {
                return ResultData.CreateResult("-1", "账套名称或账套号已存在", null);
            }

            foreach (var item in infos)
            {
                if (item.DataAdapterAlias != oldinfo.AccountSetNumber)
                {
                    continue;
                }

                item.DataAdapterAlias = publicDataAdapter.DataAdapterAlias;
                item.DataAdapterType = publicDataAdapter.DataAdapterType;
                item.DataAdapterInfo = publicDataAdapter.DataAdapterInfo;
                item.DataAdapterAccountName = publicDataAdapter.DataAdapterAccountName;
                break;
            }

            using (StreamWriter writer = new StreamWriter(localPath, false, Encoding.UTF8))
            {
                await writer.WriteAsync(ApiUtils.ConvertJsonString(JsonConvert.SerializeObject(infos)));
            }
            
            ResultData result = await ApiUtils.UpdateData(_dbContext, info);
            return result;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/Api/DeleteDataBaseInfo")]
        public async Task<ResultData> DeleteDataBaseInfo([FromBody]List<DataBaseInfo> infos)
        {
            //要删除的业务中间层信息
            List<DataBaseInfo> ywinfos = infos.Where(x => x.BusinessType == "业务中间层").ToList();
            //要删除的报表中间层信息
            List<DataBaseInfo> bbinfos = infos.Where(x => x.BusinessType == "报表中间层").ToList();

            if (ywinfos.Count > 0)
            {
                ResultData resultData = await DeleteDataBaseInfo(ywinfos, "业务中间层");
                if (resultData.Code != "0")
                {
                    return resultData;
                }
            }

            if (bbinfos.Count > 0)
            {
                ResultData resultData = await DeleteDataBaseInfo(bbinfos, "报表中间层");
                if (resultData.Code != "0")
                {
                    return resultData;
                }
            }
            ResultData result = await ApiUtils.DeleteData(_dbContext, infos);
            return result;
        }

        public async Task<ResultData> DeleteDataBaseInfo(List<DataBaseInfo> infos,string type)
        {
            //获取配置文件信息
            ConfigInfo configInfo = _dbContext.Queryable<ConfigInfo>().Where(x => x.ConfigName == "PublicDataAdapters.json" && x.CategoryType == type).First();
            if (configInfo == null)
            {
                return ResultData.CreateResult("-1", "配置文件不存在", null);
            }

            string localPath = ApiUtils.GetLocalPath(_dbContext, configInfo.Guid, out string errorMessage);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return ResultData.CreateResult("-1", errorMessage, null);
            }

            List<PublicDataAdapters> datainfos;

            using (StreamReader reader = new StreamReader(localPath))
            {
                string json = await reader.ReadToEndAsync();
                datainfos = JsonConvert.DeserializeObject<List<PublicDataAdapters>>(json);
            }

            string[] dataAdapterAlias = infos.Select(x => x.AccountSetNumber).ToArray();

            List<PublicDataAdapters> deleteData = datainfos.Where(x => dataAdapterAlias.Contains(x.DataAdapterAlias)).ToList();

            foreach (var item in deleteData)
            {
                datainfos.Remove(item);
            }

            using (StreamWriter writer = new StreamWriter(localPath, false, Encoding.UTF8))
            {
                await writer.WriteAsync(ApiUtils.ConvertJsonString(JsonConvert.SerializeObject(datainfos)));
            }

            return ResultData.CreateSuccessResult();
        }
        #endregion

        #region 生成数据库连接串
        [HttpPost]
        [Route("/Api/BuildPublicDataAdapters")]
        public ResultData BuildPublicDataAdapters([FromBody]List<DataBaseInfo> infos)
        {
            try
            {
                List<DataBaseInfo> dataBaseInfos = _dbContext.Queryable<DataBaseInfo>().In(infos.Select(x => x.Guid).ToArray()).ToList();
                List<PublicDataAdapters> publicDataAdapters = new List<PublicDataAdapters>();
                foreach (var item in dataBaseInfos)
                {
                    PublicDataAdapters publicDataAdapter = new PublicDataAdapters
                    {
                        DataAdapterType = item.DataBaseType,
                        DataAdapterAlias = item.AccountSetNumber,
                        IsDefaultDataAdapter = "0",
                        DataAdapterAccountName = item.AccountSetName,
                        DataAdapterInfo = $"Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={item.ServerAddress})(PORT={item.Port})))(CONNECT_DATA=(SERVICE_NAME={item.DataBaseName})));User Id={item.UserName};Password={item.UserPassword};{item.ConnectionStringOther}".ToEncodeString()
                    };
                    publicDataAdapters.Add(publicDataAdapter);
                }
                return ResultData.CreateSuccessResult("", JsonConvert.SerializeObject(publicDataAdapters));
            }
            catch (Exception ex)
            {
                return ResultData.CreateResult("-1", ex.Message, null);
            }
        }

        [Route("JsonPreview")]
        public IActionResult JsonPreview()
        {
            return View();
        }
        #endregion

        #region 从配置文件导入数据库链接

        //[HttpPost]
        //[Route("/Api/ImportPublicDataAdapters")]
        //public async Task<ResultData> ImportPublicDataAdapters()
        //{
        //    var files = Request.Form.Files;
        //    if (files == null || files.Count == 0)
        //    {
        //        return ResultData.CreateResult("-1", "无效的文件", null);
        //    }

        //    MemoryStream memoryStream = new MemoryStream();
        //    await files[0].CopyToAsync(memoryStream);
            
        //    string  datas = System.Text.Encoding.UTF8.GetString(memoryStream.GetBuffer());

        //    List<PublicDataAdapters> dataAdapters = JsonConvert.DeserializeObject<List<PublicDataAdapters>>(datas);

        //    return ResultData.CreateSuccessResult();
        //}

        #endregion
    }
}