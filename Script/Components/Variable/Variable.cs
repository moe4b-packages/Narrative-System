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

using System.Reflection;
using UnityEngine.Scripting;

namespace MB.NarrativeSystem
{
    [Preserve]
    public abstract class Variable
    {
        public VariableInfo Info { get; protected set; }
        public string Name { get; protected set; }
        public string Path { get; protected set; }

        public Type Type => Info.ValueType;

        protected static Narrative Narrative => Narrative.Instance;

        internal void Configure(VariableInfo info, string segment)
        {
            this.Info = info;

            Name = FormatName(Info.Member);
            Path = JObjectComposer.Path.Compose(segment, Name);

            Load();
        }

        public bool IsAssigned { get; protected set; }

        public abstract void Save();
        public abstract void Load();

        //Static Utility

        public static string FormatName(MemberInfo info) => FormatName(info.Name);
        public static string FormatName(string text) => MUtility.Text.Prettify(text);

        internal static Variable Assimilate(object target, VariableInfo info, string segment)
        {
            var variable = info.Read(target) as Variable;

            if (variable == null)
            {
                variable = Activator.CreateInstance(info.ValueType) as Variable;
                info.Set(target, variable);
            }

            variable.Configure(info, segment);

            return variable;
        }

        public static Variable<T> Assign<T>(T value) => new Variable<T>(value);
    }

    [Preserve]
    public class Variable<T> : Variable
    {
        T value;
        public T Value
        {
            get => value;
            set
            {
                IsAssigned = true;

                this.value = value;

                Save();
            }
        }

        public override void Save()
        {
            Narrative.Progress.Set(Path, Value);
        }
        public override void Load()
        {
            if (Narrative.Progress.Contains(Path) == false)
            {
                IsAssigned = false;
                value = default;
            }
            else
            {
                IsAssigned = true;
                Value = Narrative.Progress.Read<T>(Path);
            }
        }

        public override string ToString()
        {
            if (IsAssigned == false)
                return $"[ {Name}: No Value ]";

            return $"[ {Name}: {Value} ]";
        }

        public Variable() : this(default, false) { }
        public Variable(T value) : this(value, true) { }
        protected Variable(T value, bool isAssigned)
        {
            this.value = value;
            this.IsAssigned = isAssigned;
        }

        //Static Utility

        public static implicit operator T (Variable<T> variable) => variable.Value;
    }
}