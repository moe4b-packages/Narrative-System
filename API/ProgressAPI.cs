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

			public static class Global
			{
				public const string ID = nameof(Global);

				static string FormatPath(string path) => JObjectComposer.Path.Compose(ID, path);

				public static T Read<T>(string id) => Composer.Read<T>(FormatPath(id));
				public static object Read(Type data, string id) => Composer.Read(data, FormatPath(id));

				public static bool Contains(string id) => Composer.Contains(FormatPath(id));

				public static bool Remove(string id) => Composer.Remove(FormatPath(id));
				public static bool RemoveAll() => Composer.Remove(ID);

				public static void Set(string id, object value) => Composer.Set(FormatPath(id), value);

				internal static void SetDefaults()
				{
					var token = Composer.Read<JToken>(ID);

					if (token == null)
					{
						token = new JObject();
						Composer.Set(ID, token);
					}
				}
			}

			public static class Scripts
			{
				public static T Read<T>(Script script, string id)
				{
					var type = script.GetType();

					return Read<T>(type, id);
				}
				public static T Read<T>(Type script, string id)
				{
					var path = FormatScriptPath(script, id);

					return Composer.Read<T>(path);
				}

				public static object Read(Script script, Type data, string id)
				{
					var type = script.GetType();

					return Read(type, data, id);
				}
				public static object Read(Type script, Type data, string id)
				{
					var path = FormatScriptPath(script, id);

					return Composer.Read(data, path);
				}

				public static bool Contains(Script script, string id)
				{
					var type = script.GetType();

					return Contains(type, id);
				}
				public static bool Contains(Type script, string id)
				{
					var path = FormatScriptPath(script, id);

					return Composer.Contains(path);
				}

				public static bool Contains(Script script)
				{
					var type = script.GetType();

					return Contains(type);
				}
				public static bool Contains(Type script)
				{
					var path = FormatScriptPath(script);

					return Composer.Contains(path);
				}

				public static bool Remove(Script script, string id)
				{
					var type = script.GetType();

					return Remove(type, id);
				}
				public static bool Remove(Type type, string id)
				{
					var path = FormatScriptPath(type, id);

					return Composer.Remove(path);
				}

				public static bool RemoveAll(Script script)
				{
					var type = script.GetType();

					return RemoveAll(type);
				}
				public static bool RemoveAll(Type type)
				{
					var path = FormatScriptPath(type);

					return Composer.Remove(path);
				}

				public static void Set(Script script, string id, object value)
				{
					var type = script.GetType();

					Set(type, id, value);
				}
				public static void Set(Type script, string id, object value)
				{
					var path = FormatScriptPath(script, id);

					Composer.Set(path, value);
				}

				static string FormatScriptPath(Type type) => Script.Format.Name.Retrieve(type);
				static string FormatScriptPath(Type type, string id) => JObjectComposer.Path.Compose(FormatScriptPath(type), id);
			}
			public static class Script<TScript>
				where TScript : Script
			{
				public static Type Type => typeof(TScript);

				public static T Read<T>(string id) => Scripts.Read<T>(Type, id);

				public static bool Contains(string id) => Scripts.Contains(Type, id);
				public static bool Contains() => Scripts.Contains(Type);

				public static bool Remove(string id) => Scripts.Remove(Type, id);
				public static bool RemoveAll() => Scripts.RemoveAll(Type);

				public static void Set(string id, object value) => Scripts.Set(Type, id, value);
			}

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

			static void Load()
			{
				var json = IO.Load(Slot);

				Composer.Load(json);

				SetDefaults();
			}

			static void SetDefaults()
			{
				Global.SetDefaults();
			}

			public static void Reset()
			{
				Composer.Clear();

				SetDefaults();

				Save();
			}

			public static void Save()
			{
				IsDirty = false;

				var json = Composer.Read();

				IO.Save(Slot, json);
			}

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