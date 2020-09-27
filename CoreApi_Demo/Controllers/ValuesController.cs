using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreApi_Demo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CoreApi_Demo.Controllers
{    
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private IConfiguration _configuration;//方式一：通过注入IConfiguration推向读取配置
        private readonly SystemConfig _sysConfig;//方式二：通过自定义对象读取配置

        public ValuesController(IConfiguration configuration, IOptions<SystemConfig> sysConfig)
        {
            _configuration = configuration;
            _sysConfig = sysConfig.Value;
        }

        // GET: api/<ValuesController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            var ConnString = _configuration["ConnString"];
            var MySQLConnection = _configuration.GetSection("ConnectionStrings")["MySQLConnection"];
            var UploadPath = _configuration.GetSection("SystemConfig")["UploadPath"];
            var LogDefault = _configuration.GetSection("Logging").GetSection("LogLevel")["Default"];
            return new string[] { "value1", "value2" };
        }

        [HttpGet]
        public IEnumerable<string> GetSetting()
        {
            var UploadPath = _sysConfig.UploadPath;
            var Domain = _sysConfig.Domain;

            var UploadPath2 = MySettings.Setting.UploadPath;//方式三：通过绑定静态类读取

            return new string[] { "value1", "value2" };
        }

        // GET api/<ValuesController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<ValuesController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ValuesController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ValuesController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
