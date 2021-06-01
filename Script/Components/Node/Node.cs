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

        internal void Set(Branch reference)
        {
            Branch = reference;
        }

        public Script Script => Branch.Script;

        public event Action OnInvoke;
        public virtual void Invoke()
        {
            OnInvoke?.Invoke();
        }
    }

    public class NodeWaitProperty<T>
    {
        public T Reference { get; protected set; }

        public bool Value { get; protected set; }

        public T Do() => Set(true);
        public T Dont() => Set(false);
        public T Set(bool value)
        {
            Value = value;
            return Reference;
        }

        public NodeWaitProperty(T reference)
        {
            this.Reference = reference;
            Value = true;
        }
    }
}