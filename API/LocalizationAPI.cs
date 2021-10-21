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

using static MB.NarrativeSystem.NarrativeManager;
using static MB.NarrativeSystem.NarrativeManager.LocalizationProperty;

namespace MB.NarrativeSystem
{
    partial class Narrative
    {
        public static class Localization
        {
            public const string Path = Narrative.Path + "Localization/";

            static LocalizationProperty Manager => Narrative.Manager.Localization;

            public static LocalizationEntry[] Entries => Manager.Entries;

            public static Dictionary<string, LocalizationEntry> Dictionary { get; } = new Dictionary<string, LocalizationEntry>(StringComparer.OrdinalIgnoreCase);

            public static LocalizationEntry Selection { get; private set; }

            public static TMP_FontAsset Font => Selection.Font;
            public static HorizontalAlignmentOptions Alignment => Selection.Alignment;
            public static LocalizationEntryComposition.TextDictionary Text => Selection.Composition.Text;

            const string ChoiceID = "Localization/Choice";
            public static string Choice
            {
                get
                {
                    return Progress.Read(ChoiceID, fallback: Entries[0].Title);
                }
                private set
                {
                    Progress.Set(ChoiceID, value);
                }
            }

            public static class IO
            {
#if UNITY_EDITOR
                public static void Save(TextAsset asset, LocalizationEntryComposition entry)
                {
                    var json = JsonConvert.SerializeObject(entry, Formatting.Indented);

                    asset.WriteText(json);
                }
#endif

                public static LocalizationEntryComposition Load(TextAsset asset)
                {
                    var json = asset.text;

                    if (string.IsNullOrEmpty(json))
                        return new LocalizationEntryComposition();

                    return JsonConvert.DeserializeObject<LocalizationEntryComposition>(json);
                }
            }

            internal static void Prepare()
            {
                for (int i = 0; i < Entries.Length; i++)
                {
                    var composition = IO.Load(Entries[i].Asset);
                    Entries[i].Set(composition);
                }

                Dictionary.Clear();

                for (int i = 0; i < Entries.Length; i++)
                    Dictionary.Add(Entries[i].Title, Entries[i]);

                Set(Choice);
            }

            public delegate void SetDelegate(LocalizationEntry entry);
            public static event SetDelegate OnSet;
            public static void Set(string id)
            {
                if (Dictionary.TryGetValue(id, out var entry) == false)
                    throw new Exception($"Cannot Set Localization With ID: '{id}' Because No Entry Was Registerd With That ID");

                Choice = id;

                Selection = entry;

                OnSet?.Invoke(Selection);
            }

#if UNITY_EDITOR
            [MenuItem(Path + "Extract")]
            public static void Extract()
            {
                foreach (var entry in Entries)
                    Extraction.Process(entry);
            }

            public static class Extraction
            {
                public static LocalizationEntryComposition Process(LocalizationEntry entry)
                {
                    var composition = IO.Load(entry.Asset);

                    ProcessText(composition.Text);

                    IO.Save(entry.Asset, composition);

                    return composition;
                }

                public static void ProcessText(LocalizationEntryComposition.TextDictionary text)
                {
                    //Procsss Nodes
                    {
                        var hash = new HashSet<string>();

                        foreach (var localization in Composition.IterateAllNodes<ILocalizationTarget>())
                        {
                            foreach (var node in localization.TextForLocalization)
                            {
                                if (text.ContainsKey(node) == false)
                                    text.Add(node, node);

                                hash.Add(node);
                            }
                        }

                        foreach (var key in text.Keys.ToArray())
                        {
                            if (hash.Contains(key) == false)
                                text.Remove(key);
                        }
                    }
                }
            }
#endif
        }
    }

    partial class NarrativeManager
    {
        [SerializeField]
        LocalizationProperty localization = new LocalizationProperty();
        public LocalizationProperty Localization => localization;
        [Serializable]
        public class LocalizationProperty : Property
        {
            [SerializeField]
            LocalizationEntry[] entries = Array.Empty<LocalizationEntry>();
            public LocalizationEntry[] Entries => entries;
        }
    }

    [Serializable]
    public class LocalizationEntry : ISerializationCallbackReceiver
    {
        [SerializeField]
        [HideInInspector]
        string title;
        public string Title => title;

        [SerializeField]
        TextAsset asset;
        public TextAsset Asset => asset;

        [SerializeField]
        TMP_FontAsset font;
        public TMP_FontAsset Font => font;

        [SerializeField]
        HorizontalAlignmentOptions alignment = HorizontalAlignmentOptions.Center;
        public HorizontalAlignmentOptions Alignment => alignment;

        public LocalizationEntryComposition Composition { get; protected set; }
        internal void Set(LocalizationEntryComposition reference)
        {
            Composition = reference;
        }

        public void OnBeforeSerialize()
        {
            title = asset == null ? null : asset.name;
        }
        public void OnAfterDeserialize() { }
    }

    [JsonObject]
    public class LocalizationEntryComposition
    {
        [JsonProperty]
        public TextDictionary Text { get; set; }
        [JsonArray]
        public class TextDictionary : Dictionary<string, string>
        {
            public new string this[string key]
            {
                get
                {
                    if (TryGetValue(key, out var value) == false)
                    {
                        Debug.LogWarning($"No Localization Text Found for '{key}'");
                        return key;
                    }

                    return value;
                }
            }

            public TextDictionary() : base(StringComparer.OrdinalIgnoreCase) { }
        }

        public LocalizationEntryComposition()
        {
            Text = new TextDictionary();
        }
        public LocalizationEntryComposition(TextDictionary text)
        {
            this.Text = text;
        }
    }
}