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
    [Serializable]
    public abstract partial class Script
    {
        public BranchesProperty Branches { get; protected set; }
        [Serializable]
        public class BranchesProperty
        {
            public List<Branch> List { get; protected set; }

            public Dictionary<string, Branch> Dictionary { get; protected set; }

            public bool TryGet(string id, out Branch branch) => Dictionary.TryGetValue(id, out branch);
            public Branch this[string id] => Dictionary[id];

            public Branch this[int index] => List[index];
            public int Count => List.Count;

            public Branch First => List.SafeIndexer(0);
            public Branch Last => List.SafeIndexer(List.Count - 1);

            public Branch Selection { get; protected set; }
            public IEnumerator<Node> Numerator { get; protected set; }

            internal void SetSelection(Branch target)
            {
                Selection = target;

                if (Selection == null)
                    Numerator = null;
                else
                    Numerator = Selection.GetEnumerator();
            }

            public BranchesProperty(Script script)
            {
                var functions = Branch.Composition.Read(script);

                List = new List<Branch>(functions.Count);

                for (int i = 0; i < functions.Count; i++)
                {
                    var id = Branch.FormatID(functions[i]);

                    var entry = new Branch(id, functions[i], script, i);

                    List.Add(entry);
                }

                Dictionary = List.ToDictionary(x => x.ID);
            }
        }

        public bool Ready { get; protected set; }

        public virtual void Prepare()
        {
            if (Ready)
            {
                Debug.LogWarning("Script already Ready");
                return;
            }

            Branches = new BranchesProperty(this);

            Ready = true;
        }

        #region Writing Utility
        public Character SpeakingCharacter { get; protected set; }
        #endregion

        #region Flow Logic
        public virtual void Play()
        {
            if (Ready == false) Prepare();

            Clear();

            if(Branches.Count == 0)
            {
                Debug.LogWarning($"{this} Has No Branches Defined");
                return;
            }

            Invoke(Branches.First);
        }

        public virtual void Clear()
        {

        }

        void Invoke(Branch branch)
        {
            Branches.SetSelection(branch);

            Continue();
        }

        void Invoke(Node node)
        {
            node.Set(Branches.Selection);
            node.Invoke();
        }

        public void Continue()
        {
            if (Branches.Numerator.MoveNext())
                Invoke(Branches.Numerator.Current);
            else if (Branches.Selection.Next == null)
                End();
            else
                Continue(Branches.Selection.Next);
        }
        public void Continue(Branch.Delegate branch)
        {
            var id = Branch.FormatID(branch);

            if (Branches.TryGet(id, out var instance) == false)
                throw new Exception($"Couldn't Find Branch with ID {id} On {this}");

            Continue(instance);
        }
        public void Continue(Branch branch) => Invoke(branch);

        public void Stop()
        {
            End();
        }

        public event Action OnEnd;
        protected void End()
        {
            Branches.SetSelection(null);

            OnEnd?.Invoke();
        }
        #endregion

        public Script()
        {
            
        }

        #region Static Utility
        public static T[] Arrange<T>(params T[] array) => array;
        #endregion

        #region Utility Types
        /// <summary>
        /// Varaible for holding any script, for a script to be selectable by this,
        /// it must be in its own file with a name matching the script name
        /// </summary>
        [Serializable]
        public class Asset : ISerializationCallbackReceiver
        {
            public static Type TargetType => typeof(Script);

            [SerializeField]
            Object asset = default;
            public Object File
            {
                get => asset;
                set
                {
                    asset = value;
                }
            }

            [SerializeField]
            string id = string.Empty;
            public string ID => id;

            Type cache;
            bool cached;

            public Type Type
            {
                get
                {
                    if(cached == false)
                    {
                        if (string.IsNullOrEmpty(ID))
                            throw new Exception($"Invalid Script Asset Selection, Cannot Parse Type");

                        cache = Type.GetType(ID);
                        cached = true;
                    }

                    return cache;
                }
                set
                {
                    if (value == null)
                        id = string.Empty;
                    else
                        id = value.AssemblyQualifiedName;

                    cached = false;
                }
            }

            public Script CreateInstance(params object[] args) => CreateInstance<Script>(args);
            public T CreateInstance<T>(params object[] args) where T : Script => Activator.CreateInstance(Type, args) as T;

#if UNITY_EDITOR
            public void Refresh()
            {
                var script = asset as MonoScript;

                if (script == null)
                {
                    Type = null;
                    return;
                }

                if (Validate(asset))
                    Type = script.GetClass();
                else
                    Type = null;
            }
#endif

            public void OnBeforeSerialize()
            {
#if UNITY_EDITOR
                Refresh();
#endif
            }
            public void OnAfterDeserialize() { }

            public override string ToString() => Type.ToString();

#if UNITY_EDITOR
            public static bool Validate(Object asset)
            {
                var script = asset as MonoScript;

                var type = script.GetClass();

                if (typeof(Script).IsAssignableFrom(type) == false)
                    return false;

                if (type == typeof(Script))
                    return false;

                return true;
            }

            [CustomPropertyDrawer(typeof(Asset))]
            public class Drawer : PropertyDrawer
            {
                SerializedProperty property;

                SerializedProperty asset;

                void Init(SerializedProperty reference)
                {
                    if (property?.propertyPath == reference?.propertyPath) return;

                    property = reference;

                    asset = property.FindPropertyRelative(nameof(asset));
                }

                public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
                {
                    Init(property);

                    var height = 0f;

                    height += EditorGUIUtility.singleLineHeight;

                    if (asset.objectReferenceValue != null && Validate(asset.objectReferenceValue) == false)
                        height += EditorGUIUtility.singleLineHeight;

                    return height;
                }

                public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
                {
                    DrawField(ref rect, label);

                    if (asset.objectReferenceValue != null && Validate(asset.objectReferenceValue) == false)
                        DrawHelpBox(ref rect);

                }

                void DrawField(ref Rect rect, GUIContent label)
                {
                    var area = MUtility.GUICoordinates.SliceLine(ref rect);

                    EditorGUI.ObjectField(area, asset, typeof(MonoScript), label);
                }

                void DrawHelpBox(ref Rect rect)
                {
                    var area = MUtility.GUICoordinates.SliceLine(ref rect);

                    EditorGUI.HelpBox(area, "Invalid Script Selection", MessageType.Error);
                }
            }
#endif
        }
        #endregion
    }
}