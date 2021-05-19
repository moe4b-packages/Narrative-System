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
    [DisallowMultipleComponent]
    [AddComponentMenu(Narrative.Path + "Flowbranch")]
    public class Flowbranch : MonoBehaviour
    {
        public Flowchart Chart { get; protected set; }
        public int Index { get; protected set; }

        public Flowbranch Previous
        {
            get
            {
                if (Chart.Branches.Collection.TryGet(Index - 1, out var value) == false)
                    return null;

                return value;
            }
        }
        public Flowbranch Next
        {
            get
            {
                if (Chart.Branches.Collection.TryGet(Index + 1, out var value) == false)
                    return null;

                return value;
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

            public NodesProperty(Flowbranch branch)
            {
                Collection = QueryComponents.InSelf<FlowNode>(branch);

                for (int i = 0; i < Collection.Length; i++)
                    Collection[i].Set(branch, i);
            }
        }

        const bool EnsureComponentOnTop = true;

        internal void Set(Flowchart flowchart, int index)
        {
            this.Chart = flowchart;
            this.Index = index;

            Nodes = new NodesProperty(this);
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (EnsureComponentOnTop)
            {
                var components = GetComponents<Component>();

                for (int i = 0; i < components.Length; i++)
                    ComponentUtility.MoveComponentUp(this);
            }
        }
#endif
    }
}