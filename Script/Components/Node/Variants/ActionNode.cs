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
	public class ActionNode : Node
	{
        public Action Method { get; protected set; }

        protected internal override void Invoke()
        {
            base.Invoke();

            Method.Invoke();

            Playback.Next();
        }

        public ActionNode(Action action)
        {
            this.Method = action;
        }
	}

    partial class Script
    {
        public static ActionNode Action(Action function) => new ActionNode(function);

        public static ActionNode Action<T>(Action<T> function, T value)
        {
            void Surrogate() => function.Invoke(value);

            return new ActionNode(Surrogate);
        }
        public static ActionNode Action<T>(Action<object> function, T value)
        {
            void Surrogate() => function.Invoke(value);

            return new ActionNode(Surrogate);
        }
        public static ActionNode Action(Action<object> function, object value)
        {
            void Surrogate() => function.Invoke(value);

            return new ActionNode(Surrogate);
        }
    }
}