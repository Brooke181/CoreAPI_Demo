using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreApi_Demo.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreApi_Demo.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;

        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        public ApiResult UploadFile(List<IFormFile> files)
        {
            ApiResult result = new ApiResult();

            //注：参数files对象也可以通过换成： var files = Request.Form.Files;来获取

            if (files.Count <= 0)
            {
                result.Message = "上传文件不能为空";
                return result;
            }

            #region 上传          

            List<string> filenames = new List<string>();

            var webRootPath = _env.WebRootPath;
            var rootFolder = MySettings.Setting.UploadPath;           

            var physicalPath = $"{webRootPath}/{rootFolder}/";

            if (!Directory.Exists(physicalPath))
            {
                Directory.CreateDirectory(physicalPath);
            }

            foreach (var file in files)
            {
                var fileExtension = Path.GetExtension(file.FileName);//获取文件格式，拓展名               

                var saveName = $"{rootFolder}/{Path.GetRandomFileName()}{fileExtension}";
                filenames.Add(saveName);//相对路径

                var fileName = webRootPath + saveName;

                using FileStream fs = System.IO.File.Create(fileName);
                file.CopyTo(fs);
                fs.Flush();

            }          
            #endregion


            result.IsSuccess = true;
            result.Data["files"] = filenames;

            return result;
        }
    }
}
