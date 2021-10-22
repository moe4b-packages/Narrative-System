using System;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;

using UnityEngine;

namespace MB.NarrativeSystem
{
    [CreateAssetMenu]
    [Global(ScriptableManagerScope.Project)]
    [SettingsMenu(Toolbox.Paths.Root + "Narrative")]
    public partial class NarrativeManager : ScriptableManager<NarrativeManager>
    {
        [SerializeField]
        bool autoInitialize = true;
        public bool AutoInitialize => autoInitialize;

        [Serializable]
        public class Property : IReference<NarrativeManager>, IInitialize
        {
            [NonSerialized]
            protected NarrativeManager Manager;
            public virtual void Set(NarrativeManager reference)
            {
                Manager = reference;
            }

            public virtual void Configure()
            {

            }
            public virtual void Initialize()
            {

            }

#if UNITY_EDITOR
            protected internal virtual void PreProcessBuild()
            {

            }
#endif
        }

        public IEnumerable<Property> RetrieveAllProperties()
        {
            yield return characters;
            yield return progress;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            References.Set(this, RetrieveAllProperties);

            Initializer.Configure(RetrieveAllProperties);
            Initializer.Initialize(RetrieveAllProperties);
        }
    }
}