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
        public override IEnumerable<Branch> Assemble()
        {
            yield return Branch.From(TalkAboutCrystal);
            yield return Branch.From(ReturnTheCrystal);
            yield return Branch.From(KeepTheCrystal);
            yield return Branch.From(FightTheGuardian);
        }

        IEnumerable TalkAboutCrystal()
        {
            SpeakingCharacter = "Character 1";

            yield return Say("Impossible!");
            yield return Say("The crystal cannot be held by a mortal like you");
            yield return Say("Even I could only glance at its majestic glow for the last 400 years");

            yield return Say("Perhaps there is more to you than meets the eye");

            yield return Say("Nevertheless, the crystal cannot be exploited by the means of humans");
            yield return Say("I request that you return it to its righteous place, young hero").SetAutoSubmit();

            yield return Choice(ReturnTheCrystal, KeepTheCrystal);
        }

        IEnumerable ReturnTheCrystal()
        {
            yield return Say("It's good to see that its power hasn't corrupted " +
                "you as it has currpoted the lives of men many ages ago");

            yield return Say("Could it be ...");
            yield return Say("You ...");

            yield return Say("At another time & place perhaps");

            yield return Say("Thank you for your help hero");

            yield return ContinueStory();
        }

        IEnumerable KeepTheCrystal()
        {
            yield return Say("The crystal must've corrupted you");
            yield return Say("As it has corrupted the lives of men many ages before");

            yield return Say("I cannot allow its power to be exploited by those weak of heart");

            yield return Say("Are you ready to die for it?").SetAutoSubmit();

            yield return Choice(ReturnTheCrystal, FightTheGuardian);
        }

        IEnumerable FightTheGuardian()
        {
            yield return Say("So be it");

            yield return Say("I will not let a shura roam free");

            yield return Say("Prepare to Die");

            yield return ContinueStory();
        }

        IEnumerable ContinueStory()
        {
            yield return InvokeScript<Script1>();
        }
    }
}