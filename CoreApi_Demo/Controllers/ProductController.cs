using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreApi_Demo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace CoreApi_Demo.Controllers
{
    //[ApiExplorerSettings(IgnoreApi = true)]
    [Produces("application/json")]  
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private IMemoryCache _cache;

        public ProductController(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        [HttpGet]
        public DateTime GetTime()
        {
            string key = "_timeKey";          

            // Look for cache key.
            if (!_cache.TryGetValue(key, out DateTime cacheEntry))
            {
                // Key not in cache, so get data.
                cacheEntry = DateTime.Now;

                // Set cache options.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    // Keep in cache for this time, reset time if accessed.
                    .SetSlidingExpiration(TimeSpan.FromSeconds(3));

                // Save data in cache.
                _cache.Set(key, cacheEntry, cacheEntryOptions);
            }

            return cacheEntry;
        }

        /// <summary>
        /// 获取所有产品
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ApiResult Get()
        {
            var result = new ApiResult();

            var rd = new Random();

            result.Data["dataList"] = Enumerable.Range(1, 5).Select(index => new 
            {
                Name = $"商品-{index}",
                Price = rd.Next(100, 9999)
            });           

            result.IsSuccess = true;
            return result;
        }
    }
}
