using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flare.Filters {
    public class SSIFilter : AggregatedFilterPipeline {
        public SSIFilter() : base(new List<IBaseFilter>() {
            new RegexBasedFilter(new Regex(@"\<\?php", RegexOptions.IgnoreCase), (int) VulnerabilityType.SSI),
            new RegexBasedFilter(new Regex(@"(https?|ftp|php|data):", RegexOptions.IgnoreCase),
                (int) VulnerabilityType.SSI),
            new RegexBasedFilter(new Regex(@"bin\/b?a?sh", RegexOptions.IgnoreCase),
                (int) VulnerabilityType.SSI),
            new RegexBasedFilter(new Regex(@"bin\/git", RegexOptions.IgnoreCase),
                (int) VulnerabilityType.SSI),
            new RegexBasedFilter(new Regex(@"\\x[0-9a-f]", RegexOptions.IgnoreCase),
                (int) VulnerabilityType.SSI),
            new RegexBasedFilter(new Regex(@"\| ls", RegexOptions.IgnoreCase),
                (int) VulnerabilityType.SSI),
            new RegexBasedFilter(new Regex(@"\=pwd", RegexOptions.IgnoreCase),
                (int) VulnerabilityType.SSI)
        }) {

        }
    }
}