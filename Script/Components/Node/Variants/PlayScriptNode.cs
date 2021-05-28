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
        Script target;

        bool wait = true;

        public PlayScriptNode DoWait() => SetWait(true);
        public PlayScriptNode DontWait() => SetWait(false);
        public PlayScriptNode SetWait(bool value)
        {
            wait = value;
            return this;
        }

        public override void Invoke()
        {
            base.Invoke();

            if (target == Script)
                throw new Exception($"Cannot Play Script {target} from Within the Script");

            Script.Variables.Save();

            if (wait)
                target.OnEnd += TargetEndCallback;

            Narrative.Play(target);

            if (wait == false)
                Script.Continue();
        }

        void TargetEndCallback()
        {
            target.OnEnd -= TargetEndCallback;
            Script.Continue();
        }

        public PlayScriptNode(Script target)
        {
            this.target = target;
        }
    }

    partial class Script
    {
        protected PlayScriptNode Play(Script target) => new PlayScriptNode(target);
        protected PlayScriptNode Play<T>() where T : Script, new()
        {
            var script = new T();
            return Play(script);
        }
    }
}