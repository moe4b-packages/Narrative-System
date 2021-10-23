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
	public class SetSpeakerNode : Node
	{
        public string ID { get; protected set; }

        public override void Invoke()
        {
            base.Invoke();

            Script.Speaker = Character.Find(ID);

            Narrative.Player.Continue();
        }

        public SetSpeakerNode(string ID)
        {
            this.ID = ID;
        }
    }

	partial class Script
    {
        public static SetSpeakerNode SetSpeaker(string ID) => new SetSpeakerNode(ID);
    }
}