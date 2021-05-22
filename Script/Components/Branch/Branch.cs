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

        public static class Composition
        {
            public static Dictionary<Type, Collection> Dictionary { get; private set; }

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

            public class Collection : List<Data> { }

            public static List<Delegate> Read(Script script)
            {
                var type = script.GetType();

                if (Dictionary.TryGetValue(type, out var collection) == false)
                {
                    collection = ComposeAll(type);
                    Dictionary[type] = collection;
                }

                return collection.Select(CreateDelegate).ToList();

                Delegate CreateDelegate(Data data) => BranchAttribute.CreateDelegate(data.Method, script);
            }

            static Collection ComposeAll(Type type)
            {
                var tree = ReadInheritanceTree(type);

                var list = new Collection();

                foreach (var member in tree)
                {
                    var data = Iterate(member);

                    list.AddRange(data);
                }

                return list;
            }

            static Collection Iterate(Type type)
            {
                var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                var methods = type.GetMethods(flags);

                var list = new Collection();

                for (int i = 0; i < methods.Length; i++)
                {
                    if (BranchAttribute.TryGet(methods[i], out var attribute) == false)
                        continue;

                    var data = new Data(methods[i], attribute);

                    list.Add(data);
                }

                list.Sort((right, left) => right.Attribute.Line - left.Attribute.Line);

#if UNITY_EDITOR || DEBUG
                var pathes = list.Select(x => x.Attribute.Path).Distinct();

                if (pathes.Count() > 1)
                    throw new Exception($"Multiple Branches Detected in Multiple Files of Partial Class for {type}, " +
                        $"This is not Supported");
#endif

                return list;
            }

            static Composition()
            {
                Dictionary = new Dictionary<Type, Collection>();
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