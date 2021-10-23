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
    public class FadeInNode : Node
    {
        public float? Duration { get; protected set; }

        public override void Invoke()
        {
            base.Invoke();

            GlobalCoroutine.Start(Procedure);
        }

        IEnumerator Procedure()
        {
            yield return Narrative.Controls.Fader.Show(duration: Duration);

            Narrative.Player.Continue();
        }

        public FadeInNode(float? duration)
        {
            this.Duration = duration;
        }
    }

    partial class Script
    {
        protected FadeInNode FadeIn(float? duration = null) => new FadeInNode(duration);
    }
}