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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Text;
using System.Text.RegularExpressions;

namespace MB.NarrativeSystem
{
	public static partial class Narrative
	{
		public const string Path = Toolbox.Path + "Narrative System/";

		#region Play
		public static Script[] PlayAll(params Script.Asset[] assets)
		{
			var scripts = Array.ConvertAll(assets, x => x.CreateInstance());

			PlayAll(scripts);

			return scripts;
		}
		public static void PlayAll(params Script[] scripts)
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

		public static T Play<T>()
			where T : Script, new()
		{
			var instance = new T();

			Play(instance);

			return instance;
		}
		public static Script Play(Script.Surrogate surrogate)
		{
			surrogate.Script.Play();

			return surrogate.Script;
		}
		#endregion
	}

	public interface ILocalizationTarget
	{
		IEnumerable<string> TextForLocalization { get; }
	}

	public interface IDynamicResourceTarget
	{
		IEnumerable<string> DynamicResources { get; }
	}

	public interface INestedScriptTarget
	{
		IEnumerable<Script> NestedScripts { get; }
	}

	partial class Narrative
    {
		public static List<Node> GetNodes<T>()
        {
			var type = typeof(T);
			return GetNodes(type);
        }
		public static List<Node> GetNodes(Type type)
        {
			var script = Activator.CreateInstance(type) as Script;
			return GetNodes(script);
        }
		public static List<Node> GetNodes(Script script)
		{
			var list = new List<Node>();

			var composition = Script.Composer.Retrieve(script);

			foreach (var branch in composition.Branches.List)
			{
				var numerator = branch.CreateFunction(script).Invoke();

				while (numerator.MoveNext())
				{
					var node = numerator.Current;

					list.Add(node);
				}
			}

			return list;
		}
	}
}