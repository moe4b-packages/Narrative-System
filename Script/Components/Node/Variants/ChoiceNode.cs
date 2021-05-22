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
        List<Entry> entries;
        public class Entry : IChoiceData
        {
            public string Text { get; protected set; }

            public string Branch { get; protected set; }

            public Entry(string text, string branch)
            {
                this.Text = text;
                this.Branch = branch;
            }
        }

        public ChoiceNode Add(Branch.Delegate function)
        {
            var text = function.Method.Name.ToDisplayString();

            return Add(function, text);
        }
        public ChoiceNode Add(Branch.Delegate function, string text)
        {
            var branch = Branch.FormatID(function);

            var item = new Entry(text, branch);

            return Add(item);
        }
        public ChoiceNode Add(Entry entry)
        {
            entries.Add(entry);

            return this;
        }

        public override void Invoke()
        {
            base.Invoke();

            Narrative.Controls.Choice.Show(entries, Submit);
        }

        public ChoiceNode Callback(SubmitDelegate function)
        {
            OnSubmit += function;
            return this;
        }

        public delegate void SubmitDelegate(int index, IChoiceData data);
        public event SubmitDelegate OnSubmit;
        public void Submit(int index, IChoiceData data)
        {
            var entry = entries[index];

            if (Script.Branches.TryGet(entry.Branch, out var branch) == false)
                throw new Exception($"Branch {entry.Branch} Couldn't be found");

            OnSubmit?.Invoke(index, data);

            Script.Continue(branch);
        }

        public ChoiceNode()
        {
            entries = new List<Entry>();
        }
    }

    partial class Script
    {
        protected ChoiceNode Choice()
        {
            var node = new ChoiceNode();

            Register(node);

            return node;
        }
        protected ChoiceNode Choice(params Branch.Delegate[] branches)
        {
            var node = Choice();

            for (int i = 0; i < branches.Length; i++)
                node.Add(branches[i]);

            return node;
        }
    }
}