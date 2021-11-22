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
	public class EvaluateNode<T> : Node
	{
        public Variable<T> Variable { get; protected set; }

        public Dictionary<T, Branch.Delegate> Options { get; protected set; }
        protected void Register(T value, Branch.Delegate branch)
        {
            Options[value] = branch;
        }

        [NarrativeConstructorMethod]
        public Condition If(T value) => new Condition(this, value);
        public struct Condition
        {
            EvaluateNode<T> node;
            T condition;

            [NarrativeConstructorMethod]
            public EvaluateNode<T> Then(Branch.Delegate branch)
            {
                node.Register(condition, branch);
                return node;
            }

            public Condition(EvaluateNode<T> node, T condition)
            {
                this.node = node;
                this.condition = condition;
            }
        }

        public Branch.Delegate Fallback { get; protected set; }
        [NarrativeConstructorMethod]
        public EvaluateNode<T> Default(Branch.Delegate branch)
        {
            Fallback = branch;
            return this;
        }

        protected internal override void Invoke()
        {
            base.Invoke();

            if (Options.TryGetValue(Variable.Value, out var branch))
            {
                Playback.Goto(branch);
            }
            else
            {
                if (Fallback == null)
                    Playback.Next();
                else
                    Playback.Goto(Fallback);
            }
        }

        public EvaluateNode(Variable<T> variable)
        {
            this.Variable = variable;

            Options = new Dictionary<T, Branch.Delegate>();
        }
	}

	partial class Script
    {
        [NarrativeConstructorMethod]
        public static EvaluateNode<T> Evaluate<T>(Variable<T> variable) => new EvaluateNode<T>(variable);
    }
}