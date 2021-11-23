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
using System.Text;

namespace MB.NarrativeSystem
{
    public class Node
    {
        protected internal virtual void Invoke()
        {

        }

        protected static class Playback
        {
            public static void Next() => Narrative.Player.Continue();
            public static void Goto(Branch.Delegate branch) => Narrative.Player.Invoke(branch);

            public static void Start(Script script) => Narrative.Player.Start(script);
            public static void Stop() => Narrative.Player.Stop();
        }

        public Node()
        {

        }
    }

    public class NodeWaitProperty<TSelf>
        where TSelf : Node
    {
        TSelf Self { get; }

        public bool On { get; private set; }

        /// <summary>
        /// Determine if this Node will Wait to Complete or not Before Movin to the Next Node
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [NarrativeConstructorMethod]
        public TSelf SetWait(bool value)
        {
            On = value;
            return Self;
        }

        /// <summary>
        /// Continue to the Next Node Without Waiting for this Node to Complete
        /// </summary>
        /// <returns></returns>
        [NarrativeConstructorMethod]
        public TSelf Continue() => SetWait(false);

        /// <summary>
        /// Wait for this Node to Complete before Moving to the Next Node
        /// </summary>
        /// <returns></returns>
        [NarrativeConstructorMethod]
        public TSelf Await() => SetWait(true);

        public NodeWaitProperty(TSelf reference) : this(reference, true) { }
        public NodeWaitProperty(TSelf reference, bool on)
        {
            this.Self = reference;
            this.On = on;
        }
    }

    public class NodeTextFormatProperty<TSelf>
        where TSelf : Node
    {
        TSelf Self { get; }

        internal Dictionary<string, string> Dictionary { get; }

        public TSelf Add(string key, [LocalizationParameter] object value)
        {
            Dictionary[key] = value.ToString();
            return Self;
        }

        public NodeTextFormatProperty(TSelf self)
        {
            this.Self = self;

            Dictionary = new Dictionary<string, string>();
        }
    }
}