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
        public delegate IEnumerable<Node> Delegate();

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

        public IEnumerator<Node> GetEnumerator() => Function().GetEnumerator();

        public override string ToString() => $"{Script}->{ID}";

        public Branch(string id, Delegate function, Script script, int index)
        {
            this.ID = id;
            this.Function = function;

            this.Script = script;
            this.Index = index;
        }

        //Static Utility

        public static string FormatID(Delegate function) => function.Method.Name;
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class BranchAttribute : Attribute
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