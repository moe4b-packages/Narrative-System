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
    [AddComponentMenu(Narrative.Path + "Flowchart")]
	public class Flowchart : MonoBehaviour
	{
        public BranchesProperty Branches { get; protected set; }
        [Serializable]
        public class BranchesProperty
        {
            public Flowbranch[] Collection { get; protected set; }

            public Flowbranch this[int index] => Collection[index];
            public int Count => Collection.Length;

            public Flowbranch First => Collection.Length == 0 ? null : Collection[0];
            public Flowbranch Last => Collection.Length == 0 ? null : Collection[Collection.Length - 1];

            public int NodeCapacity
            {
                get
                {
                    var value = 0;

                    for (int i = 0; i < Collection.Length; i++)
                        value += Collection[i].Nodes.Count;

                    return value;
                }
            }

            public BranchesProperty(Flowchart chart)
            {
                Collection = QueryComponents.InChildren<Flowbranch>(chart);

                for (int i = 0; i < Collection.Length; i++)
                    Collection[i].Set(chart, i);
            }
        }

        public NodesProperty Nodes { get; protected set; }
        [Serializable]
        public class NodesProperty
        {
            public FlowNode[] Collection { get; protected set; }

            public FlowNode this[int index] => Collection[index];
            public int Count => Collection.Length;

            public FlowNode First => Collection.Length == 0 ? null : Collection[0];
            public FlowNode Last => Collection.Length == 0 ? null : Collection[Collection.Length - 1];

            public NodesProperty(BranchesProperty branches)
            {
                Collection = new FlowNode[branches.NodeCapacity];

                var index = 0;

                for (int b = 0; b < branches.Count; b++)
                {
                    for (int n = 0; n < branches[b].Nodes.Count; n++)
                    {
                        Collection[index] = branches[b].Nodes[n];
                        Collection[index].Set(branches[b], index);

                        index += 1;
                    }
                }
            }
        }

        public int Progress { get; internal set; }

        public FlowNode Selection
        {
            get => Nodes[Progress];
            set
            {
                if (value == null)
                    Progress = 0;
                else
                    Progress = value.Index;
            }
        }

        void Awake()
        {
            Branches = new BranchesProperty(this);

            Nodes = new NodesProperty(Branches);
        }

        public void Invoke() => Invoke(0);
        public void Invoke(int progress)
        {
            if (Nodes.Collection.TryGet(progress, out var node) == false)
                throw new Exception($"Invalid Progress of {node} Loaded on '{this}'");

            Invoke(node);
        }

        public delegate void InvokeDelegate(FlowNode node);
        public event InvokeDelegate OnInvoke;
        void Invoke(FlowNode node)
        {
            Selection = node;
            Selection.Invoke();

            OnInvoke?.Invoke(node);
        }

        public void Continue()
        {
            if (Selection.Next == null)
                End();
            else
                Invoke(Selection.Next);
        }
        public void Continue(FlowNode node)
        {
            Invoke(node);
        }

        public void Stop()
        {
            End();
        }

        public event Action OnEnd;
        protected void End()
        {
            Selection = null;

            OnEnd?.Invoke();
        }
    }
}