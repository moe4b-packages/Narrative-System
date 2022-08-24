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
	public static partial class Story
	{
		internal static Narrative Narrative => Narrative.Instance;

		public static class Variables
		{
			public const BindingFlags Flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

			public static List<Variable> List { get; private set; }

			public static class Scripts
            {
				internal static void Configure()
				{
					var types = TypeQuery.FindAll<Script>();

					for (int i = 0; i < types.Count; i++)
						Process(types[i]);
				}

				internal static void Process(Type type)
				{
					var members = type.GetVariables(Flags);

					var path = Narrative.ScriptSuffixPath + Script.Format.Name.Retrieve(type);

					for (int i = 0; i < members.Count; i++)
					{
						if (typeof(Variable).IsAssignableFrom(members[i].ValueType) == false) continue;

						var variable = Variable.Assimilate(null, members[i], path);

						Register(variable);
					}
				}
			}
			public static class Global
            {
				internal static void Configure()
				{
					var type = typeof(Story);
					Process(type);
				}

				internal static void Process(Type type)
				{
					var members = type.GetVariables(Flags);

					var path = RetrievePath(type);

					for (int i = 0; i < members.Count; i++)
					{
						if (typeof(Variable).IsAssignableFrom(members[i].ValueType) == false) continue;

						var variable = Variable.Assimilate(null, members[i], path);

						Register(variable);
					}

					var nested = type.GetNestedTypes(Flags | BindingFlags.Instance);
					for (int i = 0; i < nested.Length; i++)
					{
						if (nested[i].IsClass == false) continue;

						Process(nested[i]);
					}
				}

				public static string RetrievePath(Type type)
				{
					var path = MUtility.Type.IterateNesting(type)
							.Reverse()
							.Select(Select)
							.Join(JObjectComposer.Path.Seperator);

					return path;

					static string Select(Type type) => type.Name;
				}
			}

			internal static void Prepare()
			{
				//Register Narrative Progress Entry
				{
					const string id = nameof(Story);

					if (Narrative.Progress.Contains(id) == false)
						Narrative.Progress.Set(id, new object());
				}

				Scripts.Configure();
				Global.Configure();
			}

			public static void Register(Variable variable)
			{
				List.Add(variable);
			}

			static Variables()
            {
				List = new List<Variable>();
            }
		}

		public static void Prepare()
		{
			Variables.Prepare();
		}

		static Story()
		{

		}
	}

	//Story Extension Example
	partial class Story
	{
		public static Variable<int> Counter = Variable.Assign(0);

		public static class Player
		{
			public static Variable<int> Health = Variable.Assign(100);

			public static Variable<int> Armor = Variable.Assign(100);
		}
	}
}