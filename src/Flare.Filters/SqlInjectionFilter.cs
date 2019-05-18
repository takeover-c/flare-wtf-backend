using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flare.Filters {
    public class SqlInjectionFilter : AggregatedFilterPipeline {
        public SqlInjectionFilter() : base(new List<IBaseFilter>() {
            new RegexBasedFilter(new Regex(@"(\')|(\%27)|(\-\-)|(\#)|(\%23)", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace), (int)VulnerabilityType.SQL),
            new RegexBasedFilter(new Regex(@"\w*((\%27)|(\'))((\%6F)|o|(\%4F))((\%72)|r|(\%52))", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace), (int)VulnerabilityType.SQL),
            new RegexBasedFilter(new Regex(@"exec(\s|\+)+(s|x)p\w+", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace), (int)VulnerabilityType.SQL)
        }) {
            
        }
    }
}