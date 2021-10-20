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
    [AddComponentMenu(Narrative.Controls.Path + "Choice Dialog UI")]
    public class ChoiceDialogUI : UIPanel, IChoiceDialog
    {
        [SerializeField]
        [PrefabSelection]
        GameObject template;

        [SerializeField]
        RectTransform layout;

        List<ChoiceDialogUITemplate> templates;
        
        ChoiceSubmitDelegate submit;

        public void Show<T>(ICollection<T> entries, ChoiceSubmitDelegate submit) where T : IChoiceData
        {
            Show();

            templates = ChoiceDialogUITemplate.CreateAll(template, entries, ProcessTemplate);

            this.submit = submit;
        }

        public void UpdateLocalization()
        {

        }

        void ProcessTemplate(ChoiceDialogUITemplate template, int index)
        {
            template.SetParent(layout);

            template.Button.onClick.AddListener(OnClick);

            void OnClick() => Action(index);
        }

        void Action(int index)
        {
            Hide();

            var data = templates[index].Data;

            foreach (var item in templates) Destroy(item.gameObject);

            Invoke(index, data);
        }
        void Invoke(int index, IChoiceData data)
        {
            var callback = submit;
            submit = null;
            callback?.Invoke(index, data);
        }
    }
}