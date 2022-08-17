using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

namespace MB.NarrativeSystem
{
    partial class Narrative
    {
		public const string ScriptSuffixPath = nameof(Script) + "/";

		public static T Play<T>()
			where T : Script, new()
		{
			var instance = new T();

			Play(instance);

			return instance;
		}

		public static Script Play(Script.Asset asset) => Play(asset.Type);
		public static Script Play(Type type)
		{
			if (typeof(Script).IsAssignableFrom(type) == false)
				throw new ArgumentException($"Invalid Type, Type Must Inherit from {typeof(Script)}", nameof(type));

			var instance = Activator.CreateInstance(type) as Script;

			Play(instance);

			return instance;
		}

		public static void Play(Script script)
		{
			Player.Start(script);
		}

		public static class Player
        {
			public static Script Script { get; private set; }
			public static Script.Composition Composition { get; private set; }

			public static class Variables
			{
				public static List<Variable> List { get; } = new List<Variable>();

				internal static void Load()
				{
					Clear();

					List.EnsureCapacity(Composition.Variables.Count);

					for (int i = 0; i < Composition.Variables.Count; i++)
					{
						var variable = Variable.Assimilate(Script, Composition.Variables[i].Info, ScriptSuffixPath + Script.Name);
						List.Add(variable);
					}
				}

				static void Clear()
				{
					List.Clear();
				}
			}

			public static class Branches
			{
				public static List<Branch> List { get; } = new List<Branch>();

				public static Branch First => List.First();
				public static Branch Last => List.Last();

				public static int Count => List.Count;

				public static DualDictionary<string, Branch.Delegate, Branch> Dictionary { get; } = new DualDictionary<string, Branch.Delegate, Branch>();
				public static bool TryGet(string id, out Branch branch) => Dictionary.TryGetValue(id, out branch);
				public static bool TryGet(Branch.Delegate id, out Branch branch) => Dictionary.TryGetValue(id, out branch);

				public static class Selection
                {
					public static int Index { get; internal set; }
					public static Branch Value => List[Index];

					public static Branch Previous => List.SafeIndex(Index - 1);
					public static Branch Next => List.SafeIndex(Index + 1);
				}

				public static class Nodes
                {
					static Stack<IEnumerator<Script.Block>> Stack { get; } = new Stack<IEnumerator<Script.Block>>();
					static IEnumerator<Script.Block> Top => Stack.Peek();

					internal static void Load(Branch branch)
					{
						Clear();

						var numerator = branch.Function().GetEnumerator();
						Stack.Push(numerator);
					}

					internal static bool Iterate(out Node node)
					{
						Start:

						if (Top.MoveNext())
						{
							if (Top.Current.HasNode)
							{
								node = Top.Current.Node;
								return true;
							}
							else if (Top.Current.HasBody)
							{
								var numerator = Top.Current.Body.GetEnumerator();
								Stack.Push(numerator);
								goto Start;
							}
							else
							{
								throw new InvalidDataException($"Invalid Yield Return, Cannot Process {Top.Current}");
							}
						}
						else
						{
							Stack.Pop();

							if (Stack.Count == 0)
							{
								node = default;
								return false;
							}
							else
							{
								goto Start;
							}
						}
					}

					static void Clear()
					{
						Stack.Clear();
					}
				}

				public static void Load()
				{
					Clear();

					Selection.Index = -1;

					var functions = Composition.Branches.RetrieveFunctions(Script);

					List.EnsureCapacity(functions.Length);

					for (int i = 0; i < functions.Length; i++)
					{
						var branch = new Branch(Script, i, functions[i]);

						List.Add(branch);
						Dictionary.Add(branch.ID, branch.Function, branch);
					}
				}

				internal static bool Iterate()
				{
					if (Selection.Next == null)
						return false;

					Set(Selection.Next);
					return true;
				}

				internal static void Set(Branch branch)
				{
					Selection.Index = branch.Index;
					Nodes.Load(Selection.Value);
				}

				static void Clear()
				{
					List.Clear();
					Dictionary.Clear();
				}
			}

			public static void Start(Script instance)
			{
				Script = instance;
				Composition = Script.Composition.Retrieve(Script);

				if (Composition.Branches.Count == 0)
				{
					Debug.LogWarning($"{Script} Has 0 Branches Defined");
					Stop();
					return;
				}

				Variables.Load();
				Branches.Load();

				Invoke(Branches.First);
			}

			public static void Invoke(Branch.Delegate branch)
			{
				if (Branches.TryGet(branch, out var instance) == false)
                {
					var id = Branch.Format.ID(branch);
					throw new Exception($"Couldn't Find Branch Assigned for {Script}->{id}");
				}

				Invoke(instance);
			}
			internal static void Invoke(Branch branch)
			{
				Branches.Set(branch);

				Continue();
			}

			static void Invoke(Node selection)
			{
				selection.Invoke();
			}

			public static void Continue()
			{
				if (Branches.Nodes.Iterate(out var node))
				{
					Invoke(node);
				}
				else
				{
					if (Branches.Selection.Next == null)
						End();
					else
						Invoke(Branches.Selection.Next);
				}
			}

			public static void Stop()
			{
				End();
			}

			public delegate void EndDelegate(Script script);
			public static event EndDelegate OnEnd;
			public static void End()
			{
				var instance = Script;

				Script = default;
				Composition = default;

				OnEnd?.Invoke(instance);
			}
		}
	}
}