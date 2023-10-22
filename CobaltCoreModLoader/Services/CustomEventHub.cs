using CobaltCoreModding.Definitions.ModContactPoints;
using Microsoft.Extensions.Logging;

namespace CobaltCoreModLoader.Services
{
    public class CustomEventHub : ICustomEventHub
    {
        private const int CLEANUP_AFTER = 100;
        private static readonly Dictionary<string, Tuple<Type, List<WeakReference<object>>>> customEventLookup = new Dictionary<string, Tuple<Type, List<WeakReference<object>>>>();
        private static ILogger<CustomEventHub>? logger;
        private int cleanup_countdown = CLEANUP_AFTER;

        public CustomEventHub(ILogger<CustomEventHub> logger)
        {
            CustomEventHub.logger = logger;
        }

        public bool ConnectToEvent<T>(string eventName, Action<T> handler)
        {
            cleanup_countdown--;
            if (cleanup_countdown <= 0)
            {
                lock (customEventLookup)
                {
                    if (cleanup_countdown <= 0)
                    {
                        foreach (var list in customEventLookup.Values.Select(e => e.Item2))
                        {
                            lock (list)
                            {
                                list.RemoveAll(e => !e.TryGetTarget(out _));
                            }
                        }
                        cleanup_countdown = CLEANUP_AFTER;
                    }
                }
            }

            if (!customEventLookup.TryGetValue(eventName, out var entry))
            {
                logger?.LogWarning("Unkown Event {0}", eventName);
                return false;
            }
            if (entry.Item1 != typeof(T))
            {
                logger?.LogWarning("Event {0} expects Action<{1}> but was given Action<{2}>", eventName, entry.Item1.Name, typeof(T).Name);
                return false;
            }
            //Register weak reference to allow even actions/artifact sto listen to events without being hung up.
            lock (entry.Item2)
            {
                entry.Item2.Add(new(handler));
            }
            return true;
        }

        public void LoadManifest()
        {
            foreach (var manifest in ModAssemblyHandler.CustomEventManifests)
            {
                manifest.LoadManifest(this);
            }
        }

        public bool MakeEvent<T>(string eventName)
        {
            return MakeEvent(eventName, typeof(T));
        }

        public bool MakeEvent(string eventName, Type eventArgType)
        {
            if (customEventLookup.ContainsKey(eventName))
            {
                logger?.LogError("Event {0} already registered", eventName);
                return false;
            }

            customEventLookup.Add(eventName, new(eventArgType, new List<WeakReference<object>>()));
            return true;
        }

        public void SignalEvent<T>(string eventName, T eventArg)
        {
            if (!customEventLookup.TryGetValue(eventName, out var entry))
            {
                logger?.LogError("Unkown Event {0} signaled", eventName);
                return;
            }

            if (entry.Item1 != typeof(T))
            {
                throw new Exception($"Attempted to signal event {eventName} with wrong type {typeof(T).Name}");
            }

            foreach (var listener_reference in entry.Item2)
            {
                if (!listener_reference.TryGetTarget(out var obj_listener))
                    continue;
                if (obj_listener is not Action<T> listener)
                    continue;
                try
                {
                    listener.Invoke(eventArg);
                }
                catch (Exception err)
                {
                    logger?.LogCritical(err, "During custom event {0} exception was thrown in listener.", eventName);
                }
            }
        }
    }
}