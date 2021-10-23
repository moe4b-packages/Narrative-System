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
        public Script Script => Branch.Script;

        internal void Set(Branch branch)
        {
            this.Branch = branch;
        }

        public event Action OnInvoke;
        public virtual void Invoke()
        {
            OnInvoke?.Invoke();
        }

        public Node()
        {

        }
    }

    public interface IWaitNode<TSelf>
    {
        bool Wait { get; }

        /// <summary>
        /// Determine if this Node will Wait to Complete or not Before Movin to the Next Node
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        TSelf SetWait(bool value);

        /// <summary>
        /// Wait for this Node to Complete before Moving to the Next Node
        /// </summary>
        /// <returns></returns>
        TSelf Await();

        /// <summary>
        /// Continue to the Next Node Without Waiting for this Node to Complete
        /// </summary>
        /// <returns></returns>
        TSelf Continue();
    }
}