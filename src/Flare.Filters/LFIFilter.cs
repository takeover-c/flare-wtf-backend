using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flare.Filters {
    public class LFIFilter : AggregatedFilterPipeline {
        public LFIFilter() : base(new List<IBaseFilter>() {
            new RegexBasedFilter(new Regex(@"\.\.\/"), (int)VulnerabilityType.LFI),
            new RegexBasedFilter(new Regex(@"file\:\/\/"), (int)VulnerabilityType.LFI),
            new RegexBasedFilter(new Regex(@"etc\/"), (int)VulnerabilityType.LFI)
        }) {
            
        }
    }
}