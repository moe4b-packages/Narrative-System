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

using MB.UISystem;

namespace MB.NarrativeSystem
{
	partial class Narrative
	{
		public ControlsProperty Controls { get; private set; } = new ControlsProperty();
		[Serializable]
		public class ControlsProperty
		{
			public const string Path = Narrative.Path + "Controls/";
			
			public ISayDialog Say { get; set; }
			public IChoiceDialog Choice { get; set; }

			public FadeUI Fader { get; set; }

			public AudioSource AudioSource { get; set; }
		}
	}

	public interface IDialog
	{
		MRoutine.Handle Show();
		MRoutine.Handle Hide();

		void UpdateLocalization();
	}
}