using System;
using UnityEngine;

#region Aliases
using Body = System.Collections.Generic.IEnumerable<MB.NarrativeSystem.Script.Block>;
using static MB.RichTextMarker;
#endregion

using MB.NarrativeSystem;
using System.Collections.Generic;
using UnityEngine.Scripting;
using System.Collections;

namespace MB.NarrativeSystem
{
    [System.Serializable]
    public class TempleConversation : Script
    {
        public static Variable<EncounterChoice> Encounter { get; set; } = Variable.Assign(EncounterChoice.None);
        [Flags]
        public enum EncounterChoice
        {
            None,
            ReturnCrystal,
            FightGuardian
        }

        [Branch]
        Body Intro()
        {
            yield return Log(Encounter);

            yield return SetFadeState(true);
            yield return Delay(1);
            yield return FadeOut();

            yield return PlayAudio("SFX/Unsheath Sword").Wait.Continue();

            yield return Say("Hello {player}, How are you doing on this fine day?").
                Format.Add("player", "Moe4B");

            yield return Say("Im Fine {companion}").
                Format.Add("companion", "Parvarti");
        }

        [Branch]
        Body TalkAboutCrystal()
        {
            yield return SetSpeaker("Character 1");

            yield return Say("Impossible!");
            yield return Say("<b>The crystal</b> cannot be held by a mortal like you");
            yield return Say("Even I could only glance at its majestic glow for the last <b>400 years</b>");

            yield return Say("Perhaps there is more to you than meets the eye");

            yield return Say("Nevertheless, <b>the crystal</b> cannot be exploited by the means of humans");
            yield return Say("I request that you return it to its righteous place, young hero").SetAutoSubmit();

            yield return Choice().
                Add("Keep the Crystal", KeepTheCrystal).
                Add("Return the Crystal", ReturnTheCrystal);
        }

        [Branch]
        Body ReturnTheCrystal()
        {
            yield return SetVariable(Encounter, EncounterChoice.ReturnCrystal);

            yield return Say("It's good to see that its power hasn't corrupted you");
            yield return Say("As it has currpoted the lives of men many ages ago");

            yield return Say("Could it be ...");
            yield return Say("You ...");

            yield return Say("At another time & place maybe");

            yield return Say("Thank you for your help hero");

            yield return GoTo(ContinueStory);
        }

        [Branch]
        Body KeepTheCrystal()
        {
            yield return Say("<b>The crystal</b> must've corrupted you");
            yield return Say("As it has corrupted the lives of men many ages before");

            yield return Say("I cannot allow its power to be exploited by those weak of heart");

            yield return Say("Are you ready to die for it?").SetAutoSubmit();

            yield return Choice().
                Add("Return the Crystal", ReturnTheCrystal).
                Add("Fight the Guardian", FightTheGuardian);
        }

        [Branch]
        Body FightTheGuardian()
        {
            yield return SetVariable(Encounter, EncounterChoice.FightGuardian);

            yield return Say("So be it");

            yield return Say("I will not let a shura roam free");

            yield return Say("Prepare to Die");

            yield return GoTo(ContinueStory);
        }

        [Branch]
        Body ContinueStory()
        {
            yield return RaiseEvent("Exit Temple");

            yield return FadeIn();
            yield return HideDialog();

            yield return PlayScript<TempleConversation>();
        }
    }
}