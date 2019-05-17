using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Flare.Backend.Models;
using Microsoft.AspNetCore.HttpOverrides;
using Flare.Backend.Utils;
using Tamturk.AspNetCore.RequestServiceExtender;
using Newtonsoft.Json;
using System.Text;
using Flare.Backend.Controllers;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore;
using System.Xml;
using Microsoft.Extensions.FileProviders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Flare.Backend.Services;
using Tamturk.AspNetCore;

namespace Flare.Backend {
    public class Startup {
        private readonly IConfiguration _config;

        private SecurityKey _key;

        readonly InMemoryCache<Token> token_cache = new InMemoryCache<Token>();
        readonly InMemoryCache<Token> personal_token_cache = new InMemoryCache<Token>();

        public Startup(IConfiguration config) {
            _config = config;
        }

        private static RSAParameters LoadKey(string xmlString) {
            RSAParameters parameters = new RSAParameters();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlString);

            if (xmlDoc.DocumentElement.Name.Equals("RSAKeyValue"))
            {
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "Modulus": parameters.Modulus = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "Exponent": parameters.Exponent = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "P": parameters.P = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "Q": parameters.Q = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "DP": parameters.DP = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "DQ": parameters.DQ = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "InverseQ": parameters.InverseQ = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                        case "D": parameters.D = (string.IsNullOrEmpty(node.InnerText) ? null : Convert.FromBase64String(node.InnerText)); break;
                    }
                }
            }
            else
            {
                throw new Exception("Invalid XML RSA key.");
            }

            return parameters;
        }

        public void ConfigureServices(IServiceCollection services) {
            _key = new RsaSecurityKey(LoadKey(_config["jwt"]));
            services.AddSingleton(_key);
            services.AddSingleton(new SigningCredentials(_key, SecurityAlgorithms.RsaSha256Signature));

            var databaseConnectionString = _config["database"];

            string  smtp_host = _config["smtp:server"],
                    smtp_port = _config["smtp:port"];

            if (!Int32.TryParse(smtp_port, out int _smtp_port)) {
                throw new Exception("SMTP server port is invalid");
            }
            
            var smtp_credientals = new NetworkCredential(_config["smtp:user"], _config["smtp:pass"]);

            services
                .AddScoped(a => new SmtpClient(smtp_host, _smtp_port) {
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = smtp_credientals,
                    EnableSsl = false
                })
                .AddScoped<FileService>()
                .AddSingleton(_config)
                .AddRequestSigning(_config["hash"])
                .AddDbContext<ApplicationDbContext>(options => options.UseMySql(databaseConnectionString,
                    mysqlOptions => {
                        mysqlOptions.ServerVersion(new Version(8, 0, 12), ServerType.MySql);
                    }
                ))
                .Configure<ForwardedHeadersOptions>(opts => opts.ForwardedHeaders = ForwardedHeaders.XForwardedProto)
                .AddCors()
                .AddMvcCore()
                .AddJsonFormatters()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope()) {
                scope.ServiceProvider.GetService<ApplicationDbContext>().Database.Migrate();
            }
            
            app
                .UseForwardedHeaders(new ForwardedHeadersOptions {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                    KnownNetworks = { new IPNetwork(new IPAddress(new byte[] { 127, 0, 0, 1 }), 32 )}
                })
                .UseCors(policy => policy.SetPreflightMaxAge(TimeSpan.FromMinutes(10)).AllowAnyMethod().AllowAnyOrigin().AllowAnyHeader())
                .UseStaticFiles(new StaticFileOptions {
                    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "../uploads")),
                    RequestPath = new PathString("/uploads")
                })
                .Use(async (context, next) => {
                    try {
                        await next();
                    } catch (Exception e) {
                        context.Response.StatusCode = e is NotImplementedException ? 404 : e is UnauthorizedAccessException || e is SecurityTokenValidationException ? 401 : e is ArgumentException ? 400 : 500;
                        context.Response.ContentType = "application/json; charset=utf-8";

                        string message = "";

                        Exception x = e;
                        do {
                            message += x.Message + "\r\n\r\n";
                        } while ((x = x.InnerException) != null);

                        await context.Response.WriteAsync(JsonConvert.SerializeObject(new {
                            code = -1,
                            message = message.Substring(0, message.Length - 4),
                            stacktrace = e.StackTrace
                        }));
                    }
                })
                .Use(async (context, next) => {
                    Token token = null;
                    string strToken = context.Request.Query["token"];

                    if (strToken == null) {
                        string authorization = context.Request.Headers["Authorization"];
                        if (authorization != null) {
                            if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) {
                                strToken = authorization.Substring("Bearer ".Length).Trim();
                            } else if(authorization.StartsWith("Basic ")) {
                                var encoding = Encoding.UTF8.GetString(Convert.FromBase64String(authorization.Substring("Basic ".Length))).Split(':');
                                if(encoding.Length != 2) {
                                    throw new UnauthorizedAccessException();
                                }

                                var username = encoding[0];
                                var password = encoding[1];

                                token = await personal_token_cache.GetAsync(username + ":" + password, async () => {
                                    var db = context.RequestServices.GetRequiredService<ApplicationDbContext>();

                                    var _code = AccountController.StringToByteArrayFastest(username);
                                    var id = new Guid(_code.ToArray());

                                    var app2 = await db.pacs
                                        .Include(a => a.user)
                                        .SingleOrDefaultAsync(b => b.id == id);

                                    if (app2 == null || app2.password_hash == null || !app2.password_hash.SequenceEqual(
                                        KeyDerivation.Pbkdf2(
                                            password: password,
                                            salt: app2.password_salt,
                                            iterationCount: 10000,
                                            numBytesRequested: 256 / 8,
                                            prf: KeyDerivationPrf.HMACSHA1
                                        )
                                    )) {
                                        throw new UnauthorizedAccessException();
                                    }
                                    
                                    return await token_cache.GetAsync(app2.user.id.ToString(), () => Task.FromResult<Token>(new UserToken(app2.user.id, app2.user.name, app2.user.type)), TimeSpan.FromDays(3));
                                }, TimeSpan.FromDays(3));
                            }
                        }
                    }

                    if (strToken != null) {
                        var a = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();

                        var claims = a.ValidateToken(strToken, new TokenValidationParameters {
                            ValidateAudience = true,
                            ValidIssuer = "Flare",
                            ValidAudience = "Flare Users",
                            IssuerSigningKey = _key
                        }, out SecurityToken _);

                        context.User = claims;

                        var strID = context.User?.Claims?.FirstOrDefault(b => b.Type == "user_id")?.Value;
                        if(Int32.TryParse(strID, out int dwID)) { 
                            token = await token_cache.GetAsync(strID, async () => {
                                var db = context.RequestServices.GetRequiredService<ApplicationDbContext>();
                                var app2 = await db.users.SingleAsync(b => b.id == dwID);

                                return new UserToken(app2.id, app2.name, app2.type);
                            }, TimeSpan.FromDays(3));
                        }
                    }

                    context.AddScoped(() => {
                        if(token == null) {
                            throw new UnauthorizedAccessException();
                        }

                        return token;
                    });

                    await next();
                })
                .UseMvc();
        }

        public static void Main(string[] args) {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) => {
                    config
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                        .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: false);
                    context.Configuration = config.Build();
                })
                .UseStartup<Startup>()
                .UseIISIntegration()
                .Build();
    }
}
