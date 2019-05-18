using System.IO;
using System.Threading.Tasks;
using Flare.Backend.Models;
using Flare.Backend.Services;
using Flare.Base;
using Flare.Filters;
using Flare.Parsers;
using Microsoft.AspNetCore.Mvc;

namespace Flare.Backend.Controllers {
    public class PassiveLogDetectionController : ControllerBase {
        private readonly ApplicationDbContext db;
        private readonly GeoIpService geoIpService;

        public PassiveLogDetectionController(ApplicationDbContext db, GeoIpService geoIpService) {
            this.db = db;
            this.geoIpService = geoIpService;
        }

        [HttpPost("server/{server_id}/apache2")]
        public async Task ProcessLogFile(int server_id) {
            int savecounter = 0;
            
            using (var streamReader = new StreamReader(Request.Body)) {
                var commonLogParser = new CommonLogFormatParser(streamReader);
                
                var pipeline = new AggregatedFilterPipeline();
                
                FlareContext context;
                while ((context = await commonLogParser.ParseSingle()) != null) {
                    var flaggableContext = new FlaggableFlareContext(context);
                    await pipeline.ProcessRequest(flaggableContext);

                    var ip = await geoIpService.Query(flaggableContext.Context.request.ip);

                    db.requests.Add(new request {
                        server_id = server_id,
                        ip_id = ip.id,
                        request_identity = context.request.identity,
                        request_user_id = context.request.userid,
                        request_date = context.request.date,
                        request_method = context.request.method,
                        request_path = context.request.path,
                        request_query_string = context.request.query_string,
                        request_http_version = context.request.http_version,
                        response_code = context.response?.status_code,
                        response_length = context.response?.bytes_sent,
                        flags = flaggableContext.Flags
                    });
                    
                    if(++savecounter % 4000 == 0) {
                        await db.SaveChangesAsync();
                        savecounter = 0;
                    }
                }
            }
        }
    }
}