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
    [AddComponentMenu(Path + "Delay")]
	public class DelayNode : FlowNode
    {
        [SerializeField]
        float duration = 1f;

        public override void Invoke()
        {
            base.Invoke();

            StartCoroutine(Procedure());
        }

        IEnumerator Procedure()
        {
            yield return new WaitForSeconds(duration);

            Chart.Continue();
        }
    }
}