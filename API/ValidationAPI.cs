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
		internal ValidateScriptOptions validationOption = ValidateScriptOptions.All;
		public static ValidateScriptOptions ValidationOption => Instance.validationOption;

		public static class Validation
		{
			internal static void OnRecompile()
			{
				if (EditorApplication.isPlayingOrWillChangePlaymode)
					return;

				if (ValidationOption.HasFlag(ValidateScriptOptions.Recompile))
					Process();
			}
			internal static void OnEntetPlayerMode()
			{
				if (ValidationOption.HasFlag(ValidateScriptOptions.EnterPlayMode))
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

					for (int i = 0; i < data.Count; i++)
						Process(script, data[i]);
				}

				public static void Process(Type script, Script.Composition.BranchesData.Data data)
				{
					var method = data.Method;

					if (method.ReturnType != typeof(IEnumerable<Script.Block>))
						Throw("Invalid Return Type, Branches Must Have a Return Type of Body (Alias for IEnumerable<Script.Block>)", script, data.Method);

					if (method.GetParameters().Length > 0)
						Throw("Invalid Parameters, Branches Must not take in any Parameters", script, data.Method);
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

	[Flags]
	public enum ValidateScriptOptions
	{
		None = 0,
		Recompile = 1 << 0,
		EnterPlayMode = 1 << 1,
		All = ~0,
	}

	public class ScriptValidationException : Exception
	{
		public ScriptValidationException(string text) : base(text)
		{

		}
	}
}