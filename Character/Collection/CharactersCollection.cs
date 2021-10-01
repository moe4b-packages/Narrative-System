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
	[CreateAssetMenu(menuName = Character.Path + "Collection")]
	public class CharactersCollection : GlobalScriptableObject<CharactersCollection>, IScriptableObjectBuildPreProcess
	{
        [SerializeField]
        List<Character> list = default;
        public List<Character> Collection => list;

        public Dictionary<string, Character> Dictionary { get; private set; }

        public static bool TryFind(string id, out Character asset) => Instance.Dictionary.TryGetValue(id, out asset);

        protected override void Load()
        {
            base.Load();

#if UNITY_EDITOR
            Refresh();

            AssetCollection.OnRefresh += Refresh;
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
                EditorUtility.SetDirty(this);
            }
        }

        public void PreProcessBuild() => Refresh();
#endif

        public CharactersCollection()
        {
            list = new List<Character>();
        }
    }
}