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

namespace MB.NarrativeSystem
{
	public static class ScriptLocalization
	{
		public static class Extractor
		{
			public const string Path = "Assets/Localization/Default.json";

			public struct Entry
			{
				[JsonProperty]
				public string Key;

				[JsonProperty]
				public string Value;

				public Entry(string key, string value)
				{
					this.Key = key;
					this.Value = value;
				}

				public static Entry From(KeyValuePair<string, string> pair) => new Entry(pair.Key, pair.Value);
			}

			[MenuItem("Narrative System/Extract Localization")]
			public static void Process()
			{
				var list = Load();

				var dictionary = list.ToDictionary(x => x.Key, x => x.Value);
				var hash = new HashSet<string>();

				foreach (var localization in Narrative.Composition.IterateAllNodes<ILocalizationTarget>())
				{
					foreach (var entry in localization.TextForLocalization)
					{
						if (dictionary.ContainsKey(entry) == false)
							dictionary.Add(entry, entry);

						hash.Add(entry);
					}
				}

				foreach (var key in dictionary.Keys.ToArray())
					if (hash.Contains(key) == false)
						dictionary.Remove(key);

				dictionary.ToArray();

				list = dictionary.Select(Entry.From).ToList();

				Save(list);
			}

			public static List<Entry> Load()
			{
				if (File.Exists(Path) == false)
					return new List<Entry>();

				var json = File.ReadAllText(Path);

				if (string.IsNullOrEmpty(json))
					return new List<Entry>();

				var dictionary = JsonConvert.DeserializeObject<List<Entry>>(json);

				return dictionary;
			}

			public static void Save(List<Entry> dictionary)
			{
				var json = JsonConvert.SerializeObject(dictionary, Formatting.Indented);

				new FileInfo(Path).Directory.Create();

				File.WriteAllText(Path, json);
			}
		}
	}
}