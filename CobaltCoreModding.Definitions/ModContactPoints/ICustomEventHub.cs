namespace CobaltCoreModding.Definitions.ModContactPoints
{
    /// <summary>
    ///
    /// </summary>
    public interface ICustomEventHub
    {
        bool ConnectToEvent<T>(string eventName, Action<T> handler);

        void DisconnectFromEvent<T>(string eventName, Action<T> handler);

        bool MakeEvent<T>(string eventName);

        bool MakeEvent(string eventName, Type eventArgType);

        void SignalEvent<T>(string eventName, T eventArg);
    }
}