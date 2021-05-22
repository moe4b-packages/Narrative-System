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
	public class AutoStartScript : MonoBehaviour
	{
		[SerializeField]
		Script.Asset asset;

        void Start()
        {
            var script = Narrative.Play(asset);

            script.OnEnd += EndCallback;
        }

        void EndCallback()
        {
            Debug.Log($"{asset} Ended");
        }
    }
}