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

namespace MB.NarrativeSystem
{
    public class SayDialogUI : UIPanel, ISayDialog, IPointerClickHandler
    {
        [SerializeField]
        TextMeshProUGUI label = default;
        public TextMeshProUGUI Label => label;

        public string Text
        {
            get => label.text;
            set => label.text = value;
        }

        public int MaxVisibleCharacters
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

        Coroutine coroutine;
        IEnumerator Procedure()
        {
            Text = FormatText(data);

            for (int i = 0; i < Text.Length; i++)
            {
                MaxVisibleCharacters = i;

                yield return new WaitForSeconds(typeDelay);
            }

            Finish();
        }

        void Finish()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }

            MaxVisibleCharacters = 99999;
            Text = FormatText(data);

            if (data.AutoSubmit) Submit();
        }

        void Submit()
        {
            var callback = submit;
            submit = null;
            callback?.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsProcessing)
                Finish();
            else
                Submit();
        }

        //Static Utility
        static string FormatText(ISayData data)
        {
            if (data.Character == null)
                return data.Text;
            else
                return $"{data.Character}: {data.Text}";
        }
    }
}