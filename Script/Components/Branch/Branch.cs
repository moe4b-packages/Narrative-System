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

using System.Runtime.CompilerServices;
using System.Reflection;

namespace MB.NarrativeSystem
{
    public class Branch
    {
        public string ID { get; protected set; }

        public Delegate Function { get; protected set; }

        public Script Script { get; protected set; }
        public int Index { get; protected set; }

        public Branch Previous
        {
            get
            {
                if (Script.Branches.List.TryGet(Index - 1, out var value) == false)
                    return null;

                return value;
            }
        }
        public Branch Next
        {
            get
            {
                if (Script.Branches.List.TryGet(Index + 1, out var value) == false)
                    return null;

                return value;
            }
        }

        public NodesProperty Nodes { get; protected set; }
        [Serializable]
        public class NodesProperty
        {
            protected Branch Branch { get; set; }

            public List<Node> List { get; protected set; }

            public Node this[int index] => List[index];
            public int Count => List.Count;

            public Node First => List.SafeIndexer(0);
            public Node Last => List.SafeIndexer(List.Count - 1);

            internal void Register(Node node)
            {
                var index = List.Count;

                List.Add(node);

                node.Set(Branch, index);
            }

            public NodesProperty(Branch branch)
            {
                this.Branch = branch;

                List = new List<Node>();
            }
        }

        public override string ToString() => $"{Script}->{ID}";

        public Branch(string id, Delegate function, Script script, int index)
        {
            this.ID = id;
            this.Function = function;
            this.Script = script;
            this.Index = index;

            Nodes = new NodesProperty(this);
        }

        public delegate void Delegate();

        //Static Utility

        public static string FormatID(Delegate function) => function.Method.Name;
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    sealed class BranchAttribute : Attribute
    {
        public int Line { get; private set; }

        public BranchAttribute([CallerLineNumber] int line = 0)
        {
            this.Line = line;
        }

        public static bool IsDefined(MethodInfo info) => GetAttribute(info) != null;
        public static BranchAttribute GetAttribute(MethodInfo info) => info.GetCustomAttribute<BranchAttribute>(true);

        public static Branch.Delegate CreateDelegate(MethodInfo info, object target)
        {
            var type = typeof(Branch.Delegate);

            return info.CreateDelegate(type, target) as Branch.Delegate;
        }
    }
}