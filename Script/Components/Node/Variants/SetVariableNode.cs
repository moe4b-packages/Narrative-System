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
	public class SetVariableNode<T> : Node
	{
		public Variable<T> Variable { get; protected set; }

		public T Value { get; protected set; }

        public override void Invoke()
        {
            base.Invoke();

            Variable.Value = Value;

            Script.Continue();
        }

        public SetVariableNode(Variable<T> variable, T value)
        {
			this.Variable = variable;
			this.Value = value;
        }
	}

	partial class Script
    {
		public SetVariableNode<T> SetVariable<T>(Variable<T> variable, T value) => new SetVariableNode<T>(variable, value);
    }
}