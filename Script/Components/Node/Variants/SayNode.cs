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
    public class SayNode : Node, ISayData
    {
        public Character Character { get; set; }

        public string Text { get; set; }

        public bool AutoSubmit { get; set; }
        public SayNode SetAutoSubmit() => SetAutoSubmit(true);
        public SayNode SetAutoSubmit(bool value)
        {
            AutoSubmit = value;
            return this;
        }

        public override void Invoke()
        {
            base.Invoke();

            Narrative.Controls.Say.Show(this, Submit);
        }

        void Submit() => Script.Continue();
    }

    partial class Script
    {
        public SayNode Say() => Say("", null).SetAutoSubmit(true);

        public SayNode Say(string text, Character character = null)
        {
            if (character == null) character = SpeakingCharacter;

            return new SayNode()
            {
                Text = text,
                Character = character,
            };
        }
    }
}