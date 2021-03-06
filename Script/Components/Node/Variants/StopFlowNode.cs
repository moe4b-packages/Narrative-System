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
    public class StopFlowNode : Node
    {
        protected internal override void Invoke()
        {
            base.Invoke();

            Playback.Stop();
        }
    }

    partial class Script
    {
        [NarrativeConstructorMethod]
        public static StopFlowNode StopFlow() => new StopFlowNode();
    }
}