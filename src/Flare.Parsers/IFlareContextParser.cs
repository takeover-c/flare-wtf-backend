using System.IO;
using System.Threading.Tasks;
using Flare.Base;

namespace Flare.Parsers {
    public interface IFlareContextParser {
        Task<FlareContext> ParseSingle();
    }
}