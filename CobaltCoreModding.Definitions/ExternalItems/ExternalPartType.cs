using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ExternalItems
{
    public class ExternalPartType
    {

        private int? id;

        public int? Id { get => id; set { if (id != null) throw new Exception("Id already assigned!"); id = value; } }

        public string GlobalName { get; init; }

        public ExternalPartType(string globalName)
        {
            GlobalName = globalName;
        }
    }
}
