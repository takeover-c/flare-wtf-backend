using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Flare.Base;

namespace Flare.Parsers {
    public class CommonLogFormatParser : StreamBasedParser {
        private readonly Regex regex;
        
        public CommonLogFormatParser(StreamReader streamReader) : base(streamReader) {
            regex = new Regex(@"^(.*?)\ (.*?)\ (.*?)\ \[(.*)\] \""(.*?)\ (.*)\ (.*?)\"" (.*)\ (.*)$", RegexOptions.Compiled);
        }
        
        public override async Task<FlareContext> ParseSingle() {
            var line = await streamReader.ReadLineAsync();

            if (string.IsNullOrEmpty(line))
                return null;
            
            var matches = regex.Match(line).Groups;

            var path = HttpUtility.UrlDecode(matches[6].Value);
            
            var context = new FlareContext() {
                request = new FlareRequest() {
                    ip = matches[1].Value,
                    identity = matches[2].Value,
                    userid = matches[3].Value,
                    date = DateTimeOffset.ParseExact(matches[4].Value, "dd/MMM/yyyy:HH:mm:ss zzz", CultureInfo.InvariantCulture),
                    method = matches[5].Value,
                    path = path.Split("?")[0],
                    query_string = string.Join('?', path.Split("?").Skip(1))
                },
                response = new FlareResponse()
            };

            if(double.TryParse(matches[7].Value.Substring(5), out var http_version)) {
                context.request.http_version = (int)(http_version * 10);
            }
            
            if(int.TryParse(matches[8].Value, out var status_code)) {
                context.response.status_code = status_code;
            }
            
            if(int.TryParse(matches[9].Value, out var bytes_sent)) {
                context.response.bytes_sent = bytes_sent;
            }

            return context;
        }
    }
}