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
    [AddComponentMenu(Narrative.ControlsProperty.Path + "Choice Dialog UI")]
    public class ChoiceDialogUI : UIPanel, IChoiceDialog
    {
        [SerializeField]
        [PrefabSelection]
        GameObject template;

        [SerializeField]
        RectTransform layout;

        public List<ChoiceDialogUITemplate> Templates { get; private set; }
        public IChoiceData Data { get; private set; }

        public override void Configure()
        {
            base.Configure();

            Templates = new List<ChoiceDialogUITemplate>();
        }

        public void UpdateLocalization()
        {
            foreach (var template in Templates)
                template.UpdateLocalization();
        }

        public MRoutine.Handle Show<T>(T item) where T : IChoiceData
        {
            Data = item;

            Templates.Clear();

            for (int i = 0; i < Data.Count; i++)
            {
                var index = i;

                var entry = ChoiceDialogUITemplate.Create(template, (Data, index));
                entry.SetParent(layout);
                entry.transform.localScale = Vector3.one;

                entry.Button.onClick.AddListener(OnClick);
                void OnClick() => Invoke(index);

                Templates.Add(entry);
            }

            return Show();
        }

        public void Invoke(int index)
        {
            Hide();

            var entry = Data.Retrieve(index);
            var callback = Data.Callback;

            foreach (var item in Templates) Destroy(item.gameObject);
            Templates.Clear();

            Data = default;

            callback?.Invoke(index, entry);
        }
    }
}