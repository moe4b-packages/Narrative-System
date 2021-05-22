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
        float duration;

        public override void Invoke()
        {
            base.Invoke();

            GlobalCoroutine.Start(Procedure);
        }

        IEnumerator Procedure()
        {
            yield return new WaitForSeconds(duration);

            Script.Continue();
        }

        public DelayNode(float duration)
        {
            this.duration = duration;
        }
    }

    partial class Script
    {
        protected DelayNode Delay(float duration = 1f) => new DelayNode(duration);
    }
}