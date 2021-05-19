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
	public static class Narrative
	{
		public const string Path = Toolbox.Path + "Narrative System/";

		public static class Controls
        {
			public static ISayDialog Say { get; set; }

			public static IChoiceDialog Choice { get; set; }
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