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

using MB.NarrativeSystem;

namespace MB.NarrativeSystem
{
    public class Node
    {
        public Branch Branch { get; protected set; }
        public int Index { get; protected set; }

        public Script Script => Branch.Script;

        public Node Previous
        {
            get
            {
                if (Script.Nodes.Collection.TryGet(Index - 1, out var value) == false)
                    return null;

                return value;
            }
        }
        public Node Next
        {
            get
            {
                if (Script.Nodes.Collection.TryGet(Index + 1, out var value) == false)
                    return null;

                return value;
            }
        }

        internal void Set(Branch branch, int index)
        {
            this.Branch = branch;
            this.Index = index;
        }

        public event Action OnInvoke;
        public virtual void Invoke()
        {
            OnInvoke?.Invoke();
        }
    }

    #region Fade
    public class SetFadeState : Node
    {
        bool isOn { get; set; }

        public override void Invoke()
        {
            base.Invoke();

            Narrative.Controls.Fader.SetState(isOn);

            Script.Continue();
        }

        public SetFadeState()
        {
            isOn = true;
        }

        public static SetFadeState On => new SetFadeState() { isOn = true };
        public static SetFadeState Off => new SetFadeState() { isOn = false };
    }

    public class FadeIn : Node
    {
        public float? Duration { get; protected set; }

        public override void Invoke()
        {
            base.Invoke();

            GlobalCoroutine.Start(Procedure);
        }

        IEnumerator Procedure()
        {
            yield return Narrative.Controls.Fader.Show(duration: Duration);

            Script.Continue();
        }

        public static FadeIn Compose(float? duration = null) => new FadeIn() { Duration = duration };
    }

    public class FadeOut : Node
    {
        public float? Duration { get; protected set; }

        public override void Invoke()
        {
            base.Invoke();

            GlobalCoroutine.Start(Procedure);
        }

        IEnumerator Procedure()
        {
            yield return Narrative.Controls.Fader.Hide(duration: Duration);

            Script.Continue();
        }

        public static FadeOut Compose(float? duration = null) => new FadeOut() { Duration = duration };
    }
    #endregion

    public class Delay : Node
    {
        float Duration { get; set; }

        public override void Invoke()
        {
            base.Invoke();

            GlobalCoroutine.Start(Procedure);
        }

        IEnumerator Procedure()
        {
            yield return new WaitForSeconds(Duration);

            Script.Continue();
        }

        public Delay()
        {
            Duration = 1f;
        }

        public static Delay Compose(float duration = 1f) => new Delay() { Duration = duration };
    }

    public class Say : Node, ISayData
    {
        public Character Character { get; set; }

        public string Text { get; set; }

        public bool AutoSubmit { get; set; }
        public Say SetAutoSubmit() => SetAutoSubmit(true);
        public Say SetAutoSubmit(bool value)
        {
            AutoSubmit = value;
            return this;
        }

        public override void Invoke()
        {
            base.Invoke();

            Narrative.Controls.Say.Show(this, Submit);
        }

        void Submit() => Script.Continue();

        public static Say Clear => Compose("", null).SetAutoSubmit(true);

        public static Say Compose(string text, Character character = null)
        {
            return new Say()
            {
                Text = text,
                Character = character,
            };
        }
    }

    public class Choice : Node
    {
        public List<Entry> Entries { get; protected set; }
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

        public Choice Add(Branch.Delegate function)
        {
            var text = function.Method.Name.ToDisplayString();

            return Add(function, text);
        }
        public Choice Add( Branch.Delegate function, string text)
        {
            var branch = Branch.FormatID(function);

            var item = new Entry(text, branch);
            Entries.Add(item);

            return this;
        }

        public override void Invoke()
        {
            base.Invoke();

            Narrative.Controls.Choice.Show(Entries, Submit);
        }

        public Choice Callback(SubmitDelegate function)
        {
            OnSubmit += function;
            return this;
        }

        public delegate void SubmitDelegate(int index, IChoiceData data);
        public event SubmitDelegate OnSubmit;
        public void Submit(int index, IChoiceData data)
        {
            var entry = Entries[index];

            if (Script.Branches.TryGet(entry.Branch, out var branch) == false)
                throw new Exception($"Branch {entry.Branch} Couldn't be found");

            OnSubmit?.Invoke(index, data);

            Script.Continue(branch);
        }

        public Choice()
        {
            Entries = new List<Entry>();
        }

        public static Choice Compose()
        {
            var choice = new Choice();

            return choice;
        }
        public static Choice Compose(params Branch.Delegate[] branches)
        {
            var entries = new List<Entry>(branches.Length);

            for (int i = 0; i < branches.Length; i++)
            {
                var text = branches[i].Method.Name.ToDisplayString();
                var branch = Branch.FormatID(branches[i]);

                var item = new Entry(text, branch);
                entries.Add(item);
            }

            var choice = new Choice() { Entries = entries };

            return choice;
        }
    }

    public class StopFlow : Node
    {
        public override void Invoke()
        {
            base.Invoke();

            Script.Stop();
        }

        public static StopFlow New => new StopFlow();
    }

    public class InvokeFlowchart : Node
    {
        [SerializeField]
        Script target;

        public override void Invoke()
        {
            base.Invoke();

            target.Invoke();

            if (target != Script) Script.Continue();
        }

        public static InvokeFlowchart Compose(Script target) => new InvokeFlowchart() { target = target };
    }
}