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
		public static class Controls
		{
			public const string Path = Narrative.Path + "Controls/";
			
			public static ISayDialog Say { get; set; }

			public static IChoiceDialog Choice { get; set; }

			public static FadeUI Fader { get; set; }

			public static AudioSource AudioSource { get; set; }
		}
	}

	public interface IDialog
	{
		void Show();
		void Hide();

		void UpdateLocalization();
	}
}