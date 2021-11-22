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

        public NodeWaitProperty<StartCoroutineNode> Wait { get; private set; }

        protected internal override void Invoke()
        {
            base.Invoke();

            GlobalCoroutine.Start(Procedure);
        }

        IEnumerator Procedure()
        {
            if (Wait.On == false) Playback.Next();

            yield return Numerator;

            if (Wait.On == true) Playback.Next();
        }

        public StartCoroutineNode(IEnumerator numerator)
        {
            this.Numerator = numerator;

            this.Wait = new NodeWaitProperty<StartCoroutineNode>(this);
        }
    }

    partial class Script
    {
        [NarrativeConstructorMethod]
        public static StartCoroutineNode StartCoroutine(Func<IEnumerator> function)
        {
            var numerator = function();

            return StartCoroutine(numerator);
        }

        [NarrativeConstructorMethod]
        public static StartCoroutineNode StartCoroutine(IEnumerator numerator)
        {
            return new StartCoroutineNode(numerator);
        }
    }
}