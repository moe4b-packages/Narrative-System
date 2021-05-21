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

using MB.NarrativeSystem;

namespace MB.NarrativeSystem
{
    public class Branch
    {
        public string ID { get; protected set; }

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
            public List<Node> List { get; protected set; }

            public Node this[int index] => List[index];
            public int Count => List.Count;

            public Node First => List.SafeIndexer(0);
            public Node Last => List.SafeIndexer(List.Count - 1);

            public NodesProperty(Branch branch, IEnumerable collection)
            {
                List = GetNodes(collection);

                for (int i = 0; i < List.Count; i++)
                    List[i].Set(branch, i);
            }

            static List<Node> GetNodes(IEnumerable collection)
            {
                var list = new List<Node>();

                GetNodes(collection, ref list);

                return list;
            }
            static void GetNodes(IEnumerable collection, ref List<Node> list)
            {
                foreach (var item in collection)
                {
                    if (item is Node node)
                        list.Add(node);
                    else if (item is IEnumerable other)
                        GetNodes(other, ref list);
                }
            }
        }

        internal void Set(Script script, int index)
        {
            this.Script = script;
            this.Index = index;
        }

        public Branch(string id, IEnumerable nodes)
        {
            this.ID = id;
            this.Nodes = new NodesProperty(this, nodes);
        }

        public delegate IEnumerable Delegate();

        //Static Utility

        public static Branch From(Delegate function)
        {
            var name = FormatID(function);
            var nodes = function();

            var branch = new Branch(name, nodes);

            return branch;
        }

        public static string FormatID(Delegate function) => function.Method.Name;
    }
}