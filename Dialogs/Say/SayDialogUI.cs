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

namespace MB.NarrativeSystem
{
    public class SayDialogUI : UIPanel, ISayDialog, IPointerClickHandler
    {
        [SerializeField]
        Text label = default;
        public Text Label => label;

        [SerializeField]
        float typeDelay = 0.01f;

        ISayData data;
        Action submit;

        public void Show(ISayData data, Action submit)
        {
            this.data = data;
            this.submit = submit;

            Show();

            coroutine = StartCoroutine(Procedure());
        }

        Coroutine coroutine;
        bool IsProcessing => coroutine != null;
        IEnumerator Procedure()
        {
            var text = FormatText(data);

            for (int i = 0; i < text.Length + 1; i++)
            {
                label.text = text.Insert(i, "<color=#0000>") + "</color>";

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

            label.text = FormatText(data);

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