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
	public class Character : ScriptableObject
	{
		public string DisplayName => name;

		public override string ToString() => DisplayName;
    }
}