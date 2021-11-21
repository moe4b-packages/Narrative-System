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
        protected internal override void Invoke()
        {
            base.Invoke();

            Narrative.Controls.Say.Clear();

            Playback.Next();
        }
    }

    public class ShowDialogNode : Node
    {
        protected internal override void Invoke()
        {
            base.Invoke();

            Narrative.Controls.Say.Show();

            Playback.Next();
        }
    }

    public class HideDialogNode : Node
    {
        protected internal override void Invoke()
        {
            base.Invoke();

            Narrative.Controls.Say.Hide();

            Playback.Next();
        }
    }

    partial class Script
    {
        public static ClearDialogNode ClearDialog() => new ClearDialogNode();

        public static ShowDialogNode ShowDialog() => new ShowDialogNode();
        public static HideDialogNode HideDialog() => new HideDialogNode();
    }
}