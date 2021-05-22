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
        Delegate function;

        public delegate void Delegate(CallbackNode self);

        public override void Invoke()
        {
            base.Invoke();

            function.Invoke(this);

            Script.Continue();
        }

        public CallbackNode(Delegate function)
        {
            this.function = function;
        }
	}

	partial class Script
    {
        protected CallbackNode Callback(CallbackNode.Delegate function)
        {
            var node = new CallbackNode(function);

            Register(node);

            return node;
        }
    }
}