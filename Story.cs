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
		public static class Variables
		{
			public const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

			public static List<Variable> List { get; private set; }
			public class Variable : NarrativeSystem.Variable
            {
				public string Path { get; protected set; }

				public Variable(VariableAttribute attribute, MemberInfo member, string path) : base(attribute, member, null)
				{
					this.Path = path + JObjectComposer.Path.Seperator + Name;
				}
			}

			internal static void Configure()
			{
				Narrative.Progress.OnLoad += Load;
				Narrative.Progress.OnQuit += Save;

				var type = typeof(Story);
				Register(type);
			}
			internal static void Register(Type type)
			{
				var path = GetNestPath(type);

				var members = new List<MemberInfo>();
				members.AddRange(type.GetFields(Flags));
				members.AddRange(type.GetProperties(Flags));

				for (int i = 0; i < members.Count; i++)
				{
					var attribute = members[i].GetCustomAttribute<VariableAttribute>(true);

					if (attribute == null) continue;

					var data = new Variable(attribute, members[i], path);

					List.Add(data);
				}

				var nested = type.GetNestedTypes(Flags);

				for (int i = 0; i < nested.Length; i++)
				{
					if (nested[i].IsClass == false) continue;

					Register(nested[i]);
				}
			}

			public static void Load()
			{
				for (int i = 0; i < List.Count; i++)
				{
					if (Narrative.Progress.Global.Contains(List[i].Path) == false) continue;

					var value = Narrative.Progress.Global.Read(List[i].Type, List[i].Path);

					List[i].Value = value;
					List[i].Default = value;
				}
			}
			public static void Save()
			{
				Narrative.Progress.LockSave();

				for (int i = 0; i < List.Count; i++)
				{
					var value = List[i].Value;

					if (Equals(value, List[i].Default)) continue;

					Narrative.Progress.Global.Set(List[i].Path, value);
				}

				Narrative.Progress.UnlockSave();
			}

			static Variables()
			{
				List = new List<Variable>();
			}

			//Pure Utility

			internal static string GetNestPath(Type type)
			{
				var text = Iterate(type).Reverse().Select(Select).Aggregate(Aggregate);

				return text;

				static IEnumerable<Type> Iterate(Type type)
				{
					while (true)
					{
						yield return type;

						type = type.DeclaringType;

						if (type == null) break;
					}
				}

				static string Select(Type type) => MUtility.PrettifyName(type.Name);

				static string Aggregate(string x, string y) => $"{x}{JObjectComposer.Path.Seperator}{y}";
			}
		}

		public static void Configure()
		{
			Variables.Configure();
		}

		static Story()
		{
			
		}
	}

	partial class Story
	{
		[Variable]
		public static int Counter = 0;

		public class Player
		{
			[Variable]
			public static int Health = 100;

			[Variable]
			public static int Armor = 100;
		}
	}
}