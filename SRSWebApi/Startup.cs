using System;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SrsWebApi;

namespace SRSWebApi
{
    /// <summary>
    /// webapi配置启动类
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// webapi配置启动类构造
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            /*ThreadPool.GetMinThreads(out var workThreads, out var completionPortThreads);
            Console.WriteLine(new StringBuilder()
                .Append($"ThreadPool.ThreadCount: {ThreadPool.ThreadCount}, ")
                .Append($"Minimum work threads: {workThreads}, ")
                .Append($"Minimum completion port threads: {completionPortThreads})").ToString());
            int maxT; //最大工作线程数
            int maxIO; //最大IO工作线程数
            ThreadPool.GetMaxThreads(out maxT, out maxIO);
            string thMin = string.Format("默认的 最大工作线程数 {0},最大IO工作线程数{1}", maxT, maxIO);
            Console.WriteLine(thMin);
            ThreadPool.SetMinThreads(200, 200); // MinThreads 值不要超过 (max_thread /2  )，否则会不生效。要不然就同时加大设置max_thread


            ThreadPool.GetMinThreads(out workThreads, out completionPortThreads);
            Console.WriteLine(new StringBuilder()
                .Append($"ThreadPool.ThreadCount: {ThreadPool.ThreadCount}, ")
                .Append($"Minimum work threads: {workThreads}, ")
                .Append($"Minimum completion port threads: {completionPortThreads})").ToString());*/
        }

        /// <summary>
        /// 配置类
        /// </summary>
        public IConfiguration Configuration { get; }


        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            // 注册Swagger服务
            services.AddSwaggerGen(c =>
            {
                // 添加文档信息
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "SRSWebApi", Version = "v1"});
                //c.IncludeXmlComments(Path.Combine(Program.common.WorkPath, "Edu.Model.xml"));//这里增加model注释，返回值会增加注释：需要Edu.Model项目属性，生成中输出xml文件
                c.IncludeXmlComments(Path.Combine(Program.CommonFunctions.WorkPath, "Edu.Swagger.xml"));
            });
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddControllers().AddJsonOptions(
                options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter())
            ).AddJsonOptions(configure =>
            {
                configure.JsonSerializerOptions.Converters.Add(new DatetimeJsonConverter());
            });
        }

        /// <summary>
        ///  This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            // 启用Swagger中间件
            app.UseSwagger();

            // 配置SwaggerUI
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "SRSWebApi"); });


            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            
            if (!Directory.Exists(SrsManageCommon.Common.WorkPath + "CutMergeFile"))
            {
                Directory.CreateDirectory(SrsManageCommon.Common.WorkPath + "CutMergeFile");
            }
            
         

            var staticfile = new StaticFileOptions();
            staticfile.FileProvider = new PhysicalFileProvider(SrsManageCommon.Common.WorkPath+"CutMergeFile");//指定静态文件服务器
            //手动设置MIME Type,或者设置一个默认值， 以解决某些文件MIME Type文件识别不到，出现404错误
            staticfile.ServeUnknownFileTypes = true;
            staticfile.DefaultContentType = "application/octet-stream";//设置默认MIME Type
            staticfile.OnPrepareResponse = (c) =>
            {
                c.Context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            };
            app.UseStaticFiles(staticfile);
            app.Use(next => context =>
            {
                context.Request.EnableBuffering();
                return next(context);
            });
        }
    }
}