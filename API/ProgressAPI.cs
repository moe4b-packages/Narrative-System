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
		public static NarrativeManager.ProgressProperty Progress => Manager.Progress;
	}

	partial class NarrativeManager
    {
		[SerializeField]
		ProgressProperty progress = new ProgressProperty();
		public ProgressProperty Progress => progress;
		[Serializable]
		public class ProgressProperty : Property
		{
			[SerializeField]
			string fileName = "Narrative Progress";
			public string FileName => fileName;

			[SerializeField]
			[SerializedType.Selection(typeof(JsonConverter))]
			SerializedType[] converters = new SerializedType[] { SerializedType.From<StringEnumConverter>() };
			public SerializedType[] Converters => converters;

			public JObjectComposer Composer { get; private set; }
			public bool IsLoaded => Composer.IsLoaded;

			public bool IsDirty { get; private set; }

			#region Save Lock
			public bool SaveLock { get; private set; }

			public void LockSave()
			{
				if (SaveLock)
					Debug.LogWarning("Narrative Progress Save Lock Already On, Did you Forget to Unlock it");

				SaveLock = true;
			}

			public void UnlockSave()
			{
				SaveLock = false;

				if (IsDirty) Save();
			}
			#endregion

			public IOProperty IO { get; } = new IOProperty();
			public class IOProperty
			{
				public string Directory { get; private set; }

				public ObfuscationProperty Obfuscation { get; } = new ObfuscationProperty();
				public class ObfuscationProperty
				{
					public void SetMethods(EncryptDelegate encrypt, DecryptDelegate decrypt)
					{
						Encrypt = encrypt;
						Decrypt = decrypt;
					}

					public EncryptDelegate Encrypt { get; set; } = DefaultEncryptMethod;
					public delegate string EncryptDelegate(string text);
					public static string DefaultEncryptMethod(string text) => text;

					public DecryptDelegate Decrypt { get; set; } = DefaultDecryptMethod;
					public delegate string DecryptDelegate(string text);
					public static string DefaultDecryptMethod(string text) => text;
				}

				internal void Prepare()
                {
					Directory = Application.isEditor ? Application.dataPath : Application.persistentDataPath;
				}

				public string Load(string file)
				{
					var target = FormatPath(file);

					if (File.Exists(target) == false)
						return string.Empty;

					var content = File.ReadAllText(target);

					content = Obfuscation.Decrypt(content);

					return content;
				}

				public void Save(string file, string content)
				{
					content = Obfuscation.Encrypt(content);

					var target = FormatPath(file);

					File.WriteAllText(target, content);
				}

				string FormatPath(string file)
				{
					file += ".json";

					return System.IO.Path.Combine(Directory, file);
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

            public override void Configure()
            {
                base.Configure();

				IO.Prepare();
			}

			internal void Prepare()
            {
				JsonSerializerSettings settings;

				//Create Serializer Settings
                {
					var array = new JsonConverter[converters.Length];

					for (int i = 0; i < array.Length; i++)
						array[i] = Activator.CreateInstance(converters[i]) as JsonConverter;

					settings = new JsonSerializerSettings()
					{
						Converters = array,
						Formatting = Formatting.Indented,
					};
				}

				if (Composer.IsConfigured) throw new Exception("Narrative Progress Already Configured");

				Composer.Configure(settings);
				Composer.OnChange += InvokeChange;

				Load();

				Application.quitting += QuitCallback;
			}

			internal void Load()
			{
				var json = IO.Load(fileName);

				Composer.Load(json);
			}

			public void Reset()
			{
				Composer.Clear();

				Save();
			}

			public void Save()
			{
				IsDirty = false;

				var json = Composer.Read();

				IO.Save(fileName, json);
			}

			#region Controls
			public T Read<T>(string id) => Composer.Read<T>(id);
			public object Read(string id, Type type) => Composer.Read(id, type);

			public bool Contains(string id) => Composer.Contains(id);

			public bool Remove(string id) => Composer.Remove(id);

			public void Set(string id, object value) => Composer.Set(id, value);
			#endregion

			void InvokeChange()
			{
				if (AutoSave.OnChange)
					Save();
				else
					IsDirty = true;
			}

			public event Action OnQuit;
			void QuitCallback()
			{
				OnQuit?.Invoke();

				if (AutoSave.OnExit) if (IsDirty) Save();
			}

			public ProgressProperty()
			{
				Composer = new JObjectComposer();
			}
		}
	}
}