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
    public class ChoiceNode : Node, ILocalizationTarget
    {
        public Dictionary<IChoiceData, Branch.Delegate> Entries { get; protected set; }

        IEnumerable<string> ILocalizationTarget.TextForLocalization
        {
            get
            {
                foreach (var entry in Entries.Keys)
                    yield return entry.Text;
            }
        }

        public ChoiceNode Add(Branch.Delegate branch)
        {
            var text = Branch.Format.Name(branch);
            return Add(branch, text);
        }
        public ChoiceNode Add(Branch.Delegate branch, string text)
        {
            var entry = new DefaultChoiceData(text);
            return Add(branch, entry);
        }
        public ChoiceNode Add(Branch.Delegate branch, IChoiceData data)
        {
            Entries.Add(data, branch);
            return this;
        }

        protected internal override void Invoke()
        {
            base.Invoke();

            Narrative.Controls.Choice.Show(Entries.Keys, Submit);
        }

        public void Submit(int index, IChoiceData data)
        {
            var branch = Entries[data];

            Playback.Goto(branch);
        }

        public ChoiceNode()
        {
            Entries = new Dictionary<IChoiceData, Branch.Delegate>();
        }
    }

    partial class Script
    {
        public static ChoiceNode Choice() => new ChoiceNode();
        public static ChoiceNode Choice(params Branch.Delegate[] branches)
        {
            var node = Choice();

            for (int i = 0; i < branches.Length; i++)
                node.Add(branches[i]);

            return node;
        }
    }
}