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
	[CreateAssetMenu(menuName = Path + "Asset")]
	public class Character : ScriptableObject, ILocalizationTarget
	{
		public const string Path = Narrative.Path + "Character/";

		public string ID => name;

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

		public static Character Find(string name)
        {
			if (Narrative.Character.TryFind(name, out var character) == false)
				throw new Exception($"No Character Named {name} was Found");

			return character;
		}
	}
}