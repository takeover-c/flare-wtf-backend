using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flare.Filters {
    public class XSSFilter : AggregatedFilterPipeline {
        public XSSFilter() : base(new List<IBaseFilter>() {
            new RegexBasedFilter(new Regex(@"\<?script\>?", RegexOptions.IgnoreCase), (int)VulnerabilityType.XSS),
            new RegexBasedFilter(new Regex(@"(https?|ftp|php|data):", RegexOptions.IgnoreCase), (int)VulnerabilityType.XSS)
        }) {
            
        }
    }
}