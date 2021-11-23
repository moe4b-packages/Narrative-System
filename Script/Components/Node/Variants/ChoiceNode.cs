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

        int registerations = 0;

        [NarrativeConstructorMethod]
        public Builder Register([LocalizationParameter] string text)
        {
            registerations += 1;

            var builder = new Builder(this, text);
            return builder;
        }
        [NarrativeConstructorMethod]
        public Builder Register([LocalizationParameter] Branch.Delegate branch)
        {
            registerations += 1;

            var text = Branch.Format.Name(branch);

            var builder = new Builder(this, text);
            builder.Branch(branch);
            return builder;
        }
        public struct Builder
        {
            readonly ChoiceNode node;

            readonly string text;
            Branch.Delegate branch;
            Action callback;

            [NarrativeConstructorMethod]
            public Builder Branch(Branch.Delegate value)
            {
                branch = value;
                return this;
            }
            [NarrativeConstructorMethod]
            public Builder Callback(Action value)
            {
                callback = value;
                return this;
            }

            [NarrativeConstructorMethod]
            public ChoiceNode Submit()
            {
                node.Add(branch, text, callback);
                node.registerations -= 1;
                return node;
            }

            public Builder(ChoiceNode node, string text)
            {
                this.node = node;
                this.text = text;

                branch = default;
                callback = default;
            }
        }

        [NarrativeConstructorMethod]
        public ChoiceNode Add([LocalizationParameter] Branch.Delegate branch)
        {
            var text = Branch.Format.Name(branch);
            return Add(branch, text);
        }
        [NarrativeConstructorMethod]
        public ChoiceNode Add(Branch.Delegate branch, [LocalizationParameter] string text) => Add(branch, text, default);
        [NarrativeConstructorMethod]
        public ChoiceNode Add(Branch.Delegate branch, [LocalizationParameter] string text, Action callback)
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

            if (registerations > 0)
                Debug.LogWarning($"{registerations} Un-Submitted Choice Detected on Choice Node");

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

        [NarrativeConstructorMethod]
        public static ChoiceNode Choice([LocalizationParameter] params Branch.Delegate[] branches)
        {
            var node = Choice();

            for (int i = 0; i < branches.Length; i++)
                node.Add(branches[i]);

            return node;
        }
    }
}