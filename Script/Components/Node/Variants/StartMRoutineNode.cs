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
    public class StartMRoutineNode : Node
    {
        public IEnumerator Numerator { get; protected set; }

        public NodeWaitProperty<StartMRoutineNode> Wait { get; private set; }

        protected internal override void Invoke()
        {
            base.Invoke();

            MRoutine.Create(Procedure).Start();
        }

        IEnumerator Procedure()
        {
            if (Wait.On == false) Playback.Next();

            yield return Numerator;

            if (Wait.On == true) Playback.Next();
        }

        public StartMRoutineNode(IEnumerator numerator)
        {
            this.Numerator = numerator;

            this.Wait = new NodeWaitProperty<StartMRoutineNode>(this);
        }
    }

    partial class Script
    {
        [NarrativeConstructorMethod]
        public static StartMRoutineNode StartMRoutine(Func<IEnumerator> function)
        {
            var numerator = function();

            return StartMRoutine(numerator);
        }

        [NarrativeConstructorMethod]
        public static StartMRoutineNode StartMRoutine(IEnumerator numerator)
        {
            return new StartMRoutineNode(numerator);
        }
    }
}