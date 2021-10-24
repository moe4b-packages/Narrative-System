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

        public IEnumerable<string> TextForLocalization
        {
            get
            {
                yield return Text;
            }
        }

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

        void Submit() => Narrative.Player.Continue();

        public SayNode(Character character, string text)
        {
            this.Character = character;
            this.Text = text;
        }
    }

    partial class Script
    {
        public static SayNode Say(string text) => Say(Speaker, text);
        public static SayNode Say(Character character, string text) => new SayNode(character, text);
    }
}