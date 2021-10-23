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

        public override void Invoke()
        {
            base.Invoke();

            Method.Invoke();

            Narrative.Player.Continue();
        }

        public ActionNode(Action action)
        {
            this.Method = action;
        }
	}

    partial class Script
    {
        public ActionNode Action(Action function) => new ActionNode(function);

        public ActionNode Action<T>(Action<T> function, T value)
        {
            void Surrogate() => function.Invoke(value);

            return new ActionNode(Surrogate);
        }
        public ActionNode Action<T>(Action<object> function, T value)
        {
            void Surrogate() => function.Invoke(value);

            return new ActionNode(Surrogate);
        }
        public ActionNode Action(Action<object> function, object value)
        {
            void Surrogate() => function.Invoke(value);

            return new ActionNode(Surrogate);
        }
    }
}