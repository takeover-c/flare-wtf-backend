using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flare.Filters {
    public class RegexBasedFilter : IBaseFilter {
        private int Flags;
        private Regex Regex;

        public RegexBasedFilter(Regex Regex, int Flags) {
            this.Regex = Regex;
            this.Flags = Flags;
        }

        public Task<bool> ProcessRequest(FlaggableFlareContext context) {
            if (Regex.IsMatch(context.Context.request.path + "?" + context.Context.request.query_string)) {
                context.Flags |= Flags;
                return Task.FromResult(true);
            }
            
            return Task.FromResult(false);
        }
    }
}