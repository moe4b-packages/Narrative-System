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
	[CreateAssetMenu(menuName = Narrative.Path + "Character")]
	public class Character : ScriptableObject, ILocalizationTarget
	{
		public string DisplayName => name;

		public IEnumerable<string> TextForLocalization
		{
			get
			{
				yield return name;
			}
		}

		public override string ToString() => DisplayName;

		//Static Utility

		public static implicit operator Character(string name) => Find(name);

		public static Character Find(string name)
        {
			if (CharactersCollection.TryFind(name, out var character) == false)
				throw new Exception($"No Character Named {name} was Found");

			return character;
		}
	}
}