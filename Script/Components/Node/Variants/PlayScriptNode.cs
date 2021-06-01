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
    public class PlayScriptNode : Node
    {
        public Script Target { get; protected set; }

        public NodeWaitProperty<PlayScriptNode> Wait { get; protected set; }

        public override void Invoke()
        {
            base.Invoke();

            if (Target == Script)
                throw new Exception($"Cannot Play Script {Target} from Within the Script");

            Script.Variables.Save();

            if (Wait.Value)
                Target.OnEnd += TargetEndCallback;

            Narrative.Play(Target);

            if (Wait.Value == false)
                Script.Continue();
        }

        void TargetEndCallback()
        {
            Target.OnEnd -= TargetEndCallback;
            Script.Continue();
        }

        public PlayScriptNode(Script target)
        {
            this.Target = target;

            Wait = new NodeWaitProperty<PlayScriptNode>(this);
        }
    }

    partial class Script
    {
        protected PlayScriptNode PlayScript(Script target) => new PlayScriptNode(target);
        protected PlayScriptNode PlayScript<T>() where T : Script, new()
        {
            var script = new T();
            return PlayScript(script);
        }
    }
}