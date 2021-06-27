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

using MB.UISystem;

using TMPro;

namespace MB.NarrativeSystem
{
	public class ChoiceDialogUITemplate : UITemplate<ChoiceDialogUITemplate, IChoiceData>
	{
		[SerializeField]
		TMP_Text label = default;

        [SerializeField]
        Button button = default;
        public Button Button => button;

        public override void UpdateState()
        {
            base.UpdateState();

            Rename(Data.Text);

            label.text = Data.Text;
        }
    }
}