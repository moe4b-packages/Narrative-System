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
using MB.LocalizationSystem;

namespace MB.NarrativeSystem
{
	[AddComponentMenu(Narrative.ControlsProperty.Path + "Choice Dialog UI Template")]
	public class ChoiceDialogUITemplate : UITemplate<ChoiceDialogUITemplate, (IChoiceData Root, int Index)>
	{
		[SerializeField]
		TMP_Text label = default;

        [SerializeField]
        Button button = default;
        public Button Button => button;

        public IChoiceEntry Entry => Data.Root.Retrieve(Data.Index);

        public override void SetData((IChoiceData Root, int Index) data)
        {
            base.SetData(data);

            Rename(Entry.Text);
        }

        public override void UpdateState()
        {
            base.UpdateState();

            label.text = FormatDisplayText(Data.Root, Entry);
        }

        public void UpdateLocalization() => UpdateState();

        //Static Utility

        static Localization Localization => Localization.Instance;

        public static string FormatDisplayText(IChoiceData data, IChoiceEntry entry)
        {
            return Localization.Format(entry.Text, data.Format);
        }
    }
}