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
        float? duration;

        public override void Invoke()
        {
            base.Invoke();

            GlobalCoroutine.Start(Procedure);
        }

        IEnumerator Procedure()
        {
            yield return Narrative.Controls.Fader.Show(duration: duration);

            Script.Continue();
        }

        public FadeInNode(float? duration)
        {
            this.duration = duration;
        }
    }

    partial class Script
    {
        protected FadeInNode FadeIn(float? duration = null) => new FadeInNode(duration);
    }
}