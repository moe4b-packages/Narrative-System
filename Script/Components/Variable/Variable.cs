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

namespace MB.NarrativeSystem
{
    public abstract class Variable
    {
        public VariableInfo Info { get; protected set; }
        internal void Set(VariableInfo info, string segment)
        {
            this.Info = info;

            Name = FormatName(Info.Member);
            Path = JObjectComposer.Path.Compose(segment, Name);

            Load();
        }

        public string Name { get; protected set; }
        public string Path { get; protected set; }

        public Type Type => Info.ValueType;

        public bool HasValue { get; protected set; }

        public abstract object ManagedValue { get; set; }

        public abstract void Save();
        public abstract void Load();

        public override string ToString()
        {
            if (HasValue == false)
                return "NULL";

            return ManagedValue.ToString();
        }

        //Static Utility

        public static string FormatName(MemberInfo info) => FormatName(info.Name);
        public static string FormatName(string text) => MUtility.PrettifyName(text);

        public static Variable Assimilate(object target, VariableInfo info, string segment)
        {
            var variable = info.Read(target) as Variable;

            if(variable == null)
            {
                variable = Activator.CreateInstance(info.ValueType) as Variable;
                info.Set(target, variable);
            }

            variable.Set(info, segment);

            return variable;
        }
    }

    public class Variable<T> : Variable
    {
        T value;
        public T Value
        {
            get => value;
            set
            {
                this.value = value;

                Save();
            }
        }

        public override object ManagedValue
        {
            get => value;
            set
            {
                Value = (T)value;
            }
        }

        public override void Save()
        {
            Narrative.Progress.Set(Path, Value);
        }

        public override void Load()
        {
            if (Narrative.Progress.Contains(Path) == false) return;

            HasValue = true;
            Value = Narrative.Progress.Read<T>(Path);
        }

        public Variable() { }
        public Variable(T value) : this()
        {
            this.value = value;
        }

        //Static Utility

        public static implicit operator T (Variable<T> variable) => variable.Value;
    }
}