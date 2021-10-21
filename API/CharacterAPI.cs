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

using static MB.NarrativeSystem.NarrativeManager;
using static MB.NarrativeSystem.NarrativeManager.CharactersProperty;

namespace MB.NarrativeSystem
{
    partial class Narrative
    {
        public static class Characters
        {
            static CharactersProperty Manager => Narrative.Manager.Characters;

            public static List<Character> List => Manager.Collection;
            public static Dictionary<string, Character> Dictionary => Manager.Dictionary;

            public static bool TryFind(string id, out Character asset) => Dictionary.TryGetValue(id, out asset);
        }
    }

    partial class NarrativeManager
    {
        [SerializeField]
        CharactersProperty characters = new CharactersProperty();
        public CharactersProperty Characters => characters;
        [Serializable]
        public class CharactersProperty : Property
        {
            [ReadOnly]
            [SerializeField]
            List<Character> collection = new List<Character>();
            public List<Character> Collection => collection;

            public Dictionary<string, Character> Dictionary { get; } = new Dictionary<string, Character>();

            void UpdateDictionary()
            {
                Dictionary.Clear();

                for (int i = 0; i < collection.Count; i++)
                    Dictionary[collection[i].name] = collection[i];
            }

            public override void Configure()
            {
                base.Configure();

#if UNITY_EDITOR
                Refresh();
#endif

                UpdateDictionary();
            }

#if UNITY_EDITOR
            void Refresh()
            {
                var targets = AssetCollection.Query<Character>();

                if (MUtility.CheckElementsInclusion(collection, targets) == false)
                {
                    collection = targets;
                    UpdateDictionary();
                    ScriptableManagerRuntime.Save(Manager);
                }
            }
#endif
        }
    }
}