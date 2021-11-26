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
    public class ChoiceNode : Node, IChoiceData
    {
        public List<Entry> Entries { get; }
        IChoiceEntry IChoiceData.Retrieve(int index) => Entries[index];
        int IChoiceData.Count => Entries.Count;
        public class Entry : IChoiceEntry
        {
            public string Text { get; }

            public Branch.Delegate Branch { get; }
            public Action Callback { get; }

            public Entry(string text, Branch.Delegate branch, Action callback)
            {
                this.Text = text;
                this.Branch = branch;
                this.Callback = callback;
            }
        }

        public NodeTextFormatProperty<ChoiceNode> Format { get; }
        ILocalizationFormat IChoiceData.Format => Format;

        [NarrativeConstructorMethod]
        public ChoiceNode Add([LocalizationTextParameter] string text, Branch.Delegate branch) => Add(text, branch, default);

        [NarrativeConstructorMethod]
        public ChoiceNode Add([LocalizationTextParameter] string text, Action callback) => Add(text, default, callback);

        [NarrativeConstructorMethod]
        public ChoiceNode Add([LocalizationTextParameter] string text, Branch.Delegate branch, Action callback)
        {
            var entry = new Entry(text, branch, callback);
            Entries.Add(entry);
            return this;
        }

        protected internal override void Invoke()
        {
            base.Invoke();

            if (Entries.Count == 0)
                throw new Exception("Choice Node Has 0 Choices Submitted");

            Narrative.Controls.Choice.Show(this);
        }

        public void Submit(int index, IChoiceEntry entry)
        {
            Entries[index].Callback?.Invoke();

            if (Entries[index].Branch == null)
                Playback.Next();
            else
                Playback.Goto(Entries[index].Branch);
        }
        ChoiceSubmitDelegate IChoiceData.Callback => Submit;

        public ChoiceNode()
        {
            Entries = new List<Entry>();

            Format = new NodeTextFormatProperty<ChoiceNode>(this);
        }
    }

    partial class Script
    {
        [NarrativeConstructorMethod]
        public static ChoiceNode Choice() => new ChoiceNode();
    }
}