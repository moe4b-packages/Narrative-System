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
            SetFadeState(true);
            Delay(1);
            FadeOut();

            SetSpeaker("Character 1");

            Say("Hello user");
            Say("Hope you are doing well today");
            Say("Welcome to the narrative System");
        }

        [Branch]
        void KnowAboutFeatures()
        {
            Say("Which feature would you like to know about?");

            Choice(Option1, Option2, Option3);
        }

        [Branch]
        void Option1()
        {
            Say("You chose option 1");

            GoTo(ContinueStory);
        }
        [Branch]
        void Option2()
        {
            Say("You chose option 2");

            GoTo(ContinueStory);
        }
        [Branch]
        void Option3()
        {
            Say("You chose option 3");

            GoTo(ContinueStory);
        }

        [Branch]
        void ContinueStory()
        {
            FadeIn();
            ClearDialog();
        }
    }
}