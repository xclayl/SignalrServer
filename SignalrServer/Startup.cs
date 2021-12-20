using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SignalrServer.Hubs;
using SignalrServer.Lib;
using SignalrServer.Models;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace SignalrServer
{
    public class Startup
    {
        public const string RestCors = "RestCors";
        public const string HubsCors = "HubsCors";

        private Config _config;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            var config = Configuration.Get<Config>();
            AddDefaultConfig(config);
            ValidateConfig(config);

            _config = config;
            services.AddSingleton(config);

            services.AddSignalR();

            services.AddCors(options =>
            {
                options.AddPolicy(name: RestCors,
                    builder =>
                    {
                        if (!string.IsNullOrWhiteSpace(_config.Rest_Api_Cors_Origins))
                            builder.WithOrigins(_config.Rest_Api_Cors_Origins.Split(',').Select(ToOrigin).ToArray());
                        builder.AllowAnyMethod();
                        builder.AllowCredentials();
                        builder.AllowAnyHeader();
                    });
                options.AddPolicy(name: HubsCors,
                    builder =>
                    {
                        if (!string.IsNullOrWhiteSpace(_config.Hubs_Cors_Origins))
                            builder.WithOrigins(_config.Hubs_Cors_Origins.Split(',').Select(ToOrigin).ToArray());
                        builder.AllowAnyMethod();
                        builder.AllowCredentials();
                        builder.AllowAnyHeader();
                    });
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SignalrServer", Version = "v1" });
            });
        }


        private static string ToOrigin(string url)
        {
            var uri = new Uri(url);
            return uri.GetLeftPart(UriPartial.Authority);
        }

        private void ValidateConfig(Config config)
        {
            if (string.IsNullOrWhiteSpace(config.Token_Generator_Shared_Secret))
                throw new Exception("Token_Generator_Shared_Secret is empty");
            if (string.IsNullOrWhiteSpace(config.Token_Symmetric_Key_Base64))
                throw new Exception("Token_Symmetric_Key_Base64 is empty");

            try
            {
                var bytes = Convert.FromBase64String(config.Token_Symmetric_Key_Base64);
                if (bytes.Length != 64)
                    throw new Exception("Token_Symmetric_Key_Base64 must be 64 bytes");

            }
            catch
            {
                Console.WriteLine("Error with Token_Symmetric_Key_Base64");
                throw;
            }
        }

        private void AddDefaultConfig(Config config)
        {
            if (string.IsNullOrWhiteSpace(config.Token_Generator_Shared_Secret))
            {
                config.Token_Generator_Shared_Secret = GenerateRandomBase64(21);

                Console.WriteLine($"'Token_Generator_Shared_Secret' environment variable vas not found, so one was generated for you: {config.Token_Generator_Shared_Secret}");
            }
            if (string.IsNullOrWhiteSpace(config.Token_Symmetric_Key_Base64))
            {
                config.Token_Symmetric_Key_Base64 = GenerateRandomBase64(64);

                Console.WriteLine($"'Token_Symmetric_Key_Base64' environment variable was not found, so one was generated for you: {config.Token_Symmetric_Key_Base64}");
            }
        }

        private string GenerateRandomBase64(int bytes)
        {
            var rnd = new byte[bytes];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(rnd);
            return Convert.ToBase64String(rnd);
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SignalrServer v1"));
            app.Use(async (context, next) => await AuthMiddleware.Use(context, next, _config));



            // app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    context.Response.Redirect("/swagger");
                });
                endpoints.MapHub<DefaultHub>("/hubs/default");
                endpoints.MapControllers();
            });
        }
    }
}
