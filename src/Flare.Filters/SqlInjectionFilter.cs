using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Flare.Filters {
    public class SqlInjectionFilter : AggregatedFilterPipeline {
        public SqlInjectionFilter() : base(new List<IBaseFilter>() {
            new RegexBasedFilter(new Regex(@"(OR|AND)( |\%)(\d+)\=(\d+)", RegexOptions.IgnoreCase), (int)VulnerabilityType.SQL),
            new RegexBasedFilter(new Regex(@"(OR|AND)(.*?)IS( |\%)?", RegexOptions.IgnoreCase), (int)VulnerabilityType.SQL),
            new RegexBasedFilter(new Regex(@"\;\W*(UPDATE|SELECT|DROP|DELETE)", RegexOptions.IgnoreCase), (int)VulnerabilityType.SQL)
        }) {
            
        }
    }
}