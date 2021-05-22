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
    public class Script2 : Script
    {
        [Branch]
        void TalkAboutCrystal()
        {
            SpeakingCharacter = "Character 1";

            Say("Impossible!");
            Say("The crystal cannot be held by a mortal like you");
            Say("Even I could only glance at its majestic glow for the last 400 years");

            Say("Perhaps there is more to you than meets the eye");

            Say("Nevertheless, the crystal cannot be exploited by the means of humans");
            Say("I request that you return it to its righteous place, young hero");

            Choice(ReturnTheCrystal, KeepTheCrystal);
        }

        [Branch]
        void ReturnTheCrystal()
        {
            Say("It's good to see that its power hasn't corrupted " +
                "you as it has currpoted the lives of men many ages ago");

            Say("Could it be ...");
            Say("You ...");

            Say("At another time & place perhaps");

            Say("Thank you for your help hero");

            GoTo(ContinueStory);
        }

        [Branch]
        void KeepTheCrystal()
        {
            Say("The crystal must've corrupted you");
            Say("As it has corrupted the lives of men many ages before");

            Say("I cannot allow its power to be exploited by those weak of heart");

            Say("Are you ready to die for it?");

            Choice(ReturnTheCrystal, FightTheGuardian);
        }

        [Branch]
        void FightTheGuardian()
        {
            Say("So be it");

            Say("I will not let a shura roam free");

            Say("Prepare to Die");

            GoTo(ContinueStory);
        }

        [Branch]
        void ContinueStory()
        {
            FadeIn();

            Say();

            GoTo(TalkAboutCrystal);
            //InvokeScript<Script1>();
        }
    }
}