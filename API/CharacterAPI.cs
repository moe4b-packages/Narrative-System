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
        [field: SerializeField]
        public CharactersProperty Characters { get; private set; } = new CharactersProperty();
        [Serializable]
        public class CharactersProperty
        {
            [field: ReadOnly, SerializeField]
            public List<Character> Collection { get; private set; }

            public Dictionary<string, Character> Dictionary { get; }

            internal void Refresh(Narrative instance)
            {
#if UNITY_EDITOR
                var targets = AssetCollection.FindAll<Character>();

                if (MUtility.Collections.CheckElementsInclusion(Collection, targets) == false)
                {
                    Collection = targets;
                    Runtime.Save(instance);
                }
#endif

                Dictionary.Clear();
                Dictionary.AddAll(Collection, x => x.name);
            }

            public bool TryFind(string id, out Character asset) => Dictionary.TryGetValue(id, out asset);

            public CharactersProperty()
            {
                Collection = new List<Character>();
                Dictionary = new Dictionary<string, Character>();
            }
        }
    }
}