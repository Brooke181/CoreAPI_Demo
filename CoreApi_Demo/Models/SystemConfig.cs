using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreApi_Demo.Models
{
    public class SystemConfig
    {
        public string UploadPath { get; set; }
        public string Domain { get; set; }
    }


    public static class MySettings
    {
        public static SystemConfig Setting { get; set; } = new SystemConfig();
    }


    #region Jwt认证
    
    public static class JwtSetting
    {
        public static JwtConfig Setting { get; set; } = new JwtConfig();
    }

    public class JwtConfig
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int AccessExpiration { get; set; }
        public int RefreshExpiration { get; set; }
    }

    #endregion
}
