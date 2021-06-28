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
    public class ArabicScript : Script
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
        IEnumerator<Node> تحدث_بشأن_الكرستالة()
        {
            Debug.Log($"Encounter Choice: {Encounter}");

            yield return PlayAudio("SFX/Unsheath Sword");

            yield return SetFadeState(true);
            yield return Delay(1);
            yield return FadeOut();

            yield return PlayAudio("SFX/Unsheath Sword");

            //SpeakingCharacter = "Character 1";

            yield return Say("مستحيل!");
            yield return Say("لايمكن للبشر ان يستخدمو قوة الكريستالة العظيمه");
            yield return Say("حتي انا لم يسعني الا النظر الي بريقها المشع لمدة 400 سنة");

            yield return Say("لربما لك بعض الصفات التي لا تبدي للعين المجرده");

            yield return Say("مهما يكن, لا يمكنني ان اسمح لك ان تستخدم الكريستالة لي اغراض البشر");
            yield return Say("يجب ان اطلب منك ان ترجعها الي مكانها المستحق").SetAutoSubmit();

            yield return Choice(ارجع_الكرستالة, احتفظ_بالكرستاله);
        }

        [Branch]
        IEnumerator<Node> ارجع_الكرستالة()
        {
            Encounter = EncounterChoice.ReturnCrystal;

            yield return Say("انه لمن الجيد ان الكرستالة لم تتسب بافسادك");
            yield return Say("كما افسدت حياة البشر من قبل العديد من القرون");

            yield return Say("لربما ...");
            yield return Say("انت ؟");

            yield return Say("في مكان و زمن اخر ربما");

            yield return Say("شكرا لمساعدتك يا أيها البطل");

            yield return GoTo(استمر_بالقصه);
        }

        [Branch]
        IEnumerator<Node> احتفظ_بالكرستاله()
        {
            yield return Say("الكرستالة .. لقد افسدتك");
            yield return Say("كما أفسدت حياة البشر من قبل عقود مضت");

            yield return Say("لا يمكنني ان اسمح لقوتها ان تستخدم من قبل ضعيفي القلوب");

            yield return Say("هل انت مستعد لأن تموت من اجلها؟").SetAutoSubmit();

            yield return Choice(ارجع_الكرستالة, قاتل_الحارس);
        }

        [Branch]
        IEnumerator<Node> قاتل_الحارس()
        {
            Encounter = EncounterChoice.FightGuardian;

            yield return Say("فليكن ..");

            yield return Say("لن اسمح لي اي بشر عائق ان يهرب بهذه القوة");

            yield return Say("استعد للموت");

            yield return GoTo(استمر_بالقصه);
        }

        [Branch]
        IEnumerator<Node> استمر_بالقصه()
        {
            yield return FadeIn();
            yield return Say();

            yield return PlayScript<TempleConversation>().Continue();
        }
    }
}