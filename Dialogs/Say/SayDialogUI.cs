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

        public int VisibleCharacters
        {
            get => label.maxVisibleCharacters;
            set => label.maxVisibleCharacters = value;
        }

        [SerializeField]
        float typeDelay = 0.01f;

        ISayData data;
        Action submit;

        bool IsProcessing => coroutine != null;

        public void Show(ISayData data, Action submit)
        {
            this.data = data;
            this.submit = submit;

            Show();

            coroutine = StartCoroutine(Procedure());
        }

        public void Clear()
        {
            label.text = string.Empty;
        }

        public void UpdateLocalization()
        {
            //TODO Change Localization method
            //label.font = Localization.Font;
            //label.horizontalAlignment = Localization.Alignment;
            label.text = FormatText(data);
        }

        string FormatText(ISayData data)
        {
            return Localization.Text[data.Text];
        }

        Coroutine coroutine;
        IEnumerator Procedure()
        {
            label.text = FormatText(data);

            for (int i = 0; i < label.text.Length; i++)
            {
                VisibleCharacters = i;
                yield return new WaitForSeconds(typeDelay);
            }

            Finish();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsProcessing)
                Finish();
            else
                Submit();
        }

        void Finish()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }

            VisibleCharacters = 99999;
            label.text = FormatText(data);

            if (data.AutoSubmit) Submit();
        }

        void Submit()
        {
            var callback = submit;
            submit = null;
            callback?.Invoke();
        }
    }
}