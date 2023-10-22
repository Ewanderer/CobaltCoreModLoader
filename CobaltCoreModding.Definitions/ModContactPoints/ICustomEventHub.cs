using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CobaltCoreModding.Definitions.ModContactPoints
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICustomEventHub
    {

        bool MakeEvent<T>(string eventName);

        bool MakeEvent(string eventName, Type eventArgType);

        bool ConnectToEvent<T>(string eventName, Action<T> handler);

        void SignalEvent<T>(string eventName, T eventArg);

    }
}
