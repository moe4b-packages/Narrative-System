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
        Characters characters = new Characters();
        [Serializable]
        public class Characters
        {
            static Characters Instance => Narrative.Instance.characters;

            [ReadOnly]
            [SerializeField]
            internal List<Character> collection = new List<Character>();
            public static List<Character> Collection
            {
                get => Instance.collection;
                set => Instance.collection = value;
            }

            public static Dictionary<string, Character> Dictionary { get; } = new Dictionary<string, Character>();

            internal static void Refresh()
            {
#if UNITY_EDITOR
                var targets = AssetCollection.FindAll<Character>();

                if (MUtility.CheckElementsInclusion(Collection, targets) == false)
                {
                    Collection = targets;
                    ScriptableManagerRuntime.Save(Narrative.Instance);
                }
#endif

                Dictionary.Clear();
                Dictionary.AddAll(Collection, x => x.name);
            }

            public static bool TryFind(string id, out Character asset) => Dictionary.TryGetValue(id, out asset);
        }
    }
}