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
	public static class ScriptValidator
	{
		[InitializeOnLoadMethod]
		static void OnLoad()
		{
#if !DISABLE_NARRATIVE_SCRIPTS_VALIDATION
			Validate();
#endif
		}

		static void Validate()
		{
			var scripts = TypeQuery.FindAll<Script>();

			for (int i = 0; i < scripts.Count; i++)
				Validate(scripts[i]);
		}

		static void Validate(Type script)
		{
			ValidateBranches(script);
		}

		static void ValidateBranches(Type script)
		{
			var data = Script.Composer.Data.BranchesData.ParseSelf(script);

			var pathes = data.Select(x => x.Attribute.Path).Distinct().Count();

			if (pathes > 1)
				throw FormatException($"Multiple Branches in Multiple Files for Partial Scripts are not Supported");

			for (int i = 0; i < data.Count; i++)
				ValidateBranch(script, data[i]);

			ScriptValidationException FormatException(string text)
			{
				text = $"{text}" +
					$"{Environment.NewLine}" +
					$"On {Script.Format.Name.Retrieve(script)}";

				return new ScriptValidationException(text);
			}
		}

		static void ValidateBranch(Type script, Script.Composer.Data.BranchesData.Data data)
		{
			var method = data.Method;

			if (method.ReturnType != typeof(void))
				throw FormatException("Invalid Return Type, Branches Must Return an IEnumerator<Node>");

			if (method.GetParameters().Length > 0)
				throw FormatException("Invalid Parameters, Branches Must not take in any Parameters");

			ScriptValidationException FormatException(string text)
			{
				var name = Branch.Format.Name(method);

				var path = Branch.Format.FullName(Script.Format.Name.Retrieve(script), name);

				text = $"{text}" +
					$"{Environment.NewLine}" +
					$"On {path}";

				return new ScriptValidationException(text);
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