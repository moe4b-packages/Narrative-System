using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace MB.NarrativeSystem
{
	partial class Narrative
    {
        public EventsProperty Events { get; private set; } = new EventsProperty();
        public class EventsProperty
        {
            public List<Handler> Handlers { get; private set; }
            public class Handler
            {
                public string Key { get; protected set; }

                public Action Callback { get; protected set; }
                public void Invoke() => Callback?.Invoke();

                public Handler(string key, Action callback)
                {
                    this.Key = key;
                    this.Callback = callback;
                }
            }

            public delegate void RaiseDelegate(string key);
            public event RaiseDelegate OnRaise;
            public void Raise(string key)
            {
                if (InvokeHandlers(key) == false)
                    Debug.LogWarning($"Narrative Event '{key}' Has no Registerd Handlers");

                OnRaise?.Invoke(key);
            }

            bool InvokeHandlers(string key)
            {
                bool invoked = false;

                for (int i = 0; i < Handlers.Count; i++)
                {
                    if (Handlers[i].Key == key)
                    {
                        invoked = true;
                        Handlers[i].Invoke();
                    }
                }

                return invoked;
            }

            public void Register(string key, Action callback)
            {
                var handler = new Handler(key, callback);

                Handlers.Add(handler);
            }

            public bool Unregister(string key, Action callback)
            {
                for (int i = Handlers.Count; i-- > 0;)
                {
                    if(Handlers[i].Key == key && Handlers[i].Callback == callback)
                    {
                        Handlers.RemoveAt(i);
                        return true;
                    }
                }

                return false;
            }

            public EventsProperty()
            {
                Handlers = new List<Handler>();
            }
        }
    }
}