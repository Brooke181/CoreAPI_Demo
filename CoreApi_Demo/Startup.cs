using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CoreApi_Demo.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace CoreApi_Demo
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            //Configuration = configuration;

            var builder = new ConfigurationBuilder()
              .SetBasePath(env.ContentRootPath)
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            Configuration = builder.Build();            

            configuration.GetSection("SystemConfig").Bind(MySettings.Setting);//绑定静态配置类
            configuration.GetSection("JwtTokenConfig").Bind(JwtSetting.Setting);//同上

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(configure =>
                {
                    configure.JsonSerializerOptions.Converters.Add(new DatetimeJsonConverter());
                });

            services.Configure<SystemConfig>(Configuration.GetSection("SystemConfig"));

            //读取方式一
            var ConnString = Configuration["ConnString"];            
            var MySQLConnection = Configuration.GetSection("ConnectionStrings")["MySQLConnection"];
            var UploadPath = Configuration.GetSection("SystemConfig")["UploadPath"];
            var LogDefault = Configuration.GetSection("Logging").GetSection("LogLevel")["Default"];

            //读取方式二
            //var ConnString2 = Configuration["ConnString"];
            //var MySQLConnection2 = Configuration["ConnectionStrings:MySQLConnection"];
            //var UploadPath2 = Configuration["SystemConfig:UploadPath"];
            //var LogDefault2 = Configuration["Logging:LogLevel:Default"];


            //模型参数验证
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = (context) =>
                {
                    var error = context.ModelState.FirstOrDefault().Value;
                    var message = error.Errors.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.ErrorMessage))?.ErrorMessage;                  
                    return new JsonResult(new ApiResult { Message = message });                  
                };
            });

            services.AddHttpClient();//HttpClient注入

            services.AddMemoryCache();//缓存中间件

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();//HttpContext注入

            services.ConfigureCors();//跨域

            #region JWT认证注入            

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.RequireHttpsMetadata = false;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = JwtSetting.Setting.Issuer,
                        ValidAudience = JwtSetting.Setting.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(JwtSetting.Setting.Secret))
                    };
                });

            #endregion


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "My API",
                    Version = "v1",
                    Description = "API文档描述",
                    Contact = new OpenApiContact
                    {
                        Email = "5007032@qq.com",
                        Name = "测试项目",
                        //Url = new Uri("http://t.abc.com/")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "BROOKE许可证",
                        //Url = new Uri("http://t.abc.com/")
                    }
                });


                // 为 Swagger JSON and UI设置xml文档注释路径
                //var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);//获取应用程序所在目录（不受工作目录影响）
                //var xmlPath = Path.Combine(basePath, "CoreAPI_Demo.xml");
                //c.IncludeXmlComments(xmlPath, true);

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                #region JWT认证Swagger授权

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT授权(数据将在请求头header中进行传输) 直接在下框中输入Bearer {token}（注意两者之间是一个空格）",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });

                #endregion

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //全局异常处理
            app.UseCustomExceptionMiddleware();

            app.UseRouting();

            app.UseCors("CorsPolicy");//跨域

            

            app.UseStatusCodePages(async context =>
            {
                //context.HttpContext.Response.ContentType = "text/plain";  
                context.HttpContext.Response.ContentType = "application/json;charset=utf-8";

                int code = context.HttpContext.Response.StatusCode;
                string message =
                 code switch
                 {
                     401 => "未登录",
                     403 => "访问拒绝",
                     404 => "未找到",
                     _ => "未知错误",
                 };

                context.HttpContext.Response.StatusCode = StatusCodes.Status200OK;
                await context.HttpContext.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    isSuccess = false,
                    code,
                    message
                }));

            });

            app.UseAuthentication();//jwt认证

            app.UseAuthorization();

            //配置Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); //中间段v1要和上面SwaggerDoc定义的名字保持一致
                c.RoutePrefix = "api";// 如果设为空 访问路径就是根域名/index.html，设置为空，表示直接在根域名访问；想换一个路径，直接写名字即可，比如直接写c.RoutePrefix = "swagger"; 则访问路径为 根域名/swagger/index.html

            });

            app.UseStaticFiles();//用于访问wwwroot下的文件 

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello ,WelCome");
                    //context.Response.Redirect("http://t1.abc.com");
                });

            });
        }
    }

    //解决T时间格式
    public class DatetimeJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                if (DateTime.TryParse(reader.GetString(), out DateTime date))
                    return date;
            }
            return reader.GetDateTime();
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}
