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

        public IEnumerable<string> TextForLocalization
        {
            get
            {
                foreach (var entry in Entries.Keys)
                    yield return entry.Text;
            }
        }

        public ChoiceNode Add(Branch.Delegate branch)
        {
            var text = branch.Method.Name.ToDisplayString();

            return Add(branch, text);
        }
        public ChoiceNode Add(Branch.Delegate branch, string text)
        {
            var entry = new DefaultChoiceEntry(text);

            return Add(entry, branch);
        }
        public ChoiceNode Add(IChoiceData entry, Branch.Delegate branch)
        {
            Entries.Add(entry, branch);

            return this;
        }

        public override void Invoke()
        {
            base.Invoke();

            Narrative.Controls.Choice.Show(Entries.Keys, Submit);
        }

        public ChoiceNode Callback(SubmitDelegate function)
        {
            OnSubmit += Surrogate;

            void Surrogate(IChoiceData data)
            {
                OnSubmit -= Surrogate;

                function(data);
            }

            return this;
        }

        public event SubmitDelegate OnSubmit;
        public delegate void SubmitDelegate(IChoiceData data);
        public void Submit(int index, IChoiceData data)
        {
            var branch = Entries[data];

            OnSubmit?.Invoke(data);

            Script.Invoke(branch);
        }

        public ChoiceNode()
        {
            Entries = new Dictionary<IChoiceData, Branch.Delegate>();
        }
    }

    public class DefaultChoiceEntry : IChoiceData
    {
        public string Text { get; protected set; }

        public DefaultChoiceEntry(string text)
        {
            this.Text = text;
        }
    }

    partial class Script
    {
        protected ChoiceNode Choice() => new ChoiceNode();
        protected ChoiceNode Choice(params Branch.Delegate[] branches)
        {
            var node = Choice();

            for (int i = 0; i < branches.Length; i++)
                node.Add(branches[i]);

            return node;
        }
    }
}