using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Flare.Base;

namespace Flare.Parsers {
    public class CommonLogFormatParser : StreamBasedParser {
        private readonly Regex regex;
        
        public CommonLogFormatParser(StreamReader streamReader) : base(streamReader) {
            regex = new Regex(@"^(.*?)\ (.*?)\ (\S+)\ \[(.*)\] \""(.*?)\ (.*?)\ (.*?)\"" (.*)\ (.*)$", RegexOptions.Compiled);
        }
        
        public override async Task<FlareContext> ParseSingle() {
            var line = await streamReader.ReadLineAsync();

            if (string.IsNullOrEmpty(line))
                return null;
            
            var matches = regex.Matches(line);

            var path = matches[5].Value;
            
            return new FlareContext() {
                request = new FlareRequest() {
                    ip = matches[0].Value,
                    identity = matches[1].Value,
                    userid = matches[2].Value,
                    // TODO: timezone check on date field
                    date = DateTime.ParseExact(matches[3].Value, "dd/MMM/yyyy:HH:mm:ss", CultureInfo.InvariantCulture),
                    method = matches[4].Value,
                    path = path.Split("?")[0],
                    query_string = string.Join('?', path.Split("?").Skip(1)),
                    http_version = (int)(double.Parse(matches[6].Value.Substring(5)) * 10)
                },
                response = new FlareResponse() {
                    status_code = int.Parse(matches[8].Value),
                    bytes_sent = int.Parse(matches[9].Value)
                }
            };
        }
    }
}