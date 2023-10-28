using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ExternalItems
{
    public class ExternalPart
    {
        public string GlobalName { get; init; }


        private object? PartObjectTemplate { get; init; }

        public virtual object GetPartObject => PartObjectTemplate ?? throw new NotImplementedException("GetPartObject returned null.");

        public ExternalSprite PartSprite { get; set; }

        public string Key => "@mod_part:" + GlobalName;

        public ExternalPart(string globalName, object partObjectTemplate, ExternalSprite partSprite)
        {
            GlobalName = globalName;
            PartObjectTemplate = partObjectTemplate;
            PartSprite = partSprite;
        }

        protected ExternalPart(string globalName, ExternalSprite partSprite)
        {
            GlobalName = globalName;
            PartSprite = partSprite;
        }
    }
}
