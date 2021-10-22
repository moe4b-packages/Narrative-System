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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace MB.NarrativeSystem
{
	partial class Narrative
	{
		[SerializeField]
		ProgressProperty progress = new ProgressProperty();
		[Serializable]
		class ProgressProperty
		{
			[SerializeField]
			internal string fileName = "Narrative Progress";

			[SerializeField]
			[SerializedType.Selection(typeof(JsonConverter))]
			internal SerializedType[] converters = new SerializedType[] { SerializedType.From<StringEnumConverter>() };
		}
		public static class Progress
        {
			static ProgressProperty Instance => Narrative.Instance.progress;

			public static string FileName => Instance.fileName;

			public static SerializedType[] Converters => Instance.converters;
			static JsonConverter[] CreateConverters()
            {
				var array = new JsonConverter[Converters.Length];

				for (int i = 0; i < array.Length; i++)
					array[i] = Activator.CreateInstance(Converters[i]) as JsonConverter;

				return array;
			}

			public static JObjectComposer Composer { get; private set; }
			public static bool IsLoaded => Composer.IsLoaded;

			public static bool IsDirty { get; private set; }

			public static class IO
			{
				public static string Directory { get; private set; }

				static string FormatPath(string file)
				{
					file += ".json";

					return System.IO.Path.Combine(Directory, file);
				}

				public static string Load(string file)
				{
					var target = FormatPath(file);

					if (File.Exists(target) == false)
						return string.Empty;

					var content = File.ReadAllText(target);

					return content;
				}

				public static void Save(string file, string content)
				{
					var target = FormatPath(file);

					File.WriteAllText(target, content);
				}

				static IO()
                {
					Directory = Application.isEditor ? Application.dataPath : Application.persistentDataPath;
				}
			}

			public static class SaveLock
			{
				public static bool IsOn { get; private set; }

				public static void Start()
				{
					if (IsOn)
						Debug.LogWarning("Narrative Progress Save Lock Already On, Did you Forget to Unlock it");

					IsOn = true;
				}

				public static void End()
				{
					IsOn = false;

					if (IsDirty) Save();
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

			internal static void Prepare()
			{
				var settings = new JsonSerializerSettings()
				{
					Converters = CreateConverters(),
					Formatting = Formatting.Indented,
				};

				Composer = new JObjectComposer(settings);
				Composer.OnChange += InvokeChange;

				Load();

				Application.quitting += QuitCallback;
			}

			internal static void Load()
			{
				var json = IO.Load(FileName);

				Composer.Load(json);
			}

			public static void Reset()
			{
				Composer.Clear();

				Save();
			}

			public static void Save()
			{
				IsDirty = false;

				var json = Composer.Read();

				IO.Save(FileName, json);
			}

			#region Controls
			public static T Read<T>(string id, T fallback = default) => Composer.Read<T>(id, fallback: fallback);
			public static object Read(string id, Type type, object fallback = default) => Composer.Read(id, type, fallback: fallback);

			public static bool Contains(string id) => Composer.Contains(id);

			public static bool Remove(string id) => Composer.Remove(id);

			public static void Set(string id, object value) => Composer.Set(id, value);
			#endregion

			static void InvokeChange()
			{
				if (AutoSave.OnChange)
					Save();
				else
					IsDirty = true;
			}

			static void QuitCallback()
			{
				if (AutoSave.OnExit) if (IsDirty) Save();
			}

			static Progress()
            {
				
			}
		}
	}
}