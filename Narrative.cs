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

		public static class Controls
		{
			public static ISayDialog Say { get; set; }

			public static IChoiceDialog Choice { get; set; }

			public static UIFader Fader { get; set; }
		}

		public static class Progress
		{
			public static JObject Context { get; private set; }
			public static JsonSerializer Serializer { get; private set; }

			public static string Slot { get; private set; }

			public static bool IsDiry { get; private set; }

			public static bool IsReady { get; private set; }

			public static void Configure(params JsonConverter[] converters)
			{
				var settings = new JsonSerializerSettings()
				{
					Converters = converters,
					Formatting = Formatting.Indented,
				};

				Configure(settings);
			}
			public static void Configure(JsonSerializerSettings settings)
			{
				if (IsReady) throw new Exception("Narrative Progress Already Configured");

				IsReady = true;

				Serializer = JsonSerializer.Create(settings);

				Application.quitting += QuitCallback;
			}

			static void QuitCallback()
			{
				if (AutoSave.OnExit) if (IsDiry) Save();
			}

			public static class IO
			{
				public static string Load(string file)
				{
					var target = FormatPath(file);

					if (File.Exists(target) == false)
						return string.Empty;

					return File.ReadAllText(target);
				}

				static string FormatPath(string file)
				{
					file += ".json";

					var directory = Application.isEditor ? "Assets/" : Application.persistentDataPath;

					return System.IO.Path.Combine(directory, file);
				}

				public static void Save(string file, string content)
				{
					var target = FormatPath(file);

					File.WriteAllText(target, content);
				}
			}

			public static class AutoSave
			{
				public static bool OnChange { get; set; } = true;

				public static bool OnExit { get; set; } = true;

				public static bool All
                {
					set
                    {
						OnChange = value;
						OnExit = value;
                    }
                }
			}

			public static class Global
			{
				public static readonly string[] Path = new string[] { "Global" };

				public static bool TryRead<T>(string id, out T value) => Progress.TryRead(Path, id, out value);

				public static T Read<T>(string id) => Progress.Read<T>(Path, id);

				public static bool Contains(string id) => Progress.Contains(Path, id);

				public static bool Remove(string id) => Progress.Remove(Path, id);
				public static bool RemoveAll() => Progress.RemoveAll(Path);

				public static void Set<T>(string id, T value) => Progress.Set(Path, id, value);
			}

			public static class Scripts
			{
				public static bool TryRead<T>(Script script, string id, out T value)
				{
					var type = script.GetType();

					return TryRead(type, id, out value);
				}
				public static bool TryRead<T>(Type script, string id, out T value)
				{
					var path = RetrieveScriptPath(script);

					return Progress.TryRead(path, id, out value);
				}

				public static T Read<T>(Script script, string id)
				{
					var type = script.GetType();

					return Read<T>(type, id);
				}
				public static T Read<T>(Type script, string id)
				{
					var path = RetrieveScriptPath(script);

					return Progress.Read<T>(path, id);
				}

				public static bool Contains(Script script, string id)
				{
					var type = script.GetType();

					return Contains(type, id);
				}
				public static bool Contains(Type script, string id)
				{
					var path = RetrieveScriptPath(script);

					return Progress.Contains(path, id);
				}

				public static bool Contains(Script script)
				{
					var type = script.GetType();

					return Contains(type);
				}
				public static bool Contains(Type script)
				{
					var path = RetrieveScriptPath(script);

					return Progress.Contains(path);
				}

				public static bool Remove(Script script, string id)
				{
					var type = script.GetType();

					return Remove(type, id);
				}
				public static bool Remove(Type type, string id)
				{
					var path = Script.Format.Path.Retrieve(type);

					return Progress.Remove(path, id);
				}

				public static bool RemoveAll(Script script)
				{
					var type = script.GetType();

					return RemoveAll(type);
				}
				public static bool RemoveAll(Type type)
				{
					var path = Script.Format.Path.Retrieve(type);

					return Progress.RemoveAll(path);
				}

				public static void Set<T>(Script script, string id, T value)
				{
					var type = script.GetType();

					Set(type, id, value);
				}
				public static void Set<T>(Type script, string id, T value)
				{
					var path = RetrieveScriptPath(script);

					Progress.Set(path, id, value);
				}
			}
			public static class Script<TScript>
				where TScript : Script
			{
				public static Type Type => typeof(TScript);

				public static bool TryRead<T>(string id, out T value) => Scripts.TryRead<T>(Type, id, out value);

				public static T Read<T>(string id) => Scripts.Read<T>(Type, id);

				public static bool Contains(string id) => Scripts.Contains(Type, id);
				public static bool Contains() => Scripts.Contains(Type);

				public static bool Remove(string id) => Scripts.Remove(Type, id);
				public static bool RemoveAll() => Scripts.RemoveAll(Type);

				public static void Set<T>(string id, T value) => Scripts.Set(Type, id, value);
			}

			public static void Load(string slot)
			{
				Slot = slot;

				Load();
			}
			static void Load()
			{
				var json = IO.Load(Slot);

				if (string.IsNullOrEmpty(json))
				{
					Context = new JObject();

					Context.Add("Global", new JObject());
				}
				else
				{
					Context = JObject.Parse(json);
				}
			}

			public static void Save()
			{
				IsDiry = false;

				var json = Context.ToString();

				IO.Save(Slot, json);
			}

			static void TryAutoSave()
            {
				if (AutoSave.OnChange)
					Save();
				else
					IsDiry = true;
			}

			public static void Reset()
            {
				Context = new JObject();

				Save();
            }

			#region Utility
			static JToken Retrieve(params string[] path)
			{
				JToken token = Context;

				for (int i = 0; i < path.Length; i++)
				{
					token = token[path[i]];

					if (token == null) return null;
				}

				return token;
			}

			static JToken Assimilate(params string[] path)
            {
				JToken token = Context;

				for (int i = 0; i < path.Length; i++)
				{
					var target = token[path[i]];

					if (target == null)
					{
						target = new JObject();
						(token as JObject).Add(path[i], target);
					}

					token = target;
				}

				return token;
			}

			static string[] RetrieveScriptPath(Type type) => Script.Format.Path.Retrieve(type);
			#endregion

			#region Controls
			public static bool TryRead<T>(string[] path, string id, out T value)
            {
				var token = Retrieve(path)?[id];

				if (token == null)
				{
					value = default;
					return false;
				}

				value = token.ToObject<T>(Serializer);
				return true;
			}

			public static T Read<T>(string[] path, string id)
			{
				TryRead(path, id, out T value);

				return value;
			}

			public static bool Contains(string[] path, string id)
            {
				var token = Retrieve(path)?[id];

				if (token == null)
					return false;

				return true;
			}
			public static bool Contains(string[] path)
			{
				var token = Retrieve(path);

				if (token == null)
					return false;

				return true;
			}

			public static bool Remove(string[] path, string id)
			{
				var token = Retrieve(path)?[id]?.Parent;
				if (token == null) return false;

				token.Remove();

				TryAutoSave();

				return true;
			}
			public static bool RemoveAll(string[] path)
			{
				var token = Retrieve(path)?.Parent;
				if (token == null) return false;

				token.Remove();

				TryAutoSave();
				return false;
			}

			public static void Set<T>(string[] path, string id, T value)
			{
				var token = Assimilate(path);

				token[id] = JToken.FromObject(value, Serializer);

				TryAutoSave();
			}
			#endregion
		}

		#region Play
		public static T Play<T>()
			where T : Script, new()
		{
			var instance = new T();

			Play(instance);

			return instance;
		}

		public static Script[] Play(params Script.Asset[] assets)
		{
			var scripts = new Script[assets.Length];

			for (int i = 0; i < assets.Length; i++)
				scripts[i] = assets[i].CreateInstance();

			Play(scripts);

			return scripts;
		}

		public static void Play(params Script[] scripts)
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

		public static void Play(Script script)
		{
			script.Play();
		}
		#endregion
	}

	public interface IDialog
	{
		void Show();
		void Hide();
	}

	#region Say
	public interface ISayData
	{
		Character Character { get; }

		string Text { get; }

		bool AutoSubmit { get; }
	}

	public interface ISayDialog : IDialog
	{
		void Show(ISayData data, Action submit);
	}
	#endregion

	#region Choice
	public interface IChoiceData
	{
		public string Text { get; }
	}

	public delegate void ChoiceSubmitDelegate(int index, IChoiceData data);

	public interface IChoiceDialog : IDialog
	{
		void Show<T>(IList<T> entries, ChoiceSubmitDelegate submit) where T : IChoiceData;
	}
	#endregion
}