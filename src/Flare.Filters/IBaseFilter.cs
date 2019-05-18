using System;
using System.Threading.Tasks;

namespace Flare.Filters {
    public interface IBaseFilter {
        Task<bool> ProcessRequest(FlaggableFlareContext context);
    }
}