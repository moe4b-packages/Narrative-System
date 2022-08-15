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
        public const float DefaultDuration = 0.5f;

        public float Duration { get; protected set; }

        public NodeWaitProperty<FadeInNode> Wait { get; }

        protected internal override void Invoke()
        {
            base.Invoke();

            MRoutine.Create(Procedure).Start();
        }

        IEnumerator Procedure()
        {
            if (Wait.On == false) Playback.Next();

            yield return Narrative.Controls.Fader.Show(Duration);

            if (Wait.On == true) Playback.Next();
        }

        public FadeInNode(float duration)
        {
            this.Duration = duration;

            Wait = new NodeWaitProperty<FadeInNode>(this);
        }
    }

    public class FadeOutNode : Node
    {
        public const float DefaultDuration = 0.5f;

        public float Duration { get; protected set; }

        public NodeWaitProperty<FadeOutNode> Wait { get; }

        protected internal override void Invoke()
        {
            base.Invoke();

            MRoutine.Create(Procedure).Start();
        }

        IEnumerator Procedure()
        {
            if (Wait.On == false) Playback.Next();

            yield return Narrative.Controls.Fader.Hide(Duration);

            if (Wait.On == true) Playback.Next();
        }

        public FadeOutNode(float duration)
        {
            this.Duration = duration;

            Wait = new NodeWaitProperty<FadeOutNode>(this);
        }
    }

    public class SetFadeStateNode : Node
    {
        public bool IsOn { get; set; }

        protected internal override void Invoke()
        {
            base.Invoke();

            Narrative.Controls.Fader.SetState(IsOn);

            Playback.Next();
        }

        public SetFadeStateNode(bool isOn)
        {
            this.IsOn = isOn;
        }
    }

    partial class Script
    {
        [NarrativeConstructorMethod]
        public static FadeInNode FadeIn(float duration = FadeInNode.DefaultDuration) => new FadeInNode(duration);

        [NarrativeConstructorMethod]
        public static FadeOutNode FadeOut(float duration = FadeOutNode.DefaultDuration) => new FadeOutNode(duration);

        [NarrativeConstructorMethod]
        public static SetFadeStateNode SetFadeState(bool isOn) => new SetFadeStateNode(isOn);
    }
}