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
        string Text { get; }
        string ISayData.Text => Text;

        Character Character { get; }
        Character ISayData.Character => Character;

        public NodeTextFormatProperty<SayNode> Format { get; }
        ILocalizationFormat ISayData.Format => Format;

        bool AutoSubmit { get; set; }
        bool ISayData.AutoSubmit => AutoSubmit;

        [NarrativeConstructorMethod]
        public SayNode SetAutoSubmit(bool value = true)
        {
            AutoSubmit = value;
            return this;
        }

        protected internal override void Invoke()
        {
            base.Invoke();

            Narrative.Controls.Say.Show(this);
        }

        void Submit() => Playback.Next();
        Action ISayData.Callback => Submit;

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