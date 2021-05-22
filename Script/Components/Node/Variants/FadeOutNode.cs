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
    public class FadeOutNode : Node
    {
        float? duration;

        public override void Invoke()
        {
            base.Invoke();

            GlobalCoroutine.Start(Procedure);
        }

        IEnumerator Procedure()
        {
            yield return Narrative.Controls.Fader.Hide(duration: duration);

            Script.Continue();
        }

        public FadeOutNode(float? duration)
        {
            this.duration = duration;
        }
    }

    partial class Script
    {
        protected FadeOutNode FadeOut(float? duration = null)
        {
            var node = new FadeOutNode(duration);

            Register(node);

            return node;
        }
    }
}