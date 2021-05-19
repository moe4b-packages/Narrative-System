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
    [AddComponentMenu(Path + "Invoke Flowchart")]
    public class InvokeFlowchartNode : FlowNode
	{
		[SerializeField]
		Flowchart target;

        public override void Invoke()
        {
            base.Invoke();

            target.Invoke();

            if (target != Chart) Chart.Continue();
        }
    }
}