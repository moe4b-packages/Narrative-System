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

        public override void Invoke()
        {
            base.Invoke();

            if (target == Script)
                throw new Exception($"Cannot Play Script {target} from Within the Script");

            target.OnEnd += Continue;
            Narrative.Play(target);
        }

        void Continue()
        {
            target.OnEnd -= Continue;
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
            var story = new T();
            return Play(story);
        }
    }
}