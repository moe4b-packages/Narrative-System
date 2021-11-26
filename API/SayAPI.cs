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
	public interface ISayData
	{
		Character Character { get; }

		string Text { get; }
		ILocalizationFormat Format { get; }

		bool AutoSubmit { get; }

		Action Callback { get; }
	}

	public interface ISayDialog : IDialog
	{
		void Show(ISayData data);

		void Clear();
	}
}