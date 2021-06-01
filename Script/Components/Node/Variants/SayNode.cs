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
    public class SayNode : Node, ISayData, ILocalizationTarget
    {
        public string Text { get; protected set; }

        public Character Character { get; protected set; }

        public bool AutoSubmit { get; set; }
        public SayNode SetAutoSubmit(bool value = true)
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

        public IEnumerable<string> RetrieveLocalization()
        {
            yield return Text;
        }

        public SayNode(string text, Character character)
        {
            this.Text = text;
            this.Character = character;
        }
    }

    partial class Script
    {
        protected SayNode Say() => Say(string.Empty, null).SetAutoSubmit(true);

        protected SayNode Say(string text) => Say(text, SpeakingCharacter);
        protected SayNode Say(string text, Character character) => new SayNode(text, character);
    }
}