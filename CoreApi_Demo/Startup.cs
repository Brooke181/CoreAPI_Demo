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

            configuration.GetSection("SystemConfig").Bind(MySettings.Setting);//�󶨾�̬������
            configuration.GetSection("JwtTokenConfig").Bind(JwtSetting.Setting);//ͬ��

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

            //��ȡ��ʽһ
            var ConnString = Configuration["ConnString"];            
            var MySQLConnection = Configuration.GetSection("ConnectionStrings")["MySQLConnection"];
            var UploadPath = Configuration.GetSection("SystemConfig")["UploadPath"];
            var LogDefault = Configuration.GetSection("Logging").GetSection("LogLevel")["Default"];

            //��ȡ��ʽ��
            //var ConnString2 = Configuration["ConnString"];
            //var MySQLConnection2 = Configuration["ConnectionStrings:MySQLConnection"];
            //var UploadPath2 = Configuration["SystemConfig:UploadPath"];
            //var LogDefault2 = Configuration["Logging:LogLevel:Default"];


            //ģ�Ͳ�����֤
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = (context) =>
                {
                    var error = context.ModelState.FirstOrDefault().Value;
                    var message = error.Errors.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.ErrorMessage))?.ErrorMessage;                  
                    return new JsonResult(new ApiResult { Message = message });                  
                };
            });

            services.AddHttpClient();//HttpClientע��

            services.AddMemoryCache();//�����м��

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();//HttpContextע��

            services.ConfigureCors();//����

            #region JWT��֤ע��            

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
                    Description = "API�ĵ�����",
                    Contact = new OpenApiContact
                    {
                        Email = "5007032@qq.com",
                        Name = "������Ŀ",
                        //Url = new Uri("http://t.abc.com/")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "BROOKE���֤",
                        //Url = new Uri("http://t.abc.com/")
                    }
                });


                // Ϊ Swagger JSON and UI����xml�ĵ�ע��·��
                //var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);//��ȡӦ�ó�������Ŀ¼�����ܹ���Ŀ¼Ӱ�죩
                //var xmlPath = Path.Combine(basePath, "CoreAPI_Demo.xml");
                //c.IncludeXmlComments(xmlPath, true);

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                #region JWT��֤Swagger��Ȩ

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT��Ȩ(���ݽ�������ͷheader�н��д���) ֱ�����¿�������Bearer {token}��ע������֮����һ���ո�",
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

            //ȫ���쳣����
            app.UseCustomExceptionMiddleware();

            app.UseRouting();

            app.UseCors("CorsPolicy");//����

            

            app.UseStatusCodePages(async context =>
            {
                //context.HttpContext.Response.ContentType = "text/plain";  
                context.HttpContext.Response.ContentType = "application/json;charset=utf-8";

                int code = context.HttpContext.Response.StatusCode;
                string message =
                 code switch
                 {
                     401 => "δ��¼",
                     403 => "���ʾܾ�",
                     404 => "δ�ҵ�",
                     _ => "δ֪����",
                 };

                context.HttpContext.Response.StatusCode = StatusCodes.Status200OK;
                await context.HttpContext.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    isSuccess = false,
                    code,
                    message
                }));

            });

            app.UseAuthentication();//jwt��֤

            app.UseAuthorization();

            //����Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); //�м��v1Ҫ������SwaggerDoc��������ֱ���һ��
                c.RoutePrefix = "api";// �����Ϊ�� ����·�����Ǹ�����/index.html������Ϊ�գ���ʾֱ���ڸ��������ʣ��뻻һ��·����ֱ��д���ּ��ɣ�����ֱ��дc.RoutePrefix = "swagger"; �����·��Ϊ ������/swagger/index.html

            });

            app.UseStaticFiles();//���ڷ���wwwroot�µ��ļ� 

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

    //���Tʱ���ʽ
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
