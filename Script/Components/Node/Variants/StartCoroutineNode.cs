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
        public IEnumerator Numerator { get; protected set; }

        public NodeWaitProperty<StartCoroutineNode> Wait { get; protected set; }

        public override void Invoke()
        {
            base.Invoke();

            GlobalCoroutine.Start(Procedure);

            if (Wait.Value == false) Script.Continue();
        }

        IEnumerator Procedure()
        {
            yield return Numerator;

            if (Wait.Value) Script.Continue();
        }

        public StartCoroutineNode(IEnumerator numerator)
        {
            this.Numerator = numerator;

            Wait = new NodeWaitProperty<StartCoroutineNode>(this);
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