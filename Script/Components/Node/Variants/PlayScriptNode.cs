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
    public class PlayScriptNode : Node, INestedScriptTarget, IWaitNode<PlayScriptNode>
    {
        public Script Target { get; protected set; }

        public IEnumerable<Script> NestedScripts
        {
            get
            {
                yield return Target;
            }
        }

        #region Wait
        public bool Wait { get; protected set; } = true;

        public PlayScriptNode SetWait(bool value)
        {
            Wait = value;
            return this;
        }

        public PlayScriptNode Await() => SetWait(true);
        public PlayScriptNode Continue() => SetWait(false);
        #endregion

        public override void Invoke()
        {
            base.Invoke();

            if (Target == Script)
                throw new Exception($"Cannot Play Script {Target} from Within the Script");

            Script.Variables.Save();

            if (Wait)
                Target.OnEnd += TargetEndCallback;

            Narrative.Play(Target);

            if (Wait == false)
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