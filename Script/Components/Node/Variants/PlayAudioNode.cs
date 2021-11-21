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
    public class PlayAudioNode : Node, IDynamicResourceTarget
    {
        public string ID { get; protected set; }

        public float Volume { get; protected set; }
        public PlayAudioNode SetVolume(float value)
        {
            Volume = value;
            return this;
        }

        public NodeWaitProperty<PlayAudioNode> Wait { get; }

        IEnumerable<string> IDynamicResourceTarget.DynamicResources
        {
            get
            {
                yield return ID;
            }
        }

        protected internal override void Invoke()
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

            if (Wait.On == false) Playback.Next();

            yield return new WaitForSeconds(clip.length);
            Addressables.Release(clip);

            if (Wait.On == true) Playback.Next();
        }

        public PlayAudioNode(string id)
        {
            this.ID = id;
            Volume = 1f;

            Wait = new NodeWaitProperty<PlayAudioNode>(this);
        }
    }

    partial class Script
    {
        public static PlayAudioNode PlayAudio(string address) => new PlayAudioNode(address);
    }
}