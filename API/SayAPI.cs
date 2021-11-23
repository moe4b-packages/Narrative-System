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
	public interface ISayData
	{
		Character Character { get; }

		string Text { get; }

		Dictionary<string, string> GetPhrases();

		bool AutoSubmit { get; }
	}

	public interface ISayDialog : IDialog
	{
		void Show(ISayData data, Action submit);

		void Clear();
	}
}