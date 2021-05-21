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

            yield return Say.Compose("Impossible!", SpeakingCharacter);
            yield return Say.Compose("The crystal cannot be held by a mortal like you", SpeakingCharacter);
            yield return Say.Compose("Even I could only glance at its majestic glow for the last 400 years", SpeakingCharacter);

            yield return Say.Compose("Perhaps there is more to you than meets the eye", SpeakingCharacter);

            yield return Say.Compose("Nevertheless, the crystal cannot be exploited by the means of humans", SpeakingCharacter);
            yield return Say.Compose("I request that you return it to its righteous place, young hero", SpeakingCharacter).SetAutoSubmit();
            
            yield return Choice.Compose(ReturnTheCrystal, KeepTheCrystal);
        }

        IEnumerable ReturnTheCrystal()
        {
            yield return Say.Compose("It's good to see that its power hasn't currpoted " +
                "you as it has currpoted the lives of men many ages ago", SpeakingCharacter);

            yield return Say.Compose("Could it be ...", SpeakingCharacter);
            yield return Say.Compose("You ...", SpeakingCharacter);

            yield return Say.Compose("At another time & place perhaps", SpeakingCharacter);

            yield return Say.Compose("Thank you for your help hero", SpeakingCharacter);
        }

        IEnumerable KeepTheCrystal()
        {
            yield return Say.Compose("The crystal must've currpoted you", SpeakingCharacter);
            yield return Say.Compose("As it has currpoted the lives of men many ages before", SpeakingCharacter);

            yield return Say.Compose("I cannot allow its power to be exploited by those weak of will", SpeakingCharacter);

            yield return Say.Compose("Are you ready to die for it?", SpeakingCharacter).SetAutoSubmit();

            yield return Choice.Compose(ReturnTheCrystal, FightTheGuardian);
        }

        IEnumerable FightTheGuardian()
        {
            yield return Say.Compose("So it is then", SpeakingCharacter);

            yield return Say.Compose("I will not let a shura roam free", SpeakingCharacter);

            yield return Say.Compose("Prepare to Die", SpeakingCharacter);
        }
    }
}