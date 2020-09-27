using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CoreApi_Demo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CoreApi_Demo.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// 用户登录（密码登录）
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApiResult> Login(LoginEntity model)       
        {          

            ApiResult result = new ApiResult();

            //验证用户名和密码
            //var userInfo = await _memberService.CheckUserAndPwd(model.User, model.Pwd);
            //if (userInfo == null)
            //{
            //    result.Message = "用户名或密码不正确";
            //    return result;
            //}
            var memberId = "10001";

            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Name,model.User),
                new Claim(ClaimTypes.Role,"user"),
                new Claim(JwtRegisteredClaimNames.Sub,memberId),

            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(JwtSetting.Setting.Secret));
            var expires = DateTime.Now.AddDays(1);
            var token = new JwtSecurityToken(
                        issuer: JwtSetting.Setting.Issuer,
                        audience: JwtSetting.Setting.Audience,
                        claims: claims,
                        notBefore: DateTime.Now,
                        expires: expires,
                        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));


            //生成Token
            string jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            //更新最后登录时间
            //await _memberService.UpdateLastLoginTime(userInfo.MemberID);

            result.IsSuccess = true;        
            result.Data["token"] = jwtToken;
            result.Message = "授权成功！";
            return result;

        }
    }
}
