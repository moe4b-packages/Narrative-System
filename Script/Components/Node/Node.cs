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
    public class Node
    {
        public Branch Branch { get; protected set; }
        public int Index { get; protected set; }

        public Script Script => Branch.Script;

        public Node Previous
        {
            get
            {
                if (Script.Nodes.Collection.TryGet(Index - 1, out var value) == false)
                    return null;

                return value;
            }
        }
        public Node Next
        {
            get
            {
                if (Script.Nodes.Collection.TryGet(Index + 1, out var value) == false)
                    return null;

                return value;
            }
        }

        internal void Set(Branch branch, int index)
        {
            this.Branch = branch;
            this.Index = index;
        }

        public event Action OnInvoke;
        public virtual void Invoke()
        {
            OnInvoke?.Invoke();
        }
    }
}