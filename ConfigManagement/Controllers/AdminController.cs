using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ConfigManagement.Common;
using ConfigManagement.Models;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace ConfigManagement.Controllers
{
    [UserLogin]
    public class AdminController : Controller
    {
        private readonly SqlSugarClient _dbContext;
        public AdminController(IDbFactory factory)
        {
            _dbContext = factory.GetDbContext("sqlite");
        }

        [Route("index")]
        public IActionResult Index()
        {
            ViewBag.UserInfo = new UserInfo { Guid = "2222", Password = "0000", UserName = "M" };
            //ViewBag.UserInfo = HttpContext.Session.GetData<UserInfo>("user");
            return View();
        }

        #region Json编辑器
        [Route("JsonEditor")]
        public IActionResult JsonEditor(string guid,bool see)
        {
            ViewBag.Guid = guid;
            ViewBag.See = see;
            return View();
        }

        [HttpGet]
        [Route("/Api/GetConfigJson")]
        public async Task<ResultData> GetConfigJson(string guid)
        {
            try
            {
                ConfigInfo config = _dbContext.Queryable<ConfigInfo>().Where(x => x.Guid == guid).First();
                if (config == null)
                {
                    return ResultData.CreateResult("-1", "本地文件不存在", null);
                }

                string localPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Configs{config.LocalPath}{config.ConfigName}";
                var os = Environment.OSVersion;
                if (os.Platform != PlatformID.Unix && os.Platform != PlatformID.MacOSX)
                {
                    localPath = localPath.Replace("/", "\\");
                }
                else
                {
                    localPath = localPath.Replace("\\", "/");
                }

                if (!System.IO.File.Exists(localPath))
                {
                    return ResultData.CreateResult("-1", "本地文件不存在", null);
                }

                using StreamReader reader = new StreamReader(localPath);
                string text = await reader.ReadToEndAsync();
                Console.WriteLine(text);
                return ResultData.CreateSuccessResult("", text);
            }
            catch (Exception ex)
            {
                return ResultData.CreateResult("-1", $"系统异常 {ex.Message}", null);
            }
        }

        [HttpPost]
        [Route("/Api/UpdateConfigJson")]
        public async Task<ResultData> UpdateConfigJson([FromBody]ConfigJsonInfo info)
        {
            try
            {
                ConfigInfo config = _dbContext.Queryable<ConfigInfo>().Where(x => x.Guid == info.Guid).First();
                if (config == null)
                {
                    return ResultData.CreateResult("-1", "本地文件不存在", null);
                }

                string localPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Configs{config.LocalPath}{config.ConfigName}".ToSystemPath();

                if (!System.IO.File.Exists(localPath))
                {
                    return ResultData.CreateResult("-1", "本地文件不存在", null);
                }

                using (StreamWriter writer = new StreamWriter(localPath,false, Encoding.UTF8))
                {
                    await writer.WriteAsync(info.Json);
                }

                return ResultData.CreateSuccessResult("","");
            }
            catch (Exception ex)
            {
                return ResultData.CreateResult("-1", $"系统异常 {ex.Message}", null);
            }
        }
        #endregion

        #region 用户管理

        /// <summary>
        /// 更新密码页面
        /// </summary>
        /// <returns></returns>
        [Route("UpdateUserPassword")]
        public IActionResult UpdateUserPassword()
        {
            return View();
        }

        /// <summary>
        /// 更新密码
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/Api/UpdateUserPassword")]
        public async Task<ResultData> UpdateUserPassword([FromBody]UpdateUserPasswordRequest request)
        {
            UserInfo userInfo = HttpContext.Session.GetData<UserInfo>("user");
            if (userInfo.Password != ApiUtils.MD5Encode(request.OldPassword, "32"))
            {
                return ResultData.CreateResult("-1", "旧密码不正确", null);
            }
            userInfo.Password = ApiUtils.MD5Encode(request.NewPassword, "32");
            
            if (await _dbContext.Updateable(userInfo).ExecuteCommandAsync() > 0)
            {
                HttpContext.Session.SetData("user", userInfo);
                return ResultData.CreateSuccessResult();
            }
            else
            {
                return ResultData.CreateResult("-1", "更新失败", null);
            }
        }
        #endregion

        #region 主页
        [Route("Main")]
        public IActionResult Main()
        {
            ViewBag.Servers = _dbContext.SqlQueryable<ServerInfo>("select A.*,B.BusinessName from ServerInfo A left join BusinessCategory B on A.BusinessGuid = B.Guid").Take(3).ToList();
            ViewBag.YwNum = _dbContext.Queryable<ConfigInfo>().Where(x => x.CategoryType == "业务中间层").Count();
            ViewBag.BbNum = _dbContext.Queryable<ConfigInfo>().Where(x => x.CategoryType == "报表中间层").Count();
            return View();
        }
        #endregion
    }
}