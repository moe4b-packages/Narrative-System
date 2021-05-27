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

using MB.UISystem;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Text;
using System.Text.RegularExpressions;

namespace MB.NarrativeSystem
{
	public static partial class Narrative
	{
		public const string Path = Toolbox.Path + "Narrative System/";

		#region Play
		public static Script[] PlayAll(params Script.Asset[] assets)
		{
			var scripts = Array.ConvertAll(assets, x => x.CreateInstance());

			PlayAll(scripts);

			return scripts;
		}
		public static void PlayAll(params Script[] scripts)
		{
			Iterate(0);

			void Iterate(int index)
			{
				if (scripts.ValidateCollectionBounds(index) == false) return;

				scripts[index].OnEnd += Continue;
				void Continue() => Iterate(index + 1);

				Play(scripts[index]);
			}
		}

		public static T Play<T>()
			where T : Script, new()
		{
			var instance = new T();

			Play(instance);

			return instance;
		}
		public static Script Play(Script.Surrogate surrogate)
		{
			surrogate.Script.Play();

			return surrogate.Script;
		}
		#endregion
	}
}