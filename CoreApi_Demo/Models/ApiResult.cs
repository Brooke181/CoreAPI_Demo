using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreApi_Demo.Models
{
    public class ApiResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public string Code { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }

}
