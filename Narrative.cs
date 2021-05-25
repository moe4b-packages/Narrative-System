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
using System.Reflection;

namespace MB.NarrativeSystem
{
	public static class Narrative
	{
		public const string Path = Toolbox.Path + "Narrative System/";

		public static class Controls
		{
			public static ISayDialog Say { get; set; }

			public static IChoiceDialog Choice { get; set; }

			public static UIFader Fader { get; set; }
		}

		public static T Play<T>()
			where T : Script, new()
		{
			var instance = new T();

			Play(instance);

			return instance;
		}

		public static Script[] Play(params Script.Asset[] assets)
        {
			var scripts = new Script[assets.Length];

			for (int i = 0; i < assets.Length; i++)
				scripts[i] = assets[i].CreateInstance();

			Play(scripts);

			return scripts;
		}

		public static void Play(params Script[] scripts)
        {
			Iterate(0);

			void Iterate(int index)
			{
				if (scripts.ValidateCollectionBounds(index) == false) return;

				scripts[index].OnEnd += Continue;
				void Continue() => Iterate(index + 1);

				Play(scripts[index]);
			}
		}

		public static void Play(Script script)
		{
			script.Play();
		}
	}

	public interface IDialog
	{
		void Show();
		void Hide();
	}

	#region Say
	public interface ISayData
	{
		Character Character { get; }

		string Text { get; }

		bool AutoSubmit { get; }
	}

	public interface ISayDialog : IDialog
	{
		void Show(ISayData data, Action submit);
	}
	#endregion

	#region Choice
	public interface IChoiceData
	{
		public string Text { get; }
	}

	public delegate void ChoiceSubmitDelegate(int index, IChoiceData data);

	public interface IChoiceDialog : IDialog
	{
		void Show<T>(IList<T> entries, ChoiceSubmitDelegate submit) where T : IChoiceData;
	}
	#endregion
}