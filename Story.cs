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
			public const BindingFlags Flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

			public static class Scripts
            {
				public static List<Variable> List { get; private set; }
				public class Variable : NarrativeSystem.Variable
				{
					public Type Script { get; protected set; }

					public Variable(VariableAttribute attribute, MemberInfo member) : base(attribute, member, null)
					{
						Script = member.DeclaringType;
					}
				}

				internal static void Configure()
				{
					var types = TypeQuery.FindAll<Script>();

					for (int i = 0; i < types.Count; i++)
						Register(types[i]);
				}
				internal static void Register(Type type)
				{
					var members = new List<MemberInfo>();
					members.AddRange(type.GetFields(Flags));
					members.AddRange(type.GetProperties(Flags));

					for (int i = 0; i < members.Count; i++)
					{
						var attribute = members[i].GetCustomAttribute<VariableAttribute>(true);
						if (attribute == null) continue;

						var data = new Variable(attribute, members[i]);
						List.Add(data);
					}
				}

				public static void Load()
				{
					for (int i = 0; i < List.Count; i++)
					{
						if (Narrative.Progress.Scripts.Contains(List[i].Script, List[i].Name) == false) continue;

						var value = Narrative.Progress.Scripts.Read(List[i].Script, List[i].Type, List[i].Name);

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

						Narrative.Progress.Scripts.Set(List[i].Script, List[i].Name , value);
					}

					Narrative.Progress.UnlockSave();
				}

				static Scripts()
				{
					List = new List<Variable>();
				}
			}

			public static class Global
            {
				public static List<Variable> List { get; private set; }
				public class Variable : NarrativeSystem.Variable
				{
					public string Path { get; protected set; }

					public Variable(VariableAttribute attribute, MemberInfo member) : base(attribute, member, null)
					{
						Path = MUtility.IterateNest(member.DeclaringType, Iterate)
							.Reverse()
							.Skip(1)
							.Select(Select)
							.Join(JObjectComposer.Path.Seperator);

						Path = JObjectComposer.Path.Compose(Path, Name);
					}

					static Type Iterate(Type type) => type.DeclaringType;
					static string Select(Type type) => type.Name;
				}

				internal static void Configure()
				{
					var type = typeof(Story);
					Register(type);
				}
				internal static void Register(Type type)
				{
					var members = new List<MemberInfo>();
					members.AddRange(type.GetFields(Flags));
					members.AddRange(type.GetProperties(Flags));

					for (int i = 0; i < members.Count; i++)
					{
						var attribute = members[i].GetCustomAttribute<VariableAttribute>(true);
						if (attribute == null) continue;

						var data = new Variable(attribute, members[i]);
						List.Add(data);
					}

					var nested = type.GetNestedTypes(Flags | BindingFlags.Instance);
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

				static Global()
				{
					List = new List<Variable>();
				}
			}

			internal static void Configure()
			{
				Scripts.Configure();
				Global.Configure();

				Narrative.Progress.OnLoad += Load;
				Narrative.Progress.OnQuit += Save;

				if (Narrative.Progress.IsLoaded) Load();
			}

			public static void Save()
            {
				Scripts.Save();
				Global.Save();
			}

			public static void Load()
            {
				Scripts.Load();
				Global.Load();
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