using ConfigManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;

namespace ConfigManagement.Common
{

    public class GlobalExceptionFilter : IExceptionFilter
    {

        /// <summary>
        /// 发生异常时进入
        /// </summary>
        /// <param name="context"></param>
        public void OnException(ExceptionContext context)
        {
            if (context.ExceptionHandled == false)
            {
                string url = context.HttpContext.Request.Path.Value;
                if (url.ToLower().Contains("/api/"))
                {
                    context.Result = new JsonResult(ResultData.CreateResult("-1", "系统异常", null));
                }
                else
                {
                    context.Result = new RedirectResult("/page/500.html");
                }
               
                //context.Result = new ContentResult
                //{
                //    Content = context.Exception.Message,//这里是把异常抛出。也可以不抛出。
                //    StatusCode = StatusCodes.Status200OK,
                //    ContentType = "text/html;charset=utf-8"
                //};
            }
            context.ExceptionHandled = true;
        }

        /// <summary>
        /// 异步发生异常时进入
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Task OnExceptionAsync(ExceptionContext context)
        {
            OnException(context);
            return Task.CompletedTask;
        }

    }
}
