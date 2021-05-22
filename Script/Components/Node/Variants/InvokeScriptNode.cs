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
    public class InvokeScriptNode : Node
    {
        Script target;

        public override void Invoke()
        {
            base.Invoke();

            target.Play();

            if (target != Script) Script.Continue();
        }

        public InvokeScriptNode(Script target)
        {
            this.target = target;
        }
    }

    partial class Script
    {
        protected InvokeScriptNode InvokeScript(Script target) => new InvokeScriptNode(target);
        protected InvokeScriptNode InvokeScript<T>() where T : Script, new()
        {
            var story = new T();
            return InvokeScript(story);
        }
    }
}