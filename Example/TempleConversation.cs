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
        [Variable]
        public static EncounterChoice Encounter { get; set; }
        [Flags]
        public enum EncounterChoice
        {
            None,
            ReturnCrystal,
            FightGuardian
        }

        [Branch]
        IEnumerator<Node> TalkAboutCrystal()
        {
            Debug.Log($"Encounter Choice: {Encounter}");

            yield return SetFadeState(true);
            yield return Delay(1);
            yield return FadeOut();

            yield return PlayAudio("SFX/Unsheath Sword");
            yield return PlayAudio("SFX/Unsheath Sword");

            SpeakingCharacter = "Character 1";

            yield return Say("Impossible!");
            yield return Say("<b>The crystal</b> cannot be held by a mortal like you");
            yield return Say("Even I could only glance at its majestic glow for the last <b>400 years</b>");

            yield return Say("Perhaps there is more to you than meets the eye");

            yield return Say("Nevertheless, <b>the crystal</b> cannot be exploited by the means of humans");
            yield return Say("I request that you return it to its righteous place, young hero").SetAutoSubmit();

            yield return Choice(ReturnTheCrystal, KeepTheCrystal);
        }

        [Branch]
        IEnumerator<Node> ReturnTheCrystal()
        {
            Encounter = EncounterChoice.ReturnCrystal;

            yield return Say("It's good to see that its power hasn't corrupted you");
            yield return Say("As it has currpoted the lives of men many ages ago");

            yield return Say("Could it be ...");
            yield return Say("You ...");

            yield return Say("At another time & place maybe");

            yield return Say("Thank you for your help hero");

            yield return GoTo(ContinueStory);
        }

        [Branch]
        IEnumerator<Node> KeepTheCrystal()
        {
            yield return Say("<b>The crystal</b> must've corrupted you");
            yield return Say("As it has corrupted the lives of men many ages before");

            yield return Say("I cannot allow its power to be exploited by those weak of heart");

            yield return Say("Are you ready to die for it?").SetAutoSubmit();

            yield return Choice(ReturnTheCrystal, FightTheGuardian);
        }

        [Branch]
        IEnumerator<Node> FightTheGuardian()
        {
            Encounter = EncounterChoice.FightGuardian;

            yield return Say("So be it");

            yield return Say("I will not let a shura roam free");

            yield return Say("Prepare to Die");

            yield return GoTo(ContinueStory);
        }

        [Branch]
        IEnumerator<Node> ContinueStory()
        {
            yield return FadeIn();
            yield return Say();

            yield return PlayScript<TempleConversation>().Continue();
        }
    }
}