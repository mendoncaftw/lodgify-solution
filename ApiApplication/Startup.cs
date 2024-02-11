using System;
using System.Net.Http;
using System.Threading.Tasks;
using ApiApplication.Clients;
using ApiApplication.Clients.Abstractions;
using ApiApplication.Database;
using ApiApplication.Database.Repositories;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Middleware;
using ApiApplication.Services;
using ApiApplication.Services.Abstractions;
using Grpc.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using static ProtoDefinitions.MoviesApi;

namespace ApiApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
            });

            var appSettings = Configuration.Get<AppSettings>();

            services.Configure<AppSettings>(Configuration);

            services.AddTransient<ICacheClient, CacheClient>();
            services.AddTransient<IShowtimesService, ShowtimesService>();
            services.AddTransient<ISeatsService, SeatsService>();

            services.AddTransient<IShowtimesRepository, ShowtimesRepository>();
            services.AddTransient<ITicketsRepository, TicketsRepository>();
            services.AddTransient<IAuditoriumsRepository, AuditoriumsRepository>();

            services.AddDbContext<CinemaContext>(options =>
            {
                options.UseInMemoryDatabase("CinemaDb")
                    .EnableSensitiveDataLogging()
                    .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            });

            services.AddControllers();

            services.AddHttpClient();

            services
                .AddGrpcClient<MoviesApiClient>(options =>
                {
                    options.Address = new Uri(appSettings.MoviesApi.BaseAddress);
                })
                .ConfigureChannel(options =>
                {
                    options.HttpHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                    // In more recent versions of .NET Core there are better ways to do this
                    options.Credentials = ChannelCredentials.Create(
                        new SslCredentials(),
                        CallCredentials.FromInterceptor((auth, meta) =>
                        {
                            meta.Add(appSettings.MoviesApi.ApiHeaderName, appSettings.MoviesApi.ApiKey);
                            return Task.CompletedTask;
                        }));
                });

            if (appSettings.MoviesApi.Cache.Enabled)
            {
                services.AddTransient<IApiClientGrpc, CachedApiClientGrpc>();
            }
            else
            {
                services.AddTransient<IApiClientGrpc, ApiClientGrpc>();
            }

            services.AddStackExchangeRedisCache(o =>
            {
                o.Configuration = Configuration.GetConnectionString("redis");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<ExecutionTrackingMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            SampleData.Initialize(app);
        }


    }
}