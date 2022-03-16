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

namespace MB.NarrativeSystem
{
	[AddComponentMenu(Narrative.Controls.Path + "Auto Register Narrative Controls")]
	public class AutoRegisterNarrativeControls : MonoBehaviour
	{
        [SerializeField]
        SerializedInterface<ISayDialog> say;

        [SerializeField]
        SerializedInterface<IChoiceDialog> choice;

        [SerializeField]
        FadeUI fader;

        [SerializeField]
        AudioSource audioSource;

        void Awake()
        {
            Narrative.Controls.Say = say.Interface;
            Narrative.Controls.Choice = choice.Interface;
            Narrative.Controls.Fader = fader;
            Narrative.Controls.AudioSource = audioSource;
        }
    }
}