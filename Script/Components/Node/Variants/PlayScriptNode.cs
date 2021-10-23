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
    public class PlayScriptNode : Node, INestedScriptTarget
    {
        public Script Target { get; protected set; }

        public IEnumerable<Script> NestedScripts
        {
            get
            {
                yield return Target;
            }
        }

        public override void Invoke()
        {
            base.Invoke();

            Narrative.Player.Stop();
            Narrative.Player.Start(Target);
        }

        public PlayScriptNode(Script target)
        {
            this.Target = target;
        }
    }

    partial class Script
    {
        public static  PlayScriptNode PlayScript(Script target) => new PlayScriptNode(target);
        public static PlayScriptNode PlayScript<T>() where T : Script, new()
        {
            var script = new T();
            return PlayScript(script);
        }
    }
}