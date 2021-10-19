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

using TMPro;

namespace MB.NarrativeSystem
{
    partial class Narrative
    {
        public static NarrativeManager.LocalizationProperty Localization => Manager.Localization;
    }

    partial class NarrativeManager
    {
        [SerializeField]
        LocalizationProperty localization = new LocalizationProperty();
        public LocalizationProperty Localization => localization;
        [Serializable]
        public class LocalizationProperty : Property
        {
            public const string Path = Narrative.Path + "Localization/";

            [SerializeField]
            Entry[] entries;
            public Entry[] Entries => entries;

            public int Count => entries.Length;
            public Entry this[int index] => entries[index];

            public Dictionary<string, Entry> Dictionary { get; protected set; }
            public Entry this[string name] => Dictionary[name];

            [Serializable]
            public class Entry : ISerializationCallbackReceiver
            {
                [SerializeField]
                [HideInInspector]
                string title = default;
                public string Title => title;

                [SerializeField]
                TextAsset asset = default;
                public TextAsset Asset => asset;

                [SerializeField]
                TMP_FontAsset font = default;
                public TMP_FontAsset Font => font;

                [SerializeField]
                HorizontalAlignmentOptions alignment = HorizontalAlignmentOptions.Center;
                public HorizontalAlignmentOptions Alignment => alignment;

                public Composition Composition { get; protected set; }

                internal void Load(LocalizationProperty localization)
                {
                    Composition = localization.IO.Load(asset);
                }

                public void OnBeforeSerialize()
                {
                    title = asset == null ? null : asset.name;
                }
                public void OnAfterDeserialize() { }
            }

            public Entry Selection { get; protected set; }

            public TMP_FontAsset Font => Selection.Font;
            public HorizontalAlignmentOptions Alignment => Selection.Alignment;

            [JsonObject]
            public class Composition
            {
                [JsonProperty]
                public TextProperty.Composition Text { get; set; }

                public Composition(TextProperty.Composition text)
                {
                    this.Text = text;
                }

                public static Composition Empty
                {
                    get
                    {
                        var text = TextProperty.Composition.Empty;

                        return new Composition(text);
                    }
                }
            }

            public TextProperty Text { get; protected set; }
            [Serializable]
            public class TextProperty : Property
            {
                public IDictionary<string, string> Dictionary => Localization.Selection.Composition.Text;

                public string this[string key]
                {
                    get
                    {
                        if (Dictionary.TryGetValue(key, out var value) == false)
                        {
                            Debug.LogWarning($"No Localization Found For '{key}', Returning Key");
                            return key;
                        }

                        return value;
                    }
                }

                [JsonArray]
                public class Composition : Dictionary<string, string>
                {
                    public static Composition Empty => new Composition();

                    public Composition() : base(StringComparer.OrdinalIgnoreCase)
                    {

                    }
                }

                public Composition Extract(Composition instance)
                {
                    var hash = new HashSet<string>();

                    foreach (var localization in Narrative.Composition.IterateAllNodes<ILocalizationTarget>())
                    {
                        foreach (var entry in localization.TextForLocalization)
                        {
                            if (instance.ContainsKey(entry) == false)
                                instance.Add(entry, entry);

                            hash.Add(entry);
                        }
                    }

                    foreach (var key in instance.Keys.ToArray())
                        if (hash.Contains(key) == false)
                            instance.Remove(key);

                    return instance;
                }
            }

            public IOProperty IO { get; protected set; }
            [Serializable]
            public class IOProperty : Property
            {
#if UNITY_EDITOR
                public void Save(Composition entry, TextAsset asset)
                {
                    var json = JsonConvert.SerializeObject(entry, Formatting.Indented);

                    asset.WriteText(json);
                }
#endif

                public Composition Load(TextAsset asset)
                {
                    var json = asset.text;

                    if (string.IsNullOrEmpty(json))
                        return Composition.Empty;

                    var entry = JsonConvert.DeserializeObject<Composition>(json);

                    return entry;
                }
            }

            [Serializable]
            public class Property : IInitialize, IReference<LocalizationProperty>
            {
                [NonSerialized]
                protected LocalizationProperty Localization;
                public virtual void Set(LocalizationProperty context)
                {
                    Localization = context;
                }

                public virtual void Configure()
                {

                }

                public virtual void Initialize()
                {

                }
            }

            public IEnumerable<Property> RetrieveProperties()
            {
                yield return Text;
                yield return IO;
            }

            public void Prepare()
            {
                for (int i = 0; i < entries.Length; i++)
                    entries[i].Load(this);

                Dictionary = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);

                for (int i = 0; i < entries.Length; i++)
                    Dictionary.Add(entries[i].Title, entries[i]);

                References.Set(this, RetrieveProperties);
                Initializer.Configure(RetrieveProperties);
                Initializer.Initialize(RetrieveProperties);
            }

            public delegate void SetDelegate(Entry entry);
            public event SetDelegate OnSet;
            public void Set(string id)
            {
                if (Dictionary.TryGetValue(id, out var entry) == false)
                    throw new Exception($"Cannot Set Localization With ID: '{id}' Because No Entry Was Registerd With That ID");

                Selection = entry;

                OnSet?.Invoke(Selection);
            }

            public LocalizationProperty()
            {
                entries = new Entry[] { };

                Text = new TextProperty();
                IO = new IOProperty();
            }

            //Static Utility

#if UNITY_EDITOR
            [MenuItem(Path + "Extract")]
            public static void Extract()
            {
                var localization = Instance.localization;

                foreach (var entry in localization.entries)
                {
                    var composition = localization.IO.Load(entry.Asset);

                    composition.Text = localization.Text.Extract(composition.Text);

                    localization.IO.Save(composition, entry.Asset);
                }
            }
#endif
        }
    }
}