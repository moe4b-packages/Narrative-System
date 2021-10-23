using UnityEngine;

#region Aliases
using Body = System.Collections.Generic.IEnumerable<MB.NarrativeSystem.Script.Block>;
using static MB.RichTextMarker;
#endregion

using MB.NarrativeSystem;

namespace MB.NarrativeSystem
{
    [System.Serializable]
    public class Script1 : Script
    {
        [Branch]
        Body Introduction()
        {
            yield return SetFadeState(true);
            yield return Delay(1);
            yield return FadeOut();

            yield return SetSpeaker("Character 1");

            yield return Say("Hello user");
            yield return Say("Hope you are doing well today");
            yield return Say("Welcome to the narrative System");
        }

        [Branch]
        Body KnowAboutFeatures()
        {
            yield return Say("Which feature would you like to know about?");

            yield return Choice(Option1, Option2, Option3);
        }

        [Branch]
        Body Option1()
        {
            yield return Say("You chose option 1");

            yield return GoTo(ContinueStory);
        }
        [Branch]
        Body Option2()
        {
            yield return Say("You chose option 2");

            yield return GoTo(ContinueStory);
        }
        [Branch]
        Body Option3()
        {
            yield return Say("You chose option 3");

            yield return GoTo(ContinueStory);
        }

        [Branch]
        Body ContinueStory()
        {
            yield return FadeIn();
            yield return ClearDialog();
        }
    }
}