using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ItemLookups
{
    public interface IEnemyLookup
    {
        public object LookupEnemy(string globalName);
    }
}
