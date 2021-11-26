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

using MB.LocalizationSystem;

namespace MB.NarrativeSystem
{
    public class SayNode : Node, ISayData
    {
        public string Text { get; protected set; }
        public Character Character { get; protected set; }

        public NodeTextFormatProperty<SayNode> Format { get; }

        public Dictionary<string, string> GetPhrases() => Format.Dictionary;

        public bool AutoSubmit { get; set; }
        [NarrativeConstructorMethod]
        public SayNode SetAutoSubmit(bool value = true)
        {
            AutoSubmit = value;
            return this;
        }

        protected internal override void Invoke()
        {
            base.Invoke();

            Narrative.Controls.Say.Show(this, Submit);
        }

        void Submit() => Playback.Next();

        public SayNode(Character character, string text)
        {
            this.Character = character;
            this.Text = text;

            Format = new NodeTextFormatProperty<SayNode>(this);
        }
    }

    partial class Script
    {
        [NarrativeConstructorMethod]
        public static SayNode Say([LocalizationTextParameter] string text) => Say(text, Speaker);

        [NarrativeConstructorMethod]
        public static SayNode Say([LocalizationTextParameter] string text, Character character) => new SayNode(character, text);
    }
}