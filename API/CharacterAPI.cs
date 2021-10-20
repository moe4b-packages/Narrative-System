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
        public static NarrativeManager.CharacterProperty Character => Manager.Character;
    }

    partial class NarrativeManager
    {
        [SerializeField]
        CharacterProperty character = new CharacterProperty();
        public CharacterProperty Character => character;
        [Serializable]
        public class CharacterProperty : Property
        {
            [ReadOnly]
            [SerializeField]
            List<Character> list = new List<Character>();
            public List<Character> Collection => list;

            public Dictionary<string, Character> Dictionary { get; } = new Dictionary<string, Character>();

            void UpdateDictionary()
            {
                Dictionary.Clear();

                for (int i = 0; i < list.Count; i++)
                    Dictionary[list[i].name] = list[i];
            }

            public bool TryFind(string id, out Character asset) => Dictionary.TryGetValue(id, out asset);

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

                if (MUtility.CheckElementsInclusion(list, targets) == false)
                {
                    list = targets;
                    UpdateDictionary();
                    ScriptableManagerRuntime.Save(Manager);
                }
            }
#endif
        }
    }
}