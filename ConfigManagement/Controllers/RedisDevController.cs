using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConfigManagement.Common;
using ConfigManagement.Common.Page;
using ConfigManagement.Models;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace ConfigManagement.Controllers
{
    public class RedisDevController : Controller
    {
        private readonly SqlSugarClient _dbContext;
        public RedisDevController(IDbFactory factory)
        {
            _dbContext = factory.GetDbContext("sqlite");
        }
        public IActionResult Index()
        {
            ViewBag.RedisDBAddressInfo = _dbContext.Queryable<RedisDBSetting>().ToList();
            return View();
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        [Route("/Api/GetRedisDevList")]
        public async Task<LayuiData> GetRedisDevList(int page, int limit, string redisaddr, int redisdb, string searchkey)
        {
            if (string.IsNullOrEmpty(redisaddr))
            {
                return LayuiData.CreateResult(0, null);
            }
            #region 获取Redis的地址，连接实例
            string redisConn = await GetRedisDBAddr(redisaddr);
            RedisHelper redisHelper = new RedisHelper(redisConn);
            #endregion
            #region 根据地址查询键值对，如果redisdb为空就查询所有的（限制到1000条数据），如果searchkey为空也是如此
            List<RedisDataModel> redisDataModel = new List<RedisDataModel>();
            var data = redisHelper.GetKeys(redisdb, searchkey ?? "*"); ;
            int rowCount = data.Count;
            data = PageHelper.GetPageData<RedisDataModel>(data, page, limit);
            #endregion
            return LayuiData.CreateResult(rowCount, data);
        }


        /// <summary>
        /// 移除键
        /// </summary>
        /// <returns></returns>
        [Route("/Api/DeleteRedisDev")]
        public async Task<ResultData> DeleteRedisDev(string redisaddr, int redisdb, string searchkey)
        {
            #region 获取Redis的地址，连接实例
            string redisConn = await GetRedisDBAddr(redisaddr);
            RedisHelper redisHelper = new RedisHelper(redisConn);
            #endregion
            var result = await redisHelper.ClearKeys(redisdb, searchkey);
            if (result.Item1)
            {
                return ResultData.CreateSuccessResult("删除成功");
            }
            else
            {
                return ResultData.CreateResult("-1", $"删除键失败，失败信息：{result.Item2}", null);
            }
        }

        public IActionResult AddRedisDevInfo(string redisaddr, int redisdb)
        {
            ViewBag.RedisAddr = redisaddr;
            ViewBag.RedisDB = redisdb;
            return View();
        }

        public async Task<IActionResult> EditRedisDevInfo(string redisaddr, int redisdb, string searchkey)
        {
            ViewBag.RedisAddr = redisaddr;
            ViewBag.RedisDB = redisdb;
            string redisConn = await GetRedisDBAddr(redisaddr);
            RedisHelper redisHelper = new RedisHelper(redisConn);
            RedisDataModel redisDataModel = new RedisDataModel();
            redisDataModel.Type = "String";
            redisDataModel.Key = searchkey;
            redisDataModel.Value = redisHelper.GetValue(searchkey, redisdb);
            TimeSpan? time = redisHelper.GetExpireTime(searchkey, redisdb);
            if (time == null)
            {
                redisDataModel.TTLTime = null;
            }
            else
            {
                redisDataModel.TTLTime = DateTime.Now.Add((TimeSpan)time);
            }
            return View(redisDataModel);
        }

        /// <summary>
        /// 新增键值对
        /// </summary>
        /// <returns></returns>
        [Route("/Api/AddRedisDevInfo")]
        public async Task<ResultData> AddRedisDevInfo([FromBody]RedisDataModel model)
        {
            try
            {
                if (model.TTLTime <= DateTime.Now)
                    return ResultData.CreateResult("-1", "TTL过期时间不能早于当前日期", null);
                string redisConn = await GetRedisDBAddr(model.RedisAddr);
                RedisHelper redisHelper = new RedisHelper(redisConn);
                var timeSpan = model.TTLTime - DateTime.Now;
                var result = await redisHelper.Set(model.Type, model.Key, model.Value, timeSpan, model.RedisDB);
                if (result.Item1)
                {
                    return ResultData.CreateSuccessResult();
                }
                else
                {
                    return ResultData.CreateResult("-1", $"添加失败，失败信息为：{result.Item2}", null);
                }
            }
            catch (Exception ex)
            {
                return ResultData.CreateResult("-1", $"异常信息：{ex.Message}", null);
            }
        }

        /// <summary>
        /// 修改键值对
        /// </summary>
        /// <returns></returns>
        [Route("/Api/EditRedisDevInfo")]
        public async Task<ResultData> EditRedisDevInfo([FromBody]RedisDataModel model)
        {
            try
            {
                if (model.TTLTime <= DateTime.Now)
                    return ResultData.CreateResult("-1", "TTL过期时间不能早于当前日期", null);
                string redisConn = await GetRedisDBAddr(model.RedisAddr);
                RedisHelper redisHelper = new RedisHelper(redisConn);
                var timeSpan = model.TTLTime - DateTime.Now;
                var result = await redisHelper.Set(model.Type, model.Key, model.Value, timeSpan, model.RedisDB);
                if (result.Item1)
                {
                    return ResultData.CreateSuccessResult();
                }
                else
                {
                    return ResultData.CreateResult("-1", $"修改失败，失败信息为：{result.Item2}", null);
                }
            }
            catch (Exception ex)
            {
                return ResultData.CreateResult("-1", $"异常信息：{ex.Message}", null);
            }
        }

        /// <summary>
        /// 返回Redis的连接字符串
        /// </summary>
        /// <param name="redisAddr"></param>
        /// <returns></returns>
        private async Task<string> GetRedisDBAddr(string redisAddr)
        {
            RedisDBSetting redisDBSetting = await _dbContext.Queryable<RedisDBSetting>().WithCacheIF(false).In(redisAddr.ToString()).SingleAsync();
            if (string.IsNullOrEmpty(redisDBSetting.Password))
            {
                return $"{redisDBSetting.ServerIp},abortConnect=false";
            }
            else
            {
                return $"{redisDBSetting.ServerIp},password={redisDBSetting.Password},abortConnect=false";
            }
        }
    }
}