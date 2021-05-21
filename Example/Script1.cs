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

using MB.NarrativeSystem;

namespace MB.NarrativeSystem
{
    [Serializable]
    public class Script1 : Script
    {
        public override IEnumerable<Branch> Assemble()
        {
            yield return Branch.From(Introduction);

            yield return Branch.From(KnowAboutFeatures);

            yield return Branch.From(Option1);
            yield return Branch.From(Option2);
            yield return Branch.From(Option3);
        }

        IEnumerable Introduction()
        {
            SpeakingCharacter = "Character 1";

            yield return SetFadeState(true);
            yield return Delay(1);
            yield return FadeOut();

            yield return Say("Hello user");
            yield return Say("Hope you are doing well today");
            yield return Say("Welcome to the narrative System");
        }

        IEnumerable KnowAboutFeatures()
        {
            yield return Say("Which feature would you like to know about?").SetAutoSubmit(true);

            yield return Choice()
                .Add(Option1)
                .Add(Option2)
                .Add(Option3);
        }

        IEnumerable Option1()
        {
            yield return Say("You chose option 1");

            yield return Restart();
        }
        IEnumerable Option2()
        {
            yield return Say("You chose option 2");

            yield return Restart();
        }
        IEnumerable Option3()
        {
            yield return Say("You chose option 3");

            yield return Restart();
        }

        IEnumerable Restart()
        {
            yield return FadeIn();

            yield return Say();

            yield return InvokeScript(this);
        }
    }
}