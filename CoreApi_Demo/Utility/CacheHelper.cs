using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreApi_Demo
{
    public class CacheHelper
    {
        public static IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());

        /// <summary>
        /// 缓存绝对过期时间
        /// </summary>
        ///<param name="key">Cache键</param>
        ///<param name="value">缓存的值</param>
        ///<param name="minute">minute分钟后绝对过期</param>
        public static void SetChache(string key, object value, int minute)
        {
            if (value == null) return;
            _memoryCache.Set(key, value, new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(minute)));
        }

        /// <summary>
        /// 缓存相对过期，最后一次访问后minute分钟后过期
        /// </summary>
        ///<param name="key">Cache键</param>
        ///<param name="value">缓存的值</param>
        ///<param name="minute">滑动过期分钟</param>
        public static void SetChacheSliding(string key, object value, int minute)
        {
            if (value == null) return;
            _memoryCache.Set(key, value, new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(minute)));
        }

        /// <summary>
        ///设置缓存，如果不主动清空，会一直保存在内存中.
        /// </summary>
        ///<param name="key">Cache键值</param>
        ///<param name="value">给Cache[key]赋的值</param>
        public static void SetChache(string key, object value)
        {
            _memoryCache.Set(key, value);
        }

        /// <summary>
        ///清除缓存
        /// </summary>
        ///<param name="key">cache键</param>
        public static void RemoveCache(string key)
        {
            _memoryCache.Remove(key);
        }

        /// <summary>
        ///根据key值，返回Cache[key]的值
        /// </summary>
        ///<param name="key"></param>
        public static object GetCache(string key)
        {
            //return _memoryCache.Get(key);
            if (key != null && _memoryCache.TryGetValue(key, out object val))
            {
                return val;
            }
            else
            {
                return default;
            }
        }

        /// <summary>
        /// 通过Key值返回泛型对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetCache<T>(string key)
        {
            if (key != null && _memoryCache.TryGetValue<T>(key, out T val))
            {
                return val;
            }
            else
            {
                return default;
            }
        }

    }
}