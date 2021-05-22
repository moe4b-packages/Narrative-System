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

            public Branch.Delegate Branch { get; protected set; }

            public Entry(string text, Branch.Delegate branch)
            {
                this.Text = text;
                this.Branch = branch;
            }
        }

        public ChoiceNode Add(Branch.Delegate branch)
        {
            var text = branch.Method.Name.ToDisplayString();

            return Add(branch, text);
        }
        public ChoiceNode Add(Branch.Delegate branch, string text)
        {
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

        public delegate void SimpleSubmitDelegate(int index);
        public ChoiceNode Callback(SimpleSubmitDelegate function)
        {
            OnSubmit += Surrogate;

            void Surrogate(int index, IChoiceData data) => function?.Invoke(index);

            return this;
        }

        public delegate void ComplexSubmitDelegate(int index, IChoiceData data);
        public ChoiceNode Callback(ComplexSubmitDelegate function)
        {
            OnSubmit += function;
            return this;
        }

        public ChoiceNode Callback<T>(Action<T> function, params T[] options)
            where T : Enum
        {
            Callback(Surrogate);

            void Surrogate(int index, IChoiceData data)
            {
                var value = options[index];

                function(value);
            }

            return this;
        }

        public event ComplexSubmitDelegate OnSubmit;
        public void Submit(int index, IChoiceData data)
        {
            var entry = entries[index];

            OnSubmit?.Invoke(index, data);

            Script.Continue(entry.Branch);
        }

        public ChoiceNode()
        {
            entries = new List<Entry>();
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