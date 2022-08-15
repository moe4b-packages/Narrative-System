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

using UnityEngine.EventSystems;
using System.Text;

using TMPro;

using MB.LocalizationSystem;

namespace MB.NarrativeSystem
{
    [AddComponentMenu(Narrative.Controls.Path + "Say Dialog UI")]
    public class SayDialogUI : UIPanel, ISayDialog, IPointerClickHandler
    {
        [SerializeField]
        TextMeshProUGUI label = default;
        public TextMeshProUGUI Label => label;

        [SerializeField]
        float typeDelay = 0.01f;

        public int VisibleCharacters
        {
            get => label.maxVisibleCharacters;
            set => label.maxVisibleCharacters = value;
        }

        public ISayData Data { get; private set; }
        public bool IsActive => Data != null;

        public MRoutine.Handle Routine { get; private set; }
        bool IsProcessing => Routine.IsProcessing;

        public void UpdateLocalization()
        {
            //TODO Change Localization method
            //label.font = Localization.Font;
            //label.horizontalAlignment = Localization.Alignment;
            label.text = FormatDisplayText(Data);
        }

        public void Show(ISayData value)
        {
            Data = value;

            Show();

            Routine = MRoutine.Create(Procedure).Start();
            IEnumerator Procedure()
            {
                label.text = FormatDisplayText(value);

                for (int i = 0; i < label.text.Length; i++)
                {
                    VisibleCharacters = i;
                    yield return MRoutine.Wait.Seconds(typeDelay);
                }

                Finish();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsActive == false) return;

            if (IsProcessing)
                Finish();
            else
                Submit();
        }

        public void Clear()
        {
            label.text = string.Empty;
        }

        public void Finish()
        {
            if (IsProcessing)
                MRoutine.Stop(Routine);

            VisibleCharacters = 99999;

            if (Data.AutoSubmit) Submit();
        }

        public void Submit()
        {
            var callback = Data.Callback;
            Data = default;
            callback?.Invoke();
        }

        //Static Utility

        public static string FormatDisplayText(ISayData data)
        {
            return Localization.Format(data.Text, data.Format);
        }
    }
}