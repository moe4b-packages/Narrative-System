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
        public Script Script { get; protected set; }
        public int Index { get; protected set; }

        public Delegate Function { get; protected set; }
        public delegate IEnumerable<Script.Block> Delegate();

        public string ID { get; protected set; }
        public string Name { get; protected set; }

        public override string ToString() => Format.FullName(Script.Name, Name);

        public Branch(Script script, int index, Delegate function)
        {
            this.Script = script;
            this.Index = index;
            this.Function = function;

            ID = Format.ID(function);
            Name = Format.Name(function);
        }

        //Static Utility

        public static class Format
        {
            public static string ID(Delegate function) => ID(function.Method);
            public static string ID(MethodInfo method) => method.Name;

            public static string Name(Delegate function) => Name(function.Method);
            public static string Name(MethodInfo method)
            {
                var id = ID(method);

                return MUtility.PrettifyName(id);
            }

            public static string FullName(Type script, MethodInfo branch)
            {
                return FullName(Script.Format.Name.Retrieve(script), ID(branch));
            }
            public static string FullName(string script, string branch) => $"{script}->{branch}";
        }
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class BranchAttribute : Attribute
    {
        public int Line { get; private set; }

#if UNITY_EDITOR || DEBUG
        public string Path { get; private set; }
#endif

#if UNITY_EDITOR || DEBUG
        public BranchAttribute([CallerLineNumber] int line = 0, [CallerFilePath] string path = "")
#else
        public BranchAttribute([CallerLineNumber] int line = 0)
#endif
        {
            this.Line = line;

#if UNITY_EDITOR || DEBUG
            this.Path = path;
#endif
        }

        public static bool IsDefined(MethodInfo info) => Get(info) != null;

        public static BranchAttribute Get(MethodInfo info) => info.GetCustomAttribute<BranchAttribute>(true);

        public static bool TryGet(MethodInfo info, out BranchAttribute attribute)
        {
            attribute = Get(info);

            return attribute != null;
        }
    }
}