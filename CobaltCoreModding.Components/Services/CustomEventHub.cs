using CobaltCoreModding.Definitions.ModContactPoints;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace CobaltCoreModding.Components.Services
{
    public class CustomEventHub : ICustomEventHub
    {
        /// <summary>
        /// Similiar to volatileCustomEventLookup but with the assumption that the user can call DisconnectFromEvent function.
        /// </summary>
        private static readonly Dictionary<string, Tuple<Type, HashSet<object>>> persistentCustomEventLookup = new();

        /// <summary>
        /// Since Cards/Artifacts etc. are not disposable, we cannot hold permanent rerference for fear of an memory leak. Thus we only store a week reference to the target instance of the action and the action itself.
        /// </summary>
        private static readonly Dictionary<string, Tuple<Type, ConditionalWeakTable<object, object>>> volatileCustomEventLookup = new Dictionary<string, Tuple<Type, ConditionalWeakTable<object, object>>>();

        private static ILogger<CustomEventHub>? logger;
        private readonly ModAssemblyHandler modAssemblyHandler;

        public CustomEventHub(ILogger<CustomEventHub> logger, ModAssemblyHandler mah)
        {
            CustomEventHub.logger = logger;
            modAssemblyHandler = mah;
        }

        public bool ConnectToEvent<T>(string eventName, Action<T> handler)
        {
            if (!volatileCustomEventLookup.TryGetValue(eventName, out var entry))
            {
                logger?.LogWarning("Unkown Event {0}", eventName);
                return false;
            }
            if (entry.Item1 != typeof(T))
            {
                logger?.LogWarning("Event {0} expects Action<{1}> but was given Action<{2}>", eventName, entry.Item1.Name, typeof(T).Name);
                return false;
            }

            if (handler.Target == null || handler.Target is IDisposable)
            {
                logger?.LogWarning("Handler has no target or is disposable. Please make sure to disconnect this event handler manually!");
                if (persistentCustomEventLookup.TryGetValue(eventName, out var persistent_entry))
                {
                    return persistent_entry.Item2.Add(handler);
                }
            }
            else
            {
                //Register weak reference to allow even actions/artifact sto listen to events without being hung up.
                try
                {
                    entry.Item2.Add(handler.Target, handler);
                }
                catch (ArgumentException)
                {
                    logger?.LogCritical("Event {0} attempted to reigster a handler with a target already existing in this event", eventName);
                    return false;
                }
            }

            return true;
        }

        public void DisconnectFromEvent<T>(string eventName, Action<T> handler)
        {
            if (!volatileCustomEventLookup.TryGetValue(eventName, out var entry))
            {
                logger?.LogError("Unkown event {0}", eventName);
                return;
            }
            if (entry.Item1 != typeof(T))
            {
                logger?.LogError("Event {0} given type {1} doesn't match {2}", eventName, typeof(T).Name, entry.Item1.Name);
                return;
            }

            if (handler.Target == null || handler.Target is IDisposable)
            {
                if (persistentCustomEventLookup.TryGetValue(eventName, out var persistent_entry))
                {
                    persistent_entry.Item2.Remove(handler);
                }
            }
            else
            {
                entry.Item2.Remove(handler.Target);
            }
        }

        public void LoadManifest()
        {
            foreach (var manifest in modAssemblyHandler.LoadOrderly(ModAssemblyHandler.CustomEventManifests, logger))
            {
                try
                {
                    manifest.LoadManifest(this);
                }
                catch (Exception err)
                {
                    manifest.Logger?.LogError(err, "Exception caught by CustomEventHub registry");
                }
            }
        }

        public bool MakeEvent<T>(string eventName)
        {
            return MakeEvent(eventName, typeof(T));
        }

        public bool MakeEvent(string eventName, Type eventArgType)
        {
            if (volatileCustomEventLookup.ContainsKey(eventName))
            {
                logger?.LogError("Event {0} already registered", eventName);
                return false;
            }
            volatileCustomEventLookup.Add(eventName, new(eventArgType, new ConditionalWeakTable<object, object>()));
            persistentCustomEventLookup.Add(eventName, new(eventArgType, new()));
            return true;
        }

        public void SignalEvent<T>(string eventName, T eventArg)
        {
            if (!volatileCustomEventLookup.TryGetValue(eventName, out var entry))
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
                if (listener_reference.Value is not Action<T> listener)
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

            if (persistentCustomEventLookup.TryGetValue(eventName, out var persistent_entry))
            {
                foreach (var obj in persistent_entry.Item2)
                {
                    if (obj is not Action<T> listener)
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
}