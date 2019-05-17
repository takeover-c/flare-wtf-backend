using System.IO;
using System.Threading.Tasks;
using Flare.Base;

namespace Flare.Parsers {
    public abstract class StreamBasedParser : IFlareContextParser {
        public StreamReader streamReader;

        public StreamBasedParser(StreamReader streamReader) {
            this.streamReader = streamReader;
        }
        
        public abstract Task<FlareContext> ParseSingle();
    }
}
