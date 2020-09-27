using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoreApi_Demo.Models
{
    /// <summary>
    /// 注册实体
    /// </summary>
    public class RegisterEntity
    {
        /// <summary>
        /// 手机号
        /// </summary>
        [Display(Name = "手机号")]
        [Required(ErrorMessage = "{0}不能为空")]
        [StringLength(11, ErrorMessage = "{0}最多{1}个字符")]
        public string Mobile { get; set; }

        /// <summary>
        /// 验证码
        /// </summary>
        [Display(Name = "验证码")]
        [Required(ErrorMessage = "{0}不能为空")]
        [StringLength(6, ErrorMessage = "{0}最多{1}个字符")]
        public string Code { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Display(Name = "密码")]
        [Required(ErrorMessage = "{0}不能为空")]
        [StringLength(16, ErrorMessage = "{0}最多{1}个字符")]
        public string Pwd { get; set; }
    }

    /// <summary>
    /// 登录
    /// </summary>
    public class LoginEntity
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [Display(Name = "用户名")]
        [Required(ErrorMessage = "{0}不能为空")]
        [StringLength(50, ErrorMessage = "{0}最多{1}个字符")]
        public string User { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Display(Name = "密码")]
        [Required(ErrorMessage = "{0}不能为空")]
        [StringLength(30, ErrorMessage = "{0}最多{1}个字符")]
        public string Pwd { get; set; }

    }


}
