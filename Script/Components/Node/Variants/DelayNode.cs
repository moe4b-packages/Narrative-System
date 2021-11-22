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
    public class DelayNode : Node
    {
        public float Duration { get; protected set; }

        protected internal override void Invoke()
        {
            base.Invoke();

            GlobalCoroutine.Start(Procedure);
        }

        IEnumerator Procedure()
        {
            yield return new WaitForSeconds(Duration);

            Playback.Next();
        }

        public DelayNode(float duration)
        {
            this.Duration = duration;
        }
    }

    partial class Script
    {
        [NarrativeConstructorMethod]
        public static DelayNode Delay(float duration = 1f) => new DelayNode(duration);
    }
}