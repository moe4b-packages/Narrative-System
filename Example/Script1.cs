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

namespace MB.NarrativeSystem
{
    [Serializable]
    public class Script1 : Script
    {
        [Branch]
        void Introduction()
        {
            SpeakingCharacter = "Character 1";

            SetFadeState(true);
            Delay(1);
            FadeOut();

            Say("Hello user");
            Say("Hope you are doing well today");
            Say("Welcome to the narrative System");
        }

        [Branch]
        void KnowAboutFeatures()
        {
            Say("Which feature would you like to know about?").SetAutoSubmit(true);

            Choice(Option1, Option2, Option3);
        }

        [Branch]
        void Option1()
        {
            Say("You chose option 1");

            GoTo(Restart);
        }
        [Branch]
        void Option2()
        {
            Say("You chose option 2");

            GoTo(Restart);
        }
        [Branch]
        void Option3()
        {
            Say("You chose option 3");

            GoTo(Restart);
        }

        [Branch]
        void Restart()
        {
            FadeIn();

            Say();

            InvokeScript(this);
        }
    }
}