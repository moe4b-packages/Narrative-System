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
	public class EvaluateVariable<T> : Node
	{
        public Variable<T> Variable { get; protected set; }

        public Dictionary<T, Branch.Delegate> Options { get; protected set; }
        public EvaluateVariable<T> Condition(T value, Branch.Delegate branch)
        {
            Options[value] = branch;

            return this;
        }

        public Branch.Delegate Fallback { get; protected set; }
        public EvaluateVariable<T> Default(Branch.Delegate branch)
        {
            Fallback = branch;

            return this;
        }

        public override void Invoke()
        {
            base.Invoke();

            Debug.Log(Options.ToCollectionString());

            if (Options.TryGetValue(Variable.Value, out var branch) == false)
                branch = Fallback;

            if(branch == null)
            {
                Debug.LogWarning($"Evaluation for '{Variable}' not Found, Continuing Script");
                return;
            }

            Script.Invoke(branch);
        }

        public EvaluateVariable(Variable<T> variable)
        {
            this.Variable = variable;

            Options = new Dictionary<T, Branch.Delegate>();
        }
	}

	partial class Script
    {
		protected EvaluateVariable<T> EvaluateVariable<T>(Variable<T> variable) => new EvaluateVariable<T>(variable);
    }
}