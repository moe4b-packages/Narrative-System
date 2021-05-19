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
    [AddComponentMenu(Path + "Log")]
	public class LogNode : FlowNode
    {
		[SerializeField]
		string text = default;

        public override void Invoke()
        {
            base.Invoke();

            Debug.Log(text);

            Chart.Continue();
        }
    }
}