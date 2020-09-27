using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreApi_Demo
{
    public class CustomExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CustomExceptionMiddleware> _logger;      

        public CustomExceptionMiddleware(RequestDelegate next, ILogger<CustomExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;            

        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Unhandled exception....");
                await HandleExceptionAsync(httpContext, ex);
            }         
        }

        private Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
        {
            var result = JsonConvert.SerializeObject(new { result = 0, message = ex.Message });
            httpContext.Response.ContentType = "application/json;charset=utf-8";
            return httpContext.Response.WriteAsync(result);
        }
    }

    /// <summary>
    /// 以扩展方式添加中间件
    /// </summary>
    public static class CustomExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomExceptionMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomExceptionMiddleware>();
        }
    }
}
