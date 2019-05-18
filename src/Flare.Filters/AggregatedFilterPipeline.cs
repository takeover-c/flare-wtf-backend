using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flare.Filters {
    public class AggregatedFilterPipeline : IBaseFilter {
        private List<IBaseFilter> baseFilters;
        
        public AggregatedFilterPipeline(int Flags = -1) {
            baseFilters = new List<IBaseFilter>();

            if ((Flags & (int) VulnerabilityType.SQL) != 0) {
                baseFilters.Add(new SqlInjectionFilter());
            }
            
            if ((Flags & (int) VulnerabilityType.XSS) != 0) {
                baseFilters.Add(new XSSFilter());
            }
            
            if ((Flags & (int) VulnerabilityType.LFI) != 0) {
                baseFilters.Add(new LFIFilter());
            }
            
            if ((Flags & (int) VulnerabilityType.SSI) != 0) {
                baseFilters.Add(new SSIFilter());
            }
        }

        public AggregatedFilterPipeline(List<IBaseFilter> baseFilters) {
            this.baseFilters = baseFilters;
        }
        
        public async Task<bool> ProcessRequest(FlaggableFlareContext context) {
            foreach (var filter in baseFilters) {
                if (await filter.ProcessRequest(context)) {
                    return true;
                }
            }

            return false;
        }
    }
}