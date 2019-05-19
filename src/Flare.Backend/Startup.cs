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
using System.Net.Http;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore;
using System.Xml;
using Microsoft.Extensions.FileProviders;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Flare.Backend.Services;
using Flare.Base;
using Flare.Filters;
using MaxMind.GeoIP2;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Primitives;
using Tamturk.AspNetCore;

namespace Flare.Backend {
    public class Startup {
        private readonly IConfiguration _config;

        private SecurityKey _key;

        public static string endpoint;

        readonly InMemoryCache<Token> token_cache = new InMemoryCache<Token>();
        readonly InMemoryCache<Token> personal_token_cache = new InMemoryCache<Token>();

        public Startup(IConfiguration config) {
            _config = config;
            endpoint = config["endpoint"];
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
                .AddSingleton(new WebServiceClient(int.Parse(_config["geoip:user"]), _config["geoip:pass"]))
                .AddScoped<GeoIpService>()
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
            
            var pipeline = new AggregatedFilterPipeline();
            
            app
                .UseForwardedHeaders(new ForwardedHeadersOptions {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                    KnownNetworks = { new IPNetwork(new IPAddress(new byte[] { 127, 0, 0, 1 }), 32 )}
                })
                .Use(async (context, next) => {
                    var flareContext = new FlaggableFlareContext(new FlareContext() {
                        request = new FlareRequest() {
                            date = DateTimeOffset.Now,
                            http_version = (int)(double.Parse(context.Request.Protocol.Substring(5)) * 10),
                            identity = "-",
                            ip = context.Connection.RemoteIpAddress.ToString(),
                            method = context.Request.Method,
                            path = context.Request.Path.Value,
                            query_string = context.Request.QueryString.Value,
                            userid = "-"
                        },
                        response = null
                    });
                    
                    var domain = context.Request.Headers["host"];
                    
                    // Flare CDN logic
                    if (domain != "localhost:5000" && domain != "api.flare.wtf") {
                        var db = context.RequestServices.GetRequiredService<ApplicationDbContext>();

                        var server = await db.servers
                          .Where(a => a.proxy_active && a.domains.Any(b => b.domain == domain))
                          .FirstOrDefaultAsync();

                        if (server == null) {
                            context.Response.StatusCode = 404;
                            await context.Response.WriteAsync("The requested resource is not found. That's all we know.");
                            return;
                        }
                        
                        var ip = await context.RequestServices.GetRequiredService<GeoIpService>()
                                              .Query(flareContext.Context.request.ip);
                        
                        var db_request = new request {
                            server_id = server.id,
                            ip_id = ip.id,
                            request_identity = flareContext.Context.request.identity,
                            request_user_id = flareContext.Context.request.userid,
                            request_date = flareContext.Context.request.date,
                            request_method = flareContext.Context.request.method,
                            request_path = flareContext.Context.request.path,
                            request_query_string = flareContext.Context.request.query_string,
                            request_http_version = flareContext.Context.request.http_version,
                         //   response_code = flareContext.Context.response?.status_code,
                         //   response_length = flareContext.Context.response?.bytes_sent,
                         //   flags = flareContext.Flags
                        };

                        db.requests.Add(db_request);
                        
                        if (await pipeline.ProcessRequest(flareContext) && server.proxy_block_requests) {
                            await db.SaveChangesAsync();
                            
                            db_request.response_code = 418;
                            db_request.flags = flareContext.Flags;

                            await db.SaveChangesAsync();
                            
                            var text =
                                $"418 - I am a teapot. Your request #{db_request.id} is failed because of security checks. Contact the website owner with this number if you think this was a mistake.";

                            db_request.response_length = text.Length;

                            context.Response.StatusCode = 418;
                            await context.Response.WriteAsync(text);
                            return;
                        }

                        db_request.flags = flareContext.Flags;
                        
                        using (var httpClient = new HttpClient()) {
                            httpClient.DefaultRequestHeaders.Clear();
                            foreach (var header in context.Request.Headers
                                                          .Where(a => !a.Key.StartsWith("X-"))) {
                                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value.ToArray());
                            }
                            
                            httpClient.DefaultRequestHeaders.Add("X-Forwarded-For", context.Connection.RemoteIpAddress.ToString());
                            
                            using(var _req = new HttpRequestMessage(
                                context.Request.Method == "GET" ? HttpMethod.Get :
                                context.Request.Method == "POST" ? HttpMethod.Post :
                                context.Request.Method == "DELETE" ? HttpMethod.Delete :
                                context.Request.Method == "PUT" ? HttpMethod.Put :
                                context.Request.Method == "PATCH" ? HttpMethod.Patch :
                                context.Request.Method == "TRACE" ? HttpMethod.Trace :
                                context.Request.Method == "OPTIONS" ? HttpMethod.Options : throw new Exception("invalid method")
                                , $"http://{server.origin_ip}{context.Request.Path}{context.Request.QueryString}")) {
                                _req.Headers.Host = domain;
                                
                                using (var response = await httpClient.SendAsync(_req)) {
                                    
                                    db_request.response_code = context.Response.StatusCode = (int)response.StatusCode;

                                    foreach (var header in response.Headers
                                                                   .Where(a => a.Key != "Transfer-Encoding")) {
                                        context.Response.Headers.Add(header.Key, new StringValues(header.Value.ToArray()));
                                    }
                                    
                                    foreach (var header in response.Content.Headers) {
                                        context.Response.Headers.Add(header.Key, new StringValues(header.Value.ToArray()));
                                    }
                                    
                                    using(var streamWithProgess = new StreamWithProgress(context.Response.Body)) {
                                        await response.Content.CopyToAsync(streamWithProgess);
                                        await context.Response.Body.FlushAsync();
                                        context.Response.Body.Close();
                                        
                                        db_request.response_length = (int)streamWithProgess.bytesTotal;
                                        await db.SaveChangesAsync();
                                    }
                                }
                            }
                        }
                    }
                    else {
                        if (await pipeline.ProcessRequest(flareContext)) {
                            var text =
                                $"418 - I am a teapot. Your request is failed because of security checks. If you think it was a mistake contact support@flare.wtf";
                            
                            context.Response.StatusCode = 418;
                            await context.Response.WriteAsync(text);
                            return;
                        }
                        
                        await next();
                    }
                })
                .UseCors(policy => policy.SetPreflightMaxAge(TimeSpan.FromMinutes(10)).AllowAnyMethod().AllowAnyOrigin().AllowAnyHeader())
                .UseStaticFiles(new StaticFileOptions {
                    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "../uploads")),
                    RequestPath = new PathString("/uploads")
                })
                .Use(async (context, next) => {
                    IHttpBufferingFeature bufferingFeature = context.Features.Get<IHttpBufferingFeature>();
                    bufferingFeature?.DisableRequestBuffering();
                    bufferingFeature?.DisableResponseBuffering();
                    
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
