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
            //TODO implement new localization extractor for narrative system

            var hashset = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            return hashset;
        }
    }
#endif
}