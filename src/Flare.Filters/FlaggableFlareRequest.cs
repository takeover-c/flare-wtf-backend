using Flare.Base;

namespace Flare.Filters {
    public class FlaggableFlareContext {
        public FlareContext Context { get; set; }

        public int Flags { get; set; }

        public FlaggableFlareContext(FlareContext context) {
            this.Context = context;
            this.Flags = 0;
        }
    }
}