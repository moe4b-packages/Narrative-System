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
        public string[] Lines { get; protected set; }

        public string Text { get; protected set; }

        public IEnumerable<string> TextForLocalization
        {
            get
            {
                for (int i = 0; i < Lines.Length; i++)
                    yield return Lines[i];
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

        public SayNode(object[] targets, Character character)
        {
            Lines = Array.ConvertAll(targets, x => x.ToString());
            Text = string.Join("", targets);

            this.Character = character;
        }
    }

    partial class Script
    {
        public static  SayNode Say(params object[] lines) => Say(Speaker, lines);
        public static SayNode Say(Character character, params object[] lines) => new SayNode(lines, character);
    }
}