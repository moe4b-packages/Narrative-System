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
    public class ClearDialogNode : Node
    {
        public override void Invoke()
        {
            base.Invoke();

            Narrative.Controls.Say.Clear();

            Script.Continue();
        }
    }

    public class ShowDialogNode : Node
    {
        public override void Invoke()
        {
            base.Invoke();

            Narrative.Controls.Say.Show();

            Script.Continue();
        }
    }

    public class HideDialogNode : Node
    {
        public override void Invoke()
        {
            base.Invoke();

            Narrative.Controls.Say.Hide();

            Script.Continue();
        }
    }

    partial class Script
    {
        public ClearDialogNode ClearDialog() => new ClearDialogNode();

        public ShowDialogNode ShowDialog() => new ShowDialogNode();
        public HideDialogNode HideDialog() => new HideDialogNode();
    }
}