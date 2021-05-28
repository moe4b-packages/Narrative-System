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
	public class Variable
    {
        public VariableAttribute Attribute { get; protected set; }

        public FieldInfo Field { get; protected set; }
        public bool IsField => Field != null;

        public PropertyInfo Property { get; protected set; }
        public bool IsProperty => Property != null;

        public MemberInfo Member { get; protected set; }

        public string Name { get; protected set; }
        public Type Type { get; protected set; }

        public virtual object Target { get; }

        public object Default { get; protected set; }

        public object Value
        {
            get => Get();
            set => Set(value);
        }

        public object Get()
        {
            if (IsField)
                return Field.GetValue(Target);

            if (IsProperty)
                return Property.GetValue(Target);

            throw new NotImplementedException($"Condition State Invalid");
        }

        public void Set(object value)
        {
            if (IsField)
            {
                Field.SetValue(Target, value);
                return;
            }
            if (IsProperty)
            {
                Property.SetValue(Target, value);
                return;
            }

            throw new NotImplementedException($"Condition State Invalid");
        }

        public Variable(VariableAttribute attribute, MemberInfo member, object target)
        {
            this.Attribute = attribute;
            this.Member = member;

            Name = FormatName(member);

            Field = member as FieldInfo;
            Property = member as PropertyInfo;

            if (IsField)
            {
                Type = Field.FieldType;
            }
            if (IsProperty)
            {
                Type = Property.PropertyType;
            }

            this.Target = target;

            Default = Value;
        }

        public static string FormatName(MemberInfo info) => FormatName(info.Name);
        public static string FormatName(string text) => MUtility.PrettifyName(text);
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class VariableAttribute : Attribute
    {

    }
}