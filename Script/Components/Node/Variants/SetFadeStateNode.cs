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
    public class SetFadeStateNode : Node
    {
        public bool IsOn { get; set; }

        public override void Invoke()
        {
            base.Invoke();

            Narrative.Controls.Fader.SetState(IsOn);

            Script.Continue();
        }

        public SetFadeStateNode(bool isOn)
        {
            this.IsOn = isOn;
        }
    }

    partial class Script
    {
        protected SetFadeStateNode SetFadeState(bool isOn) => new SetFadeStateNode(isOn);
    }
}