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
	public class CallbackNode : Node
	{
        public Action Action { get; protected set; }

        public override void Invoke()
        {
            base.Invoke();

            Action.Invoke();

            Script.Continue();
        }

        public CallbackNode(Action action)
        {
            this.Action = action;
        }
	}

    partial class Script
    {
        public CallbackNode Callback(Action action) => new CallbackNode(action);

        public CallbackNode Callback<T>(Action<T> action, T value)
        {
            void Surrogate() => action.Invoke(value);

            return new CallbackNode(Surrogate);
        }
    }
}