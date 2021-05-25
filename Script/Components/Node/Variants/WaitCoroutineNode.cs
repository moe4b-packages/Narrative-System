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
    public class WaitCoroutineNode : Node
    {
        IEnumerator numerator;

        public override void Invoke()
        {
            base.Invoke();

            GlobalCoroutine.Start(Procedure);
        }

        IEnumerator Procedure()
        {
            yield return numerator;

            Script.Continue();
        }

        public WaitCoroutineNode(IEnumerator numerator)
        {
            this.numerator = numerator;
        }
    }

    partial class Script
    {
        protected WaitCoroutineNode WaitCoroutine(IEnumerator numerator) => new WaitCoroutineNode(numerator);
        protected WaitCoroutineNode WaitCoroutine(Func<IEnumerator> function)
        {
            var numerator = function();

            return WaitCoroutine(numerator);
        }
    }
}