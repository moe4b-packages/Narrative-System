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

using MB.NarrativeSystem;

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

            public int NodeCapacity
            {
                get
                {
                    var value = 0;

                    for (int i = 0; i < List.Count; i++)
                        value += List[i].Nodes.Count;

                    return value;
                }
            }

            public BranchesProperty(Script script)
            {
                List = script.Assemble().ToList();
                Dictionary = List.ToDictionary(x => x.ID);

                for (int i = 0; i < List.Count; i++)
                    List[i].Set(script, i);
            }
        }

        public NodesProperty Nodes { get; protected set; }
        [Serializable]
        public class NodesProperty
        {
            public Node[] Collection { get; protected set; }

            public Node this[int index] => Collection[index];
            public int Count => Collection.Length;

            public Node First => Collection.Length == 0 ? null : Collection[0];
            public Node Last => Collection.Length == 0 ? null : Collection[Collection.Length - 1];

            public NodesProperty(BranchesProperty branches)
            {
                Collection = new Node[branches.NodeCapacity];

                var index = 0;

                for (int b = 0; b < branches.Count; b++)
                {
                    for (int n = 0; n < branches[b].Nodes.Count; n++)
                    {
                        Collection[index] = branches[b].Nodes[n];
                        Collection[index].Set(branches[b], index);

                        index += 1;
                    }
                }
            }
        }

        public int Progress { get; internal set; }

        public Node Selection
        {
            get => Nodes[Progress];
            set
            {
                if (value == null)
                    Progress = 0;
                else
                    Progress = value.Index;
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
            Nodes = new NodesProperty(Branches);

            Ready = true;
        }

        public abstract IEnumerable<Branch> Assemble();

        #region Writing Utility
        public Character SpeakingCharacter { get; protected set; }
        #endregion

        #region Flow Logic
        public void Invoke(int progress = 0)
        {
            if (Ready == false) Prepare();

            if (Nodes.Collection.TryGet(progress, out var node) == false)
                throw new Exception($"Invalid Progress of {node} Loaded on '{this}'");

            Invoke(node);
        }

        public delegate void InvokeDelegate(Node node);
        public event InvokeDelegate OnInvoke;
        void Invoke(Node node)
        {
            Selection = node;
            Selection.Invoke();

            OnInvoke?.Invoke(node);
        }

        public void Continue()
        {
            if (Selection.Next == null)
                End();
            else
                Invoke(Selection.Next);
        }
        public void Continue(Branch branch) => Continue(branch.Nodes.First);
        public void Continue(Node node) => Invoke(node);

        public void Stop()
        {
            End();
        }

        public event Action OnEnd;
        protected void End()
        {
            Selection = null;

            OnEnd?.Invoke();
        }
        #endregion

        public Script()
        {
            
        }

        //Static Utility

        public static T[] Arrange<T>(params T[] array) => array;

        //Utility Types

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
    }
}