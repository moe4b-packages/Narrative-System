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
	[CreateAssetMenu]
	[Global(ScriptableManagerScope.Project)]
	[SettingsMenu(Toolbox.Paths.Root + "Narrative")]
	[LoadOrder(Runtime.Defaults.LoadOrder.NarrativeSystem)]
	public partial class Narrative : ScriptableManager<Narrative>
	{
		public const string Path = Toolbox.Paths.Root + "Narrative System/";

		protected override void OnLoad()
		{
			base.OnLoad();

			Characters.Refresh();

			Validation.Process();

			if (IsPlaying) Initialize();
		}

		static void Initialize()
		{
			Progress.Prepare();
			Story.Prepare();
		}

#if UNITY_EDITOR
		protected override void PreProcessBuild()
		{
			base.PreProcessBuild();

			Characters.Refresh();
		}

		public static class Composition
		{
			public static List<Script> Scripts { get; }
			public static List<Branch> Branches { get; }
			public static List<Node> Nodes { get; }

			public static IEnumerable<T> IterateNodes<T>()
			{
				foreach (var node in Nodes)
				{
					if (node is T target)
						yield return target;
				}
			}

			static Composition()
			{
				//Scripts
				{
					Scripts = new List<Script>();

					foreach (var type in TypeCache.GetTypesDerivedFrom<Script>())
					{
						if (type.IsAbstract) continue;

						var instance = Activator.CreateInstance(type) as Script;
						Scripts.Add(instance);
					}
				}

				//Branches
				{
					Branches = new List<Branch>();

					foreach (var script in Scripts)
					{
						var composition = Script.Composition.Retrieve(script).Branches;

						for (int i = 0; i < composition.Count; i++)
						{
							var function = composition[i].CreateFunction(script);
							var instance = new Branch(script, i, function);
							Branches.Add(instance);
						}
					}
				}

				//Nodes
				{
					Nodes = new List<Node>();

					foreach (var branch in Branches)
					{
						var range = Iterate(branch.Function());
						Nodes.AddRange(range);
					}

					static IEnumerable<Node> Iterate(IEnumerable<Script.Block> branch)
					{
						foreach (var Block in branch)
						{
							if (Block.HasNode)
							{
								yield return Block.Node;
							}
							else if (Block.HasBody)
							{
								foreach (var nest in Iterate(Block.Body))
								{
									yield return nest;
								}
							}
						}
					}
				}
			}
		}
#endif
	}

	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	sealed class NarrativeConstructorMethodAttribute : Attribute
	{

	}

	[AttributeUsage(AttributeTargets.Parameter)]
	public class DynamicResourceParameterAttribute : Attribute
	{

	}
}