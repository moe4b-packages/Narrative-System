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
        public float? Duration { get; protected set; }

        public override void Invoke()
        {
            base.Invoke();

            GlobalCoroutine.Start(Procedure);
        }

        IEnumerator Procedure()
        {
            yield return Narrative.Controls.Fader.Hide(duration: Duration);

            Narrative.Player.Continue();
        }

        public FadeOutNode(float? duration)
        {
            this.Duration = duration;
        }
    }

    partial class Script
    {
        public static FadeOutNode FadeOut(float? duration = null) => new FadeOutNode(duration);
    }
}