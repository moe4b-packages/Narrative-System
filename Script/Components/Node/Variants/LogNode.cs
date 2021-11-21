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
	public class LogNode : Node
	{
        public string Text { get; protected set; }

        protected internal override void Invoke()
        {
            base.Invoke();

            Debug.Log(Text);

            Playback.Next();
        }

        public LogNode(string text)
        {
            this.Text = text;
        }
	}

	partial class Script
    {
        public static LogNode Log(string text) => new LogNode(text);
    }
}