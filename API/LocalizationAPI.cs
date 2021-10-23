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

using Newtonsoft.Json;

using TMPro;

using MB.LocalizationSystem;

namespace MB.NarrativeSystem
{
#if UNITY_EDITOR
    public class LocalizationExtractor : Localization.Extraction.Processor
    {
        public override HashSet<string> RetrieveText()
        {
            var hashset = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            //Scripts Nodes
            {
                foreach (var node in Narrative.Composition.IterateNodes<ILocalizationTarget>())
                {
                    foreach (var entry in node.TextForLocalization)
                    {
                        hashset.Add(entry);
                    }
                }
            }

            //Assets (Characters, ... etc)
            {
                foreach (var asset in AssetCollection.Query<ILocalizationTarget>())
                {
                    foreach (var entry in asset.TextForLocalization)
                    {
                        hashset.Add(entry);
                    }
                }
            }

            return hashset;
        }
    }
#endif
}