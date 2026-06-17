using System;
using System.Collections.Generic;

namespace DawnOfShadow.Core
{
    public static class GameEventSystem
    {
        private static readonly Dictionary<string, Action<object>> _eventDictionary = new Dictionary<string, Action<object>>();

        public static void Subscribe(string eventName, Action<object> listener)
        {
            if (_eventDictionary.ContainsKey(eventName))
            {
                _eventDictionary[eventName] += listener;
            }
            else
            {
                _eventDictionary.Add(eventName, listener);
            }
        }

        public static void Unsubscribe(string eventName, Action<object> listener)
        {
            if (_eventDictionary.ContainsKey(eventName))
            {
                _eventDictionary[eventName] -= listener;
                if (_eventDictionary[eventName] == null)
                {
                    _eventDictionary.Remove(eventName);
                }
            }
        }

        public static void Publish(string eventName, object data = null)
        {
            if (_eventDictionary.ContainsKey(eventName))
            {
                _eventDictionary[eventName]?.Invoke(data);
            }
        }
    }
}
