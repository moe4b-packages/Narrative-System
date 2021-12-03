#if UNITY_EDITOR
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
	partial class Narrative
	{
		public static class ScriptTemplate
		{
			[MenuItem("Assets/Create/Narrative Script", priority = 81)]
			public static void Create()
			{
				var template = FindTemplate();

				if (template == null)
					throw new Exception("No Narrative Script Template Found");

				ProjectWindowUtil.CreateScriptAssetFromTemplateFile(template, "NewNarrativeScript.cs");
			}

			public static string FindTemplate()
			{
				var paths = AssetDatabase.FindAssets("Narrative Script Template")
					.Select(AssetDatabase.GUIDToAssetPath)
					.Select(x => x.ToLower())
					.OrderBy(x => x.Contains("assets") || x.Contains("override"));

				return paths.FirstOr(null);
			}
		}
	}
}
#endif