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
	[CreateAssetMenu(menuName = Narrative.Path + "Characters Collection")]
	public class CharactersCollection : GlobalScriptableObject<CharactersCollection>, IInitialize, IScriptableObjectBuildPreProcess
	{
        [SerializeField]
        List<Character> list = default;
        public List<Character> Collection => list;

        public Dictionary<string, Character> Dictionary { get; private set; }

        public static bool TryFind(string id, out Character asset) => Instance.Dictionary.TryGetValue(id, out asset);

        public void Configure()
        {
#if UNITY_EDITOR
            Refresh();
#endif

            Dictionary = list.ToDictionary(x => x.name);
        }

        public void Init() { }

#if UNITY_EDITOR
        void Refresh()
        {
            list = ScriptableObjectQuery.FindAll<Character>();

            EditorUtility.SetDirty(this);
        }

        public void PreProcessBuild() => Refresh();
#endif

        public CharactersCollection()
        {
            list = new List<Character>();
        }
    }
}