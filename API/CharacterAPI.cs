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
        [ReadOnly]
        [SerializeField]
        CharacterProperty character = new CharacterProperty();
        public CharacterProperty Character => character;
        [Serializable]
        public class CharacterProperty : Property
        {
            [SerializeField]
            List<Character> list;
            public List<Character> Collection => list;

            public Dictionary<string, Character> Dictionary { get; private set; }

            public bool TryFind(string id, out Character asset) => Dictionary.TryGetValue(id, out asset);

            public override void Configure()
            {
                base.Configure();

#if UNITY_EDITOR
                Refresh();
#endif

                Dictionary = list.ToDictionary(x => x.ID);
            }

#if UNITY_EDITOR
            void Refresh()
            {
                var targets = AssetCollection.Query<Character>();

                if (MUtility.CheckElementsInclusion(list, targets) == false)
                {
                    list = targets;
                    Dictionary = list.ToDictionary(x => x.name);
                    ScriptableManagerRuntime.Save(Manager);
                }
            }
#endif

            public CharacterProperty()
            {
                list = new List<Character>();
            }
        }
    }
}