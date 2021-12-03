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
using System.Reflection;

namespace MB.NarrativeSystem
{
#if UNITY_EDITOR
	partial class Narrative
	{
		[Tooltip("Specifies when to validate narrative components, " +
			"validating will catch the basic composition errors in your scripts, editor only!")]
		[SerializeField]
		internal bool validateScripts = true;
		public static bool ValidateScripts => Instance.validateScripts;

		public static class Validation
		{
			public static void Initialize()
            {
				if (ValidateScripts == false) return;

				Process();
            }

			public static void Process()
			{
				var scripts = TypeQuery.FindAll<Script>();

				for (int i = 0; i < scripts.Count; i++)
					Process(scripts[i]);
			}

			public static void Process(Type script)
			{
				Branches.Process(script);
			}

			public static class Branches
            {
				public static void Process(Type script)
				{
					var data = Script.Composition.BranchesData.ParseSelf(script);

					var pathes = data.Select(x => x.Attribute.Path).Distinct().Count();

					if (pathes > 1)
						Validation.Throw("Multiple Branches in Multiple Files for Partial Scripts are not Supported", script);
				}

				public static void Throw(string error, Type script, MethodInfo branch)
                {
					Debug.LogError($"Narrative Validation Error:\n" +
						$"{error}\n" +
						$"Branch: '{Branch.Format.FullName(script, branch)}'");
				}
			}

			public static void Throw(string error, Type script)
			{
				Debug.LogError($"Narrative Validation Error:\n" +
					$"{error}\n" +
					$"Script: '{Script.Format.Name.Retrieve(script)}'");
			}
		}
	}
#endif

	public class ScriptValidationException : Exception
	{
		public ScriptValidationException(string text) : base(text)
		{

		}
	}
}