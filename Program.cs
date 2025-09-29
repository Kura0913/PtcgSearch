using System.Threading.RateLimiting;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PtcgSearch.Models;
using PtcgSearch.Services;
namespace PtcgSearch
{
    public class Program
    {
        /// <summary>
        /// 主要入口方法
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddHttpClient();
            // DB Connection
            builder.Services.AddDbContext<PtcgCardContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("PokemonCardDB")));
            // Health Check
            builder.Services.AddHealthChecks();
            
            AddPtcgSearchService(builder);
            AddSwagger(builder);
            AddCors(builder);
            AddRateLimit(builder);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapHealthChecks("/health");
            // HTTPS 重導向
            app.UseHttpsRedirection();
            // 靜態檔案服務
            app.UseStaticFiles();
            // 路由匹配
            app.UseRouting();
            // CORS
            app.UseCors("AllowAllOrigins");
            // 驗證
            app.UseAuthentication();
            // 授權
            app.UseAuthorization();
            // 啟用Controller路由
            app.MapControllers();
            app.Run();
        }

        /// <summary>
        /// 新增swagger服務
        /// </summary>
        /// <param name="builder"></param>
        private static void AddSwagger(WebApplicationBuilder builder)
        {
            var env = builder.Environment;

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                if (env.IsDevelopment())
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ptcg Search API", Version = "v1" });
                    c.AddServer(new OpenApiServer
                    {
                        Url = "/",
                        Description = "API Server"
                    });
                    c.SchemaFilter<SwaggerEnumSchemaFilter>();
                }
            });
        }

        /// <summary>
        /// 新增CORS服務
        /// </summary>
        /// <param name="builder"></param>
        private static void AddCors(WebApplicationBuilder builder)
        {
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", policy =>
                {
                    policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
                });

                options.AddPolicy("AllowSpecificOrigin", policy =>
                {
                    policy.WithOrigins("http://localhost:8080",
                            "https://localhost:8083",
                            "http://127.0.0.1:8080",
                            "file://")
                        .AllowAnyMethod().AllowAnyHeader().AllowCredentials();
                });
            });
        }

        /// <summary>
        /// 新增RateLimit規則
        /// </summary>
        /// <param name="builder"></param>
        private static void AddRateLimit(WebApplicationBuilder builder)
        {
            builder.Services.AddRateLimiter(options =>
            {
                // Fixed window limitation
                options.AddFixedWindowLimiter("FixedPolicy", configure =>
                {
                    configure.PermitLimit = 10;
                    configure.Window = TimeSpan.FromMinutes(1);
                    configure.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    configure.QueueLimit = 2;
                });
                // Sliding window limitation
                options.AddSlidingWindowLimiter("SlidingPolicy", configure =>
                {
                    configure.PermitLimit = 5;
                    configure.Window = TimeSpan.FromMinutes(30);
                    configure.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    configure.QueueLimit = 3;
                });
                // IP limitation
                options.AddPolicy("PerIPPolicy", context =>
                {
                    var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetFixedWindowLimiter(ipAddress, _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1)
                        });
                });
                // Critical operation policy
                options.AddFixedWindowLimiter("CriticalOperationPolicy", configure =>
                {
                    configure.PermitLimit = 2;  // allow 2 requests per minute
                    configure.Window = TimeSpan.FromMinutes(1);
                    configure.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    configure.QueueLimit = 0;  // no queuing allowed
                });
                // IP limitation
                options.AddPolicy("PerIPPolicy", context =>
                {
                    var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    return RateLimitPartition.GetFixedWindowLimiter(ipAddress, _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 10,
                            Window = TimeSpan.FromMinutes(1)
                        });
                });
            });
        }
        /// <summary>
        /// 註冊PTCG Search相關服務
        /// </summary>
        /// <param name="builder"></param>
        private static void AddPtcgSearchService(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<LoadOfficialCardInfoService>();
        }
    }
}


