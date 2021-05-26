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
        public string Name { get; protected set; }

        public Delegate Function { get; protected set; }
        public delegate IEnumerator<Node> Delegate();

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

        public IEnumerator<Node> GetEnumerator() => Function();

        public override string ToString() => Format.FullName(Script.Name, Name);

        public Branch(Delegate function, Script script, int index)
        {
            this.Function = function;

            ID = Format.ID(function);
            Name = Format.Name(function);

            this.Script = script;
            this.Index = index;
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

            public static string FullName(string script, string branch) => $"{script} :: {branch}";
        }

        public static class Composition
        {
            public static Dictionary<Type, Hierarchy> Dictionary { get; private set; }

            public struct Data
            {
                public MethodInfo Method { get; private set; }
                public BranchAttribute Attribute { get; private set; }

                public Data(MethodInfo method, BranchAttribute attribute)
                {
                    this.Method = method;
                    this.Attribute = attribute;
                }
            }

            public class Hierarchy : List<Data> { }

            public static List<Delegate> Read(Script script)
            {
                var type = script.GetType();

                var tree = ReadInheritanceTree(type);

                var list = new List<Delegate>();

                foreach (var item in tree)
                {
                    var hierarchy = Parse(item);

                    for (int i = 0; i < hierarchy.Count; i++)
                    {
                        var function = BranchAttribute.CreateDelegate(hierarchy[i].Method, script);

                        list.Add(function);
                    }
                }

                return list;
            }

            public static Hierarchy Parse(Type type)
            {
                if (Dictionary.TryGetValue(type, out var list))
                    return list;

                var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                var methods = type.GetMethods(flags);

                list = new Hierarchy();

                for (int i = 0; i < methods.Length; i++)
                {
                    if (BranchAttribute.TryGet(methods[i], out var attribute) == false)
                        continue;

                    var data = new Data(methods[i], attribute);

                    list.Add(data);
                }

                list.Sort((right, left) => right.Attribute.Line - left.Attribute.Line);

                Dictionary[type] = list;

                return list;
            }

            static Composition()
            {
                Dictionary = new Dictionary<Type, Hierarchy>();
            }
        }

        static Stack<Type> ReadInheritanceTree(Type type)
        {
            var stack = new Stack<Type>();

            while (true)
            {
                if (type == typeof(Script)) break;

                stack.Push(type);

                type = type.BaseType;

                if (type == null) break;
            }

            return stack;
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

        public static Branch.Delegate CreateDelegate(MethodInfo info, object target)
        {
            var type = typeof(Branch.Delegate);

            return info.CreateDelegate(type, target) as Branch.Delegate;
        }
    }
}