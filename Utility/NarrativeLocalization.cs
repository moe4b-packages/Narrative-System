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
	[CreateAssetMenu(menuName = Narrative.Path + "Localization")]
	public class NarrativeLocalization : GlobalScriptableObject<NarrativeLocalization>
	{
		public const string Path = Narrative.Path + "Localization/";

		[SerializeField]
		UList<Entry> entries = default;
        public UList<Entry> Entries => entries;

        [Serializable]
		public class Entry : ISerializationCallbackReceiver
        {
			[SerializeField]
			[HideInInspector]
			string title;

			[SerializeField]
            TextAsset asset = default;
            public TextAsset Asset => asset;

			public Composition Composition { get; protected set; }

			public void OnBeforeSerialize()
            {
				if (asset == null)
					title = null;
				else
					title = asset.name;
            }
			public void OnAfterDeserialize() { }
        }

		[JsonObject]
		public class Composition
		{
			[JsonProperty]
			public List<Text.Entry> Text { get; set; }

			//Static Utility

			public static Composition Empty
			{
				get
				{
					return new Composition()
					{
						Text = new List<Text.Entry>(),
					};
				}
			}
		}

		public static void Load(Entry entry)
		{

		}

		public static class Text
        {
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

			public static List<Entry> Extract(List<Entry> list)
			{
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

				list = dictionary.Select(Entry.From).ToList();

				return list;
			}
		}

		[MenuItem(Path + "Extract")]
		public static void Extract()
		{
			foreach (var file in Instance.Entries)
			{
				var entry = IO.Load(file.Asset);

				entry.Text = Text.Extract(entry.Text);

				IO.Save(entry, file.Asset);
			}
		}

		[MenuItem(Path + "Select")]
		public static void Select()
        {
			Selection.activeObject = Instance;
        }

		public static class IO
		{
			public static void Save(Composition entry, TextAsset asset)
			{
				var json = JsonConvert.SerializeObject(entry, Formatting.Indented);

				asset.WriteText(json);
			}

			public static Composition Load(TextAsset asset)
			{
				var json = asset.text;

				if (string.IsNullOrEmpty(json))
					return Composition.Empty;

				var entry = JsonConvert.DeserializeObject<Composition>(json);

				return entry;
			}
		}
	}
}