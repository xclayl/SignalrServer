using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SignalRServer.Hubs;
using SignalRServer.Lib;
using SignalRServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SignalRServer
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

            var config = Configuration.Get<EnvVarConfig>();
            AddDefaultConfig(config);
            _config = ValidateConfig(config);

            services.AddSingleton(_config);

            services.AddSignalR();

            services.AddCors(options =>
            {
                options.AddPolicy(name: RestCors,
                    builder =>
                    {
                        if (_config.RestApiCorsOrigins.Any())
                            builder.WithOrigins(_config.RestApiCorsOrigins.ToArray());
                        builder.AllowAnyMethod();
                        builder.AllowCredentials();
                        builder.AllowAnyHeader();
                    });
                options.AddPolicy(name: HubsCors,
                    builder =>
                    {
                        if (_config.HubsCorsOrigins.Any())
                            builder.WithOrigins(_config.HubsCorsOrigins.ToArray());
                        builder.AllowAnyMethod();
                        builder.AllowCredentials();
                        builder.AllowAnyHeader();
                    });
            });

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SignalRServer", Version = "v1" });
            });
        }


        private static string ToOrigin(string url)
        {
            var uri = new Uri(url);
            return uri.GetLeftPart(UriPartial.Authority);
        }

        private Config ValidateConfig(EnvVarConfig config)
        {

            try
            {

                var hubsCorsOrigins = !string.IsNullOrWhiteSpace(config.Hubs_Cors_Origins)
                    ? config.Hubs_Cors_Origins
                        .Split(',')
                        .Select(ToOrigin)
                        .ToList()
                    : new List<string>();

                var restApiCorsOrigins = !string.IsNullOrWhiteSpace(config.Rest_Api_Cors_Origins)
                    ? config.Rest_Api_Cors_Origins
                        .Split(',')
                        .Select(ToOrigin)
                        .ToList()
                    : new List<string>();

                if ("true".Equals(config.Allow_Anonymous, StringComparison.InvariantCultureIgnoreCase))
                {
                    return new Config
                    {
                        AllowAnonymous = true,
                        RestApiCorsOrigins = restApiCorsOrigins,
                        HubsCorsOrigins = hubsCorsOrigins,
                    };
                }
                else
                {
                    var bytes = Convert.FromBase64String(config.Token_Symmetric_Key_Base64);
                    if (bytes.Length != 64)
                        throw new Exception("Token_Symmetric_Key_Base64 must be 64 bytes");


                    return new Config
                    {
                        AllowAnonymous = false,
                        RestApiCorsOrigins = restApiCorsOrigins,
                        HubsCorsOrigins = hubsCorsOrigins,
                        TokenGeneratorSharedSecret = config.Token_Generator_Shared_Secret,
                        TokenSymmetricKey = bytes,
                    };
                }
            }
            catch
            {
                Console.WriteLine("Error with Token_Symmetric_Key_Base64");
                throw;
            }

            // config.Hubs_Cors_Origins.Split(',').Select(ToOrigin)
        }

        private void AddDefaultConfig(EnvVarConfig config)
        {
            var sb = new StringBuilder();
            var suggestAnonymous = false;
            var origConfig = config.Clone();
            var allowAnonymous = "true".Equals(config.Allow_Anonymous, StringComparison.InvariantCultureIgnoreCase);

            if (!allowAnonymous)
            {
                if (string.IsNullOrWhiteSpace(config.Token_Generator_Shared_Secret))
                {
                    config.Token_Generator_Shared_Secret = GenerateRandomBase64(21);

                    sb.AppendLine($"Token_Generator_Shared_Secret={config.Token_Generator_Shared_Secret}");
                    suggestAnonymous = true;
                }
                if (string.IsNullOrWhiteSpace(config.Token_Symmetric_Key_Base64))
                {
                    config.Token_Symmetric_Key_Base64 = GenerateRandomBase64(64);
                    sb.AppendLine($"Token_Symmetric_Key_Base64={config.Token_Symmetric_Key_Base64}");
                    suggestAnonymous = true;
                }
            }

            if (allowAnonymous)
            {
                if (!string.IsNullOrWhiteSpace(origConfig.Token_Generator_Shared_Secret))
                    sb.AppendLine($"Token_Generator_Shared_Secret=");
                if (!string.IsNullOrWhiteSpace(origConfig.Token_Symmetric_Key_Base64))
                    sb.AppendLine($"Token_Symmetric_Key_Base64=");
            }

            if (sb.Length > 0)
            {
                Console.WriteLine("The following config settings were changed for you.  You may want to set them in environment variables:");
                Console.WriteLine();
                Console.WriteLine(sb);
                Console.WriteLine();
            }

            if (suggestAnonymous)
            {
                Console.WriteLine("You may want to turn off JWT authentication with the following environment variable:");
                Console.WriteLine();
                Console.WriteLine("AllowAnonymous=TRUE");
                Console.WriteLine();

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
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SignalRServer v1"));
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
