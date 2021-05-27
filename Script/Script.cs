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
using UnityEngine.UIElements;
using System.Text;

namespace MB.NarrativeSystem
{
    [Serializable]
    public abstract partial class Script
    {
        public string Name { get; protected set; }

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
                    var entry = new Branch(functions[i], script, i);

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
        protected internal virtual void Play()
        {
            if (Ready == false) Prepare();

            Clear();

            if (Branches.Count == 0)
            {
                Debug.LogWarning($"{this} Has No Branches Defined");
                return;
            }

            Invoke(Branches.First);
        }

        protected virtual void Clear()
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
            var id = Branch.Format.ID(branch);

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

        public override string ToString() => Name;

        public Script()
        {
            Name = Format.Name.Retrieve(this);
        }

        #region Static Utility
        public static T[] Arrange<T>(params T[] array) => array;

        public static class Format
        {
            public static class Name
            {
                public static Dictionary<Type, string> Dictionary { get; private set; }

                public const string Seperator = "/";

                public static string Retrieve(Script script)
                {
                    var type = script.GetType();

                    return Retrieve(type);
                }
                public static string Retrieve(Type type)
                {
                    if (Dictionary.TryGetValue(type, out var name))
                        return name;

                    name = Path.Retrieve(type).Aggregate(FormatParts);

                    Dictionary[type] = name;

                    return name;
                }

                static string FormatParts(string x, string y) => $"{x}{Seperator}{y}";

                static Name()
                {
                    Dictionary = new Dictionary<Type, string>();
                }
            }

            public static class Path
            {
                public static Dictionary<Type, Collection> Dictionary { get; private set; }

                public class Collection : List<string> { }

                public static Collection Retrieve(Script script)
                {
                    var type = script.GetType();

                    return Retrieve(type);
                }
                public static Collection Retrieve(Type type)
                {
                    if (Dictionary.TryGetValue(type, out var collection))
                        return collection;

                    collection = new Collection();
                    Dictionary[type] = collection;

                    ProcessNamespace(type, collection);
                    ProcessTitle(type, collection);

                    for (int i = 0; i < collection.Count; i++)
                        collection[i] = MUtility.PrettifyName(collection[i]);

                    return collection;
                }

                static void ProcessNamespace(Type type, Collection collection)
                {
                    var range = type.Namespace.Split('.');

                    collection.AddRange(range);
                }

                static void ProcessTitle(Type type, Collection collection)
                {
                    var range = IterateNesting(type).Reverse().Select(FormatName);

                    collection.AddRange(range);

                    static IEnumerable<Type> IterateNesting(Type type)
                    {
                        while (true)
                        {
                            yield return type;

                            type = type.DeclaringType;

                            if (type == null) break;
                        }
                    }

                    static string FormatName(Type type)
                    {
                        var name = type.Name;

                        var index = name.IndexOf('`');

                        if (index >= 0)
                            name = name.Substring(0, index);

                        return name;
                    }
                }

                static Path()
                {
                    Dictionary = new Dictionary<Type, Collection>();
                }
            }
        }
        #endregion

        #region Utility Types
        /// <summary>
        /// Varaible for holding any script, for a script to be selectable by this,
        /// it must be in its own file with a name matching the script name
        /// </summary>
        [Serializable]
        public class Asset : ISerializationCallbackReceiver
        {
            [SerializeField]
            Object file = default;
            public Object File
            {
                get => file;
                set => file = value;
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
                    if (cached == false)
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

            public Script CreateInstance(params object[] args)
            {
                return CreateInstance<Script>(args);
            }
            public T CreateInstance<T>(params object[] args)
                where T : Script
            {
                return Activator.CreateInstance(Type, args) as T;
            }

#if UNITY_EDITOR
            public void Refresh()
            {
                var script = file as MonoScript;

                if (script == null)
                {
                    Type = null;
                    return;
                }

                if (Validate(file))
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

            public override string ToString()
            {
                if (Type == null)
                    return "null";

                return Format.Name.Retrieve(Type);
            }

#if UNITY_EDITOR
            public static bool Validate(Object asset)
            {
                var script = asset as MonoScript;

                var type = script.GetClass();

                if (type == null)
                    return false;

                if (type.IsAbstract)
                    return false;

                if (type == typeof(Script))
                    return false;

                if (typeof(Script).IsAssignableFrom(type) == false)
                    return false;

                return true;
            }

            [CustomPropertyDrawer(typeof(Asset))]
            public class Drawer : PersistantPropertyDrawer
            {
                SerializedProperty file;

                protected override void Init()
                {
                    base.Init();

                    file = property.FindPropertyRelative(nameof(file));
                }

                protected override float CalculateHeight()
                {
                    var height = 0f;

                    height += EditorGUIUtility.singleLineHeight;

                    if (file.objectReferenceValue != null && Validate(file.objectReferenceValue) == false)
                        height += EditorGUIUtility.singleLineHeight;

                    return height;
                }

                protected override void Draw(Rect rect)
                {
                    DrawField(ref rect, label);

                    if (file.objectReferenceValue != null && Validate(file.objectReferenceValue) == false)
                        DrawHelpBox(ref rect);
                }

                void DrawField(ref Rect rect, GUIContent label)
                {
                    var area = MUtility.GUICoordinates.SliceLine(ref rect);

                    EditorGUI.ObjectField(area, file, typeof(MonoScript), label);
                }

                void DrawHelpBox(ref Rect rect)
                {
                    var area = MUtility.GUICoordinates.SliceLine(ref rect);

                    EditorGUI.HelpBox(area, "Invalid Script Selection", MessageType.Error);
                }
            }
#endif
        }

        public struct Surrogate
        {
            public Script Script { get; private set; }

            public Surrogate(Script script)
            {
                this.Script = script;
            }

            public static implicit operator Script(Surrogate surrogate) => surrogate.Script;

            public static implicit operator Surrogate(Script script) => new Surrogate(script);
            public static implicit operator Surrogate(Asset script)
            {
                var instance = script.CreateInstance();

                return new Surrogate(instance);
            }
        }
        #endregion
    }
}