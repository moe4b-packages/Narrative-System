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
        IEnumerator<Node> Introduction()
        {
            yield return SetFadeState(true);
            yield return Delay(1);
            yield return FadeOut();

            SpeakingCharacter = "Character 1";

            yield return Say("Hello user");
            yield return Say("Hope you are doing well today");
            yield return Say("Welcome to the narrative System");
        }

        [Branch]
        IEnumerator<Node> KnowAboutFeatures()
        {
            yield return Say("Which feature would you like to know about?").SetAutoSubmit();

            yield return Choice(Option1, Option2, Option3);
        }

        [Branch]
        IEnumerator<Node> Option1()
        {
            yield return Say("You chose option 1");

            yield return GoTo(ContinueStory);
        }
        [Branch]
        IEnumerator<Node> Option2()
        {
            yield return Say("You chose option 2");

            yield return GoTo(ContinueStory);
        }
        [Branch]
        IEnumerator<Node> Option3()
        {
            yield return Say("You chose option 3");

            yield return GoTo(ContinueStory);
        }

        [Branch]
        IEnumerator<Node> ContinueStory()
        {
            yield return FadeIn();
            yield return Say();
        }
    }
}