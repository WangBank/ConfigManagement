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
    public class RedisDBSettingController : Controller
    {
        private readonly SqlSugarClient _dbContext;
        public RedisDBSettingController(IDbFactory factory)
        {
            _dbContext = factory.GetDbContext("sqlite");
        }
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 获取数据源列表
        /// </summary>
        /// <returns></returns>
        [Route("/Api/GetRedisDBSettingList")]
        public LayuiData GetRedisDBSettingList(int page, int limit)
        {
            var data = _dbContext.Queryable<RedisDBSetting>().OrderBy(x => x.Code).ToList();
            int rowCount = data.Count;
            data = PageHelper.GetPageData<RedisDBSetting>(data, page, limit);
            return LayuiData.CreateResult(rowCount, data);
        }

        public async Task<IActionResult> AddRedisDBSettingInfo()
        {
            ViewBag.Code = await GetDefaultCode();
            return View();
        }

        public async Task<IActionResult> EditRedisDBSettingInfo(string code)
        {
            RedisDBSetting redisDBSetting = await _dbContext.Queryable<RedisDBSetting>().WithCacheIF(false).In(code.ToString()).SingleAsync();
            return View(redisDBSetting);
        }


        /// <summary>
        /// 新增组织机构
        /// </summary>
        /// <returns></returns>
        [Route("/Api/AddRedisDBSettingInfo")]
        public async Task<ResultData> AddRedisDBSettingInfo([FromBody]RedisDBSetting model)
        {
            try
            {
                int result = await _dbContext.Insertable<RedisDBSetting>(model).ExecuteCommandAsync();
                if (result > 0)
                {
                    return ResultData.CreateSuccessResult();
                }
                else
                {
                    return ResultData.CreateResult("-1", "添加失败", null);
                }
            }
            catch (Exception ex)
            {
                return ResultData.CreateResult("-1", $"异常信息：{ex.Message}", null);
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/Api/DeleteRedisDBSettingInfo")]
        public async Task<ResultData> DeleteRedisDBSettingInfo([FromBody]List<RedisDBSetting> models)
        {
            try
            {
                bool result = await _dbContext.Deleteable(models).ExecuteCommandHasChangeAsync();
                if (result)
                {
                    return ResultData.CreateSuccessResult();
                }
                else
                {
                    return ResultData.CreateResult("-1", "删除失败", null);
                }
            }
            catch (Exception ex)
            {
                return ResultData.CreateResult("-1", $"异常信息：{ex.Message}", null);
            }
        }

        [HttpPost]
        [Route("/Api/EditRedisDBSettingInfo")]
        public async Task<ResultData> EditRedisDBSettingInfo([FromBody]RedisDBSetting model)
        {
            try
            {
                bool result = await _dbContext.Updateable(model).ExecuteCommandHasChangeAsync();
                if (result)
                {
                    return ResultData.CreateSuccessResult();
                }
                else
                {
                    return ResultData.CreateResult("-1", "修改失败", null);
                }
            }
            catch (Exception ex)
            {
                return ResultData.CreateResult("-1", $"异常信息：{ex.Message}", null);
            }
        }

        private async Task<string> GetDefaultCode()
        {
            var code = "";
            List<RedisDBSetting> list = (await _dbContext.Queryable<RedisDBSetting>().ToListAsync()).OrderByDescending(x => x.Code).ToList();
            if (list.Count > 0)
            {
                var model = list.First();
                code = (Convert.ToInt32(model.Code) + 1).ToString().PadLeft(3, '0');
            }
            else
            {
                code = "001";
            }
            return code;
        }
    }
}