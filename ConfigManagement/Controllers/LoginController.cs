using System.Collections.Generic;
using ConfigManagement.Common;
using ConfigManagement.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace ConfigManagement.Controllers
{
    public class LoginController : Controller
    {
        private readonly SqlSugarClient _dbContext;
        public LoginController(IDbFactory factory)
        {
            _dbContext = factory.GetDbContext("sqlite");
        }

        /// <summary>
        /// 登录页面
        /// </summary>
        /// <returns></returns>
        [Route("")]
        [Route("Login")]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// 登录接口
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("/Api/Login")]
        public ResultData Login([FromBody]UserInfo data)
        {
            if (data == null)
            {
                return ResultData.CreateResult("-1", "用户名或密码不正确", null);
            }

            KeyValuePair<bool, string> keyValue = ApiUtils.CheckFuncParams(data, "UserName", "Password");
            if (!keyValue.Key)
            {
                return ResultData.CreateResult("-1", $"验证失败，字段{keyValue.Value}不能为空", null);
            }

            string pa = ApiUtils.MD5Encode(data.Password, "32");
            UserInfo userInfo = _dbContext.Queryable<UserInfo>().Where(x => x.UserName.ToLower() == data.UserName.ToLower() && x.Password == ApiUtils.MD5Encode(data.Password, "32")).Single();
            if (userInfo != null)
            {
                HttpContext.Session.SetData("user", userInfo);
                return ResultData.CreateSuccessResult();
            }
            else
            {
                return ResultData.CreateResult("-1", "用户名或密码不正确", null);
            }
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("/Logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.SetData("user", null);
            return new RedirectResult("/Login");
        }

    }
}