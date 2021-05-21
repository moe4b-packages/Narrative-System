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

            yield return SetFadeState.On;
            yield return Delay.Compose(1);
            yield return FadeOut.Compose();

            yield return Say.Compose("Hello user", SpeakingCharacter);
            yield return Say.Compose("Hope you are doing well today", SpeakingCharacter);
            yield return Say.Compose("Welcome to the narrative System", SpeakingCharacter);
        }

        IEnumerable KnowAboutFeatures()
        {
            yield return Say.Compose("Which feature would you like to know about?", SpeakingCharacter).SetAutoSubmit(true);

            yield return Choice.Compose()
                .Add(Option1)
                .Add(Option2)
                .Add(Option3);
        }

        IEnumerable Option1()
        {
            yield return Say.Compose("You chose option 1", SpeakingCharacter);

            yield return Restart();
        }
        IEnumerable Option2()
        {
            yield return Say.Compose("You chose option 2", SpeakingCharacter);

            yield return Restart();
        }
        IEnumerable Option3()
        {
            yield return Say.Compose("You chose option 3", SpeakingCharacter);

            yield return Restart();
        }

        IEnumerable Restart()
        {
            yield return FadeIn.Compose();

            yield return Say.Clear;

            yield return InvokeFlowchart.Compose(this);
        }
    }
}