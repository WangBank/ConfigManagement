using System.Collections.Generic;
using System.Threading.Tasks;
using ConfigManagement.Common;
using ConfigManagement.Models;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace ConfigManagement.Controllers
{
    [UserLogin]
    public class ServerController : Controller
    {

        private readonly SqlSugarClient _dbContext;
        public ServerController(IDbFactory factory)
        {
            _dbContext = factory.GetDbContext("sqlite");
        }

        #region 集群服务器管理

        /// <summary>
        /// 集群服务器列表页面
        /// </summary>
        /// <returns></returns>
        [Route("ServerList")]
        public IActionResult ServerList()
        {
            return View();
        }

        /// <summary>
        /// 添加服务器信息页面
        /// </summary>
        /// <returns></returns>
        [Route("/AddServerInfo")]
        public IActionResult AddServerInfo()
        {
            ViewBag.BusinessCategory = _dbContext.Queryable<BusinessCategory>().ToList();
            return View();
        }

        /// <summary>
        /// 更新服务器信息页面
        /// </summary>
        /// <returns></returns>
        [Route("/UpdateServerInfo")]
        public IActionResult UpdateServerInfo(string guid)
        {
            ViewBag.BusinessCategory = _dbContext.Queryable<BusinessCategory>().ToList();
            ServerInfo serverInfo = _dbContext.Queryable<ServerInfo>().Where(it => it.Guid == guid).First();
            return View(serverInfo);
        }

        /// <summary>
        /// 获取服务器列表
        /// </summary>
        /// <returns></returns>
        [Route("/Api/GetServerList")]
        public LayuiData GetServerList()
        {
           
            List<ServerInfo> data = _dbContext.SqlQueryable<ServerInfo>("select A.*,B.BusinessName from ServerInfo A left join BusinessCategory B on A.BusinessGuid = B.Guid").ToList();
            return LayuiData.CreateResult(data.Count, data);
        }

        /// <summary>
        /// 添加服务器信息
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/Api/AddServerInfo")]
        public async Task<ResultData> AddServerInfo([FromBody]ServerInfo info)
        {
            if (string.IsNullOrEmpty(info.BusinessGuid) || string.IsNullOrEmpty(info.ServerAddress))
            {
                return ResultData.CreateResult("-1", "缺少必填字段", null);
            }
            return await ApiUtils.InsertData(_dbContext, info);
        }

        [HttpPost]
        [Route("/Api/UpdateServerInfo")]
        public async Task<ResultData> UpdateServerInfo([FromBody]ServerInfo info)
        {
            if (string.IsNullOrEmpty(info.BusinessGuid) || string.IsNullOrEmpty(info.ServerAddress))
            {
                return ResultData.CreateResult("-1", "缺少必填字段", null);
            }
            return await ApiUtils.UpdateData(_dbContext, info);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/Api/DeleteServerInfo")]
        public async Task<ResultData> DeleteServerInfo([FromBody]List<ServerInfo> infos)
        {
            return await ApiUtils.DeleteData(_dbContext, infos);
        }
        #endregion
    }
}