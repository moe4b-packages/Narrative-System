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
    public class StartCoroutineNode : Node, IWaitNode<StartCoroutineNode>
    {
        public IEnumerator Numerator { get; protected set; }

        #region Wait
        public bool Wait { get; protected set; } = true;

        public StartCoroutineNode SetWait(bool value)
        {
            Wait = value;
            return this;
        }

        public StartCoroutineNode Await() => SetWait(true);
        public StartCoroutineNode Continue() => SetWait(false);
        #endregion

        public override void Invoke()
        {
            base.Invoke();

            GlobalCoroutine.Start(Procedure);

            if (Wait == false) Narrative.Player.Continue();
        }

        IEnumerator Procedure()
        {
            yield return Numerator;

            if (Wait) Narrative.Player.Continue();
        }

        public StartCoroutineNode(IEnumerator numerator)
        {
            this.Numerator = numerator;
        }
    }

    partial class Script
    {
        protected StartCoroutineNode StartCoroutine(Func<IEnumerator> function)
        {
            var numerator = function();

            return StartCoroutine(numerator);
        }
        protected StartCoroutineNode StartCoroutine(IEnumerator numerator)
        {
            return new StartCoroutineNode(numerator);
        }
    }
}