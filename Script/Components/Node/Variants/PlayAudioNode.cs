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

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace MB.NarrativeSystem
{
    public class PlayAudioNode : Node, IDynamicResourceTarget, IWaitNode<PlayAudioNode>
    {
        public string ID { get; protected set; }

        public IEnumerable<string> DynamicResources
        {
            get
            {
                yield return ID;
            }
        }

        public float Volume { get; protected set; }
        public PlayAudioNode SetVolume(float value)
        {
            Volume = value;
            return this;
        }

        #region Wait
        public bool Wait { get; protected set; } = true;

        public PlayAudioNode SetWait(bool value)
        {
            Wait = value;
            return this;
        }

        public PlayAudioNode Await() => SetWait(true);
        public PlayAudioNode Continue() => SetWait(false);
        #endregion

        public override void Invoke()
        {
            base.Invoke();

            GlobalCoroutine.Start(Procedure);
        }

        IEnumerator Procedure()
        {
            ExecutionTimer.Start("Load Addressable Clip");
            var clip = Addressables.LoadAssetAsync<AudioClip>(ID).WaitForCompletion();
            ExecutionTimer.Stop();

            Narrative.Controls.AudioSource.PlayOneShot(clip, Volume);

            if (Wait == false) Narrative.Player.Continue();

            yield return new WaitForSeconds(clip.length);
            Addressables.Release(clip);

            if (Wait == true) Narrative.Player.Continue();
        }

        public PlayAudioNode(string id)
        {
            this.ID = id;
            Volume = 1f;
        }
    }

    partial class Script
    {
        protected PlayAudioNode PlayAudio(string address) => new PlayAudioNode(address);
    }
}