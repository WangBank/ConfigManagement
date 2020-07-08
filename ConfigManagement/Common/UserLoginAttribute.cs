using ConfigManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace ConfigManagement.Common
{
    /// <summary>
    /// 验证用户登录状态
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class UserLoginAttribute : ActionFilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            UserInfo info = new UserInfo { Guid = "2222", Password = "0000", UserName = "M" };
            // UserInfo info = context.HttpContext.Session.GetData<UserInfo>("user");
            if (info == null)
            {
                string url = context.HttpContext.Request.Path.Value;
                if (url.ToLower().Contains("/api/"))
                {
                    context.Result = new JsonResult(ResultData.CreateResult("-2", "用户未登录", null));
                }
                else
                {
                    context.Result = new RedirectResult("/Login");
                }
            }
        }
    }
}
