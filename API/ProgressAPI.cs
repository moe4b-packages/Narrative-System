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

namespace MB.NarrativeSystem
{
	partial class Narrative
	{
		/// <summary>
		/// Requires .Net 4.X compatibility on Windows (and possibly other platforms too)
		/// instead of .Net 2.0 Standard because of a bug? with JsonDotNet I think
		/// </summary>
		public static class Progress
		{
			public static JObjectComposer Composer { get; private set; }

			public static string Slot { get; private set; }
			public static bool IsLoaded => Composer.IsLoaded;

			public static bool IsDirty { get; private set; }

            #region Save Lock
            public static bool SaveLock { get; private set; }

			public static void LockSave()
			{
				if (SaveLock)
					Debug.LogWarning("Narrative Progress Save Lock Already On, Did you Forget to Unlock it");

				SaveLock = true;
			}

			public static void UnlockSave()
			{
				SaveLock = false;

				if (IsDirty) Save();
			}
			#endregion

			public static class IO
			{
				public static string Directory { get; private set; }

				public static string Load(string file)
				{
					var target = FormatPath(file);

					if (File.Exists(target) == false)
						return string.Empty;

					var content = File.ReadAllText(target);

					content = Obfuscation.Decrypt(content);

					return content;
				}

				public static void Save(string file, string content)
				{
					content = Obfuscation.Encrypt(content);

					var target = FormatPath(file);

					File.WriteAllText(target, content);
				}

				static string FormatPath(string file)
				{
					file += ".json";

					return System.IO.Path.Combine(Directory, file);
				}

				static IO()
				{
					Directory = Application.isEditor ? Application.dataPath : Application.persistentDataPath;
				}
			}

			public static class Obfuscation
			{
				public static void SetMethods(EncryptDelegate encrypt, DecryptDelegate decrypt)
				{
					Encrypt = encrypt;
					Decrypt = decrypt;
				}

				public static EncryptDelegate Encrypt { get; set; } = DefaultEncryptMethod;
				public delegate string EncryptDelegate(string text);
				public static string DefaultEncryptMethod(string text) => text;

				public static DecryptDelegate Decrypt { get; set; } = DefaultDecryptMethod;
				public delegate string DecryptDelegate(string text);
				public static string DefaultDecryptMethod(string text) => text;
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

            public static void Prepare(params JsonConverter[] converters)
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
				if (Composer.IsConfigured) throw new Exception("Narrative Progress Already Configured");

				Composer.Configure(settings);
				Composer.OnChange += InvokeChange;

				Application.quitting += QuitCallback;
			}

			public static event Action OnLoad;
			public static void Load(string slot)
			{
				Slot = slot;

				Load();

				OnLoad?.Invoke();
			}

			public static void LoadOrReset()
            {
				try
				{
					Load("Narrative Progress");
				}
				catch (Exception ex)
				{
					Debug.LogError($"Error on Loading Narrative Progress, Will Reset" +
						$"{Environment.NewLine}" +
						$"Exception: {ex}");

					Reset();
				}
			}

			static void Load()
			{
				var json = IO.Load(Slot);

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

				IO.Save(Slot, json);
			}

			#region Controls
			public static T Read<T>(string id) => Composer.Read<T>(id);
			public static object Read(string id, Type type) => Composer.Read(id, type);

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

			public static event Action OnQuit;
			static void QuitCallback()
			{
				OnQuit?.Invoke();

				if (AutoSave.OnExit) if (IsDirty) Save();
			}

			static Progress()
			{
				Composer = new JObjectComposer();
			}
		}
	}
}