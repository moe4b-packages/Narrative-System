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

				static string RetrievePath(string path) => $"{ID}{JObjectComposer.Path.Seperator}{path}";

				public static bool TryRead<T>(string id, out T value) => Composer.TryRead(RetrievePath(id), out value);

				public static T Read<T>(string id) => Composer.Read<T>(RetrievePath(id));

				public static bool Contains(string id) => Composer.Contains(RetrievePath(id));

				public static bool Remove(string id) => Composer.Remove(RetrievePath(id));
				public static bool RemoveAll() => Composer.Remove(ID);

				public static void Set<T>(string id, T value) => Composer.Set(RetrievePath(id), value);

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
				public static bool TryRead<T>(Script script, string id, out T value)
				{
					var type = script.GetType();

					return TryRead(type, id, out value);
				}
				public static bool TryRead<T>(Type script, string id, out T value)
				{
					var path = RetrieveScriptPath(script, id);

					return Composer.TryRead(path, out value);
				}

				public static T Read<T>(Script script, string id)
				{
					var type = script.GetType();

					return Read<T>(type, id);
				}
				public static T Read<T>(Type script, string id)
				{
					var path = RetrieveScriptPath(script, id);

					return Composer.Read<T>(path);
				}

				public static bool Contains(Script script, string id)
				{
					var type = script.GetType();

					return Contains(type, id);
				}
				public static bool Contains(Type script, string id)
				{
					var path = RetrieveScriptPath(script, id);

					return Composer.Contains(path);
				}

				public static bool Contains(Script script)
				{
					var type = script.GetType();

					return Contains(type);
				}
				public static bool Contains(Type script)
				{
					var path = RetrieveScriptPath(script);

					return Composer.Contains(path);
				}

				public static bool Remove(Script script, string id)
				{
					var type = script.GetType();

					return Remove(type, id);
				}
				public static bool Remove(Type type, string id)
				{
					var path = RetrieveScriptPath(type, id);

					return Composer.Remove(path);
				}

				public static bool RemoveAll(Script script)
				{
					var type = script.GetType();

					return RemoveAll(type);
				}
				public static bool RemoveAll(Type type)
				{
					var path = RetrieveScriptPath(type);

					return Composer.Remove(path);
				}

				public static void Set<T>(Script script, string id, T value)
				{
					var type = script.GetType();

					Set(type, id, value);
				}
				public static void Set<T>(Type script, string id, T value)
				{
					var path = RetrieveScriptPath(script, id);

					Composer.Set(path, value);
				}

				static string RetrieveScriptPath(Type type) => Script.Format.Name.Retrieve(type);
				static string RetrieveScriptPath(Type type, string id) => RetrieveScriptPath(type) + JObjectComposer.Path.Seperator + id;
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

			public static void Load(string slot)
			{
				Slot = slot;

				Load();
			}

			static void SetDefaults()
			{
				Global.SetDefaults();
			}

			static void Load()
			{
				var json = IO.Load(Slot);

				Composer.Load(json);

				SetDefaults();
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

			static void QuitCallback()
			{
				if (AutoSave.OnExit) if (IsDirty) Save();
			}

			static Progress()
			{
				Composer = new JObjectComposer();
			}
		}
	}
}