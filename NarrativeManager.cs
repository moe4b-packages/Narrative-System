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
            yield return character;
            yield return localization;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            References.Set(this, RetrieveAllProperties);

            Configure();
            Initialize();
        }

        protected virtual void Configure()
        {
            Initializer.Configure(RetrieveAllProperties);
        }
        protected virtual void Initialize()
        {
            Initializer.Initialize(RetrieveAllProperties);
        }
    }
}