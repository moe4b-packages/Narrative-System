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
        public Character Character { get; protected set; }
        public string Text { get; protected set; }

        IEnumerable<string> ILocalizationTarget.TextForLocalization
        {
            get
            {
                yield return Text;
            }
        }

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
        }
    }

    partial class Script
    {
        [NarrativeConstructorMethod]
        public static SayNode Say(string text) => Say(Speaker, text);

        [NarrativeConstructorMethod]
        public static SayNode Say(Character character, string text) => new SayNode(character, text);
    }
}