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
    public class ChoiceNode : Node
    {
        public Dictionary<IChoiceData, Entry> Entries { get; protected set; }
        public class Entry
        {
            public Branch.Delegate Branch { get; }
            public Action Callback { get; }

            public Entry(Branch.Delegate branch, Action callback)
            {
                this.Branch = branch;
                this.Callback = callback;
            }
        }

        [NarrativeConstructorMethod]
        public ChoiceNode Add([LocalizationParameter] string text, Branch.Delegate branch) => Add(text, branch, default);

        [NarrativeConstructorMethod]
        public ChoiceNode Add([LocalizationParameter] string text, Action callback) => Add(text, default, callback);

        [NarrativeConstructorMethod]
        public ChoiceNode Add([LocalizationParameter] string text, Branch.Delegate branch, Action callback)
        {
            var data = new DefaultChoiceData(text);
            var entry = new Entry(branch, callback);
            Entries.Add(data, entry);
            return this;
        }

        protected internal override void Invoke()
        {
            base.Invoke();

            if (Entries.Count == 0)
                throw new Exception("Choice Node Has 0 Choices Submitted");

            Narrative.Controls.Choice.Show(Entries.Keys, Submit);
        }

        public void Submit(int index, IChoiceData data)
        {
            var entry = Entries[data];

            entry.Callback?.Invoke();

            if (entry.Branch == null)
                Playback.Next();
            else
                Playback.Goto(entry.Branch);
        }

        public ChoiceNode()
        {
            Entries = new Dictionary<IChoiceData, Entry>();
        }
    }

    partial class Script
    {
        [NarrativeConstructorMethod]
        public static ChoiceNode Choice() => new ChoiceNode();
    }
}