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

using static MB.RichTextMarker;

namespace MB.NarrativeSystem
{
    [Serializable]
    public class TempleConversation : Script
    {
        public static Variable<EncounterChoice> Encounter { get; set; }
        [Flags]
        public enum EncounterChoice
        {
            None,
            ReturnCrystal,
            FightGuardian
        }

        [Branch]
        void Intro()
        {
            Callback(() => Debug.Log($"Encounter Choice: {Encounter}"));

            SetFadeState(true);
            Delay(1);
            FadeOut();

            PlayAudio("SFX/Unsheath Sword").Continue();
        }

        [Branch]
        void TalkAboutCrystal()
        {
            SetSpeaker("Character 1");

            Say("Impossible!");
            Say("<b>The crystal</b> cannot be held by a mortal like you");
            Say("Even I could only glance at its majestic glow for the last <b>400 years</b>");

            Say("Perhaps there is more to you than meets the eye");

            Say("Nevertheless, <b>the crystal</b> cannot be exploited by the means of humans");
            Say("I request that you return it to its righteous place, young hero");

            Choice(ReturnTheCrystal, KeepTheCrystal);
        }

        [Branch]
        void ReturnTheCrystal()
        {
            SetVariable(Encounter, EncounterChoice.ReturnCrystal);

            Say("It's good to see that its power hasn't corrupted you");
            Say("As it has currpoted the lives of men many ages ago");

            Say("Could it be ...");
            Say("You ...");

            Say("At another time & place maybe");

            Say("Thank you for your help hero");

            GoTo(ContinueStory);
        }

        [Branch]
        void KeepTheCrystal()
        {
            Say("<b>The crystal</b> must've corrupted you");
            Say("As it has corrupted the lives of men many ages before");

            Say("I cannot allow its power to be exploited by those weak of heart");

            Say("Are you ready to die for it?");

            Choice(ReturnTheCrystal, FightTheGuardian);
        }

        [Branch]
        void FightTheGuardian()
        {
            SetVariable(Encounter, EncounterChoice.FightGuardian);

            Say("So be it");

            Say("I will not let a shura roam free");

            Say("Prepare to Die");

            GoTo(ContinueStory);
        }

        [Branch]
        void ContinueStory()
        {
            RaiseEvent("Exit Temple");

            FadeIn();
            HideDialog();

            PlayScript<TempleConversation>().Continue();
        }
    }
}