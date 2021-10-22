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

namespace MB.NarrativeSystem
{
    partial class Narrative
    {
        [SerializeField]
        CharactersProperty characters = new CharactersProperty();
        [Serializable]
        class CharactersProperty
        {
            [ReadOnly]
            [SerializeField]
            internal List<Character> collection = new List<Character>();

            internal Dictionary<string, Character> dictionary = new Dictionary<string, Character>();

            void UpdateDictionary()
            {
                dictionary.Clear();

                for (int i = 0; i < collection.Count; i++)
                    dictionary[collection[i].name] = collection[i];
            }

            internal void Load()
            {
                Refresh();
            }

            void Refresh()
            {
#if UNITY_EDITOR
                var targets = AssetCollection.Query<Character>();

                if (MUtility.CheckElementsInclusion(collection, targets) == false)
                {
                    collection = targets;
                    ScriptableManagerRuntime.Save(Instance);
                }
#endif

                UpdateDictionary();
            }
        }
        public static class Characters
        {
            static CharactersProperty Instance => Narrative.Instance.characters;

            public static List<Character> List => Instance.collection;
            public static Dictionary<string, Character> Dictionary => Instance.dictionary;

            internal static void Load() => Instance.Load();

            public static bool TryFind(string id, out Character asset) => Dictionary.TryGetValue(id, out asset);
        }
    }
}