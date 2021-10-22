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
		public const string Path = Toolbox.Paths.Root + "Narrative System/";

		public static NarrativeManager Manager => NarrativeManager.Instance;

		public static bool AutoInitialize => Manager.AutoInitialize;
		public static bool IsInitialized { get; private set; }

		[RuntimeInitializeOnLoadMethod]
		static void OnEntetPlayerMode()
		{
#if UNITY_EDITOR
			Validation.OnEntetPlayerMode();
#endif

			if (AutoInitialize) Initialize();
		}

#if UNITY_EDITOR
		[UnityEditor.Callbacks.DidReloadScripts]
		static void OnRecompile()
		{
			Validation.OnRecompile();
		}
		#endif

		public static void Initialize()
		{
			if (IsInitialized)
				throw new InvalidOperationException($"Narrative System Already Initialized");

			IsInitialized = true;

			Progress.Prepare();
			Story.Prepare();
		}

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

		public static class Composition
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
					var function = branch.CreateFunction(script);

					Node.OnCreate += Register;

					function.Invoke();
					void Register(Node node) => list.Add(node);

					Node.OnCreate -= Register;
				}

				return list;
			}

			public static IEnumerable<T> IterateAllNodes<T>()
			{
				foreach (var script in TypeQuery.FindAll<Script>())
				{
					if (script.IsAbstract) continue;

					foreach (var node in GetNodes(script))
					{
						if (node is T target)
							yield return target;
					}
				}
			}
		}
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
}