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
        List<Entry> entires;
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
            entires.Add(item);

            return this;
        }

        public override void Invoke()
        {
            base.Invoke();

            Narrative.Controls.Choice.Show(entires, Submit);
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
            var entry = entires[index];

            if (Script.Branches.TryGet(entry.Branch, out var branch) == false)
                throw new Exception($"Branch {entry.Branch} Couldn't be found");

            OnSubmit?.Invoke(index, data);

            Script.Continue(branch);
        }

        public ChoiceNode() : this(new List<Entry>()) { }
        public ChoiceNode(List<Entry> entires)
        {
            this.entires = entires;
        }
    }

    partial class Script
    {
        public ChoiceNode Choice()
        {
            var choice = new ChoiceNode();

            return choice;
        }
        public ChoiceNode Choice(params Branch.Delegate[] branches)
        {
            var entries = new List<ChoiceNode.Entry>(branches.Length);

            for (int i = 0; i < branches.Length; i++)
            {
                var text = branches[i].Method.Name.ToDisplayString();
                var branch = Branch.FormatID(branches[i]);

                var item = new ChoiceNode.Entry(text, branch);
                entries.Add(item);
            }

            var choice = new ChoiceNode(entries);

            return choice;
        }
    }
}