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
	public partial class Narrative
    {
		public static class ScriptTemplate
		{
			[MenuItem("Assets/Create/Narrative Script", priority = 82)]
			public static void Create()
			{
				var template = FindTemplate();

				if (template == default)
					throw new Exception("No Narrative Script Template Found");

				ProjectWindowUtil.CreateScriptAssetFromTemplateFile(template, "New Narrative Script.cs");
			}

			public static string FindTemplate()
			{
				var paths = AssetDatabase.FindAssets("Narrative Script Template")
					.Select(AssetDatabase.GUIDToAssetPath)
					.OrderBy(x => x.BeginsWith("Assets"));

				foreach (var path in paths)
				{
					return path;
				}

				return default;
			}
		}
	}
}
#endif