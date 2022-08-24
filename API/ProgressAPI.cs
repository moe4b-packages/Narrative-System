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
		[field: SerializeField]
		public ProgressProperty Progress { get; private set; }
		[Serializable]
		public class ProgressProperty
		{
			[field: SerializeField]
			public string FileName { get; private set; } = "Narrative Progress";

			[field: SerializeField, SerializedType.Selection(typeof(JsonConverter))]
			public SerializedType[] Converters { get; private set; } = new SerializedType[]
			{
				SerializedType.From<StringEnumConverter>()
			};

			public JObjectComposer Composer { get; private set; }
			public bool IsLoaded => Composer.IsLoaded;

			public bool IsDirty { get; private set; }

			public IOProperty IO { get; } = new IOProperty();
			public class IOProperty
			{
				public string Directory { get; private set; }

				internal void Prepare()
                {
					Directory = Application.isEditor ? Application.dataPath : Application.persistentDataPath;
				}

				string FormatPath(string file)
				{
					file += ".json";

					return System.IO.Path.Combine(Directory, file);
				}

				public string Load(string file)
				{
					var target = FormatPath(file);

					if (File.Exists(target) == false)
						return string.Empty;

					var content = File.ReadAllText(target);

					return content;
				}

				public void Save(string file, string content)
				{
					var target = FormatPath(file);

					File.WriteAllText(target, content);
				}
			}

			public SaveLockProperty SaveLock { get; } = new SaveLockProperty();
			public class SaveLockProperty
			{
				int Count;
				public bool IsOn => Count != 0;

				internal ProgressProperty Progress;

				public void Start()
				{
					Count += 1;

					if (IsOn)
						Debug.LogWarning("Narrative Progress Save Lock Already On, Did you Forget to Unlock it");
				}

				public void End()
				{
					Count += 1;

					if (Progress.IsDirty) Progress.Save();
				}
			}

			public AutoSaveProperty AutoSave { get; } = new AutoSaveProperty();
			public class AutoSaveProperty
			{
				public bool OnChange { get; set; } = true;
				public bool OnExit { get; set; } = true;

				public bool All
				{
					set
					{
						OnChange = value;
						OnExit = value;
					}
				}
			}

			internal void Prepare()
			{
				IO.Prepare();
				SaveLock.Progress = this;

				Composer = JObjectComposer.Create<ProgressProperty>();
				var settings = new JsonSerializerSettings()
				{
					Converters = CreateConverters(),
					Formatting = Formatting.Indented,
				};
				Composer.Configure(settings);

				var json = IO.Load(FileName);
				Composer.Load(json);

				Composer.OnChange += InvokeChange;

				Application.quitting += QuitCallback;

				JsonConverter[] CreateConverters()
				{
					var array = new JsonConverter[Converters.Length];

					for (int i = 0; i < array.Length; i++)
						array[i] = Activator.CreateInstance(Converters[i]) as JsonConverter;

					return array;
				}
			}

			public void Clear()
			{
				Composer.Clear();

				Save();
			}
			public void Save()
			{
				IsDirty = false;

				var json = Composer.Read();

				IO.Save(FileName, json);
			}

			#region Controls
			public T Read<T>(string id, T fallback = default) => Composer.Read<T>(id, fallback: fallback);
			public object Read(string id, Type type, object fallback = default) => Composer.Read(id, type, fallback: fallback);

			public bool Contains(string id) => Composer.Contains(id);

			public bool Remove(string id) => Composer.Remove(id);

			public void Set(string id, object value) => Composer.Set(id, value);
            #endregion

            #region Callbacks
            void InvokeChange()
			{
				if (AutoSave.OnChange)
					Save();
				else
					IsDirty = true;
			}

			void QuitCallback()
			{
				if (AutoSave.OnExit && IsDirty) Save();
			}
            #endregion
        }
    }
}