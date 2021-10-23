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
	public class RaiseEventNode : Node
	{
        public string Key { get; protected set; }

        public override void Invoke()
        {
            base.Invoke();

            Narrative.Events.Raise(Key);

            Narrative.Player.Continue();
        }

        public RaiseEventNode(string key)
        {
            this.Key = key;
        }
    }

	partial class Script
    {
		public static RaiseEventNode RaiseEvent(string key) => new RaiseEventNode(key);
    }
}