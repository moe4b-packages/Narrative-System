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
    public class StartCoroutineNode : Node
    {
        IEnumerator numerator;

        bool wait = true;

        public StartCoroutineNode DoWait() => SetWait(true);
        public StartCoroutineNode DontWait() => SetWait(false);
        public StartCoroutineNode SetWait(bool value)
        {
            wait = value;
            return this;
        }

        public override void Invoke()
        {
            base.Invoke();

            GlobalCoroutine.Start(Procedure);

            if (wait == false) Script.Continue();
        }

        IEnumerator Procedure()
        {
            yield return numerator;

            if (wait) Script.Continue();
        }

        public StartCoroutineNode(IEnumerator numerator)
        {
            this.numerator = numerator;
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