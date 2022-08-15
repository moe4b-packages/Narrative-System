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

using MB.LocalizationSystem;

namespace MB.NarrativeSystem
{
	public interface IChoiceData
	{
		public IChoiceEntry Retrieve(int index);
		public int Count { get; }

		ILocalizationFormat Format { get; }

		ChoiceSubmitDelegate Callback { get; }
	}

	public interface IChoiceEntry
	{
		public string Text { get; }
	}

	public delegate void ChoiceSubmitDelegate(int index, IChoiceEntry entry);

	public interface IChoiceDialog : IDialog
	{
		MRoutine.Handle Show<T>(T data) where T : IChoiceData;
	}
}