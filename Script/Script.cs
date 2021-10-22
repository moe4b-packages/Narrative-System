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
        public const string SuffixPath = nameof(Script) + "/";

        public string Name { get; protected set; }

        public Composer.Data Composition { get; protected set; }

        public VariablesProeprty Variables { get; protected set; }
        [Serializable]
        public class VariablesProeprty
        {
            public Script Script { get; protected set; }
            public Composer.Data.VariablesData Composition => Script.Composition.Variables;

            public Variable[] List { get; protected set; }

            public VariablesProeprty(Script script)
            {
                this.Script = script;

                List = new Variable[Composition.Count];

                for (int i = 0; i < Composition.Count; i++)
                    List[i] = Variable.Assimilate(script, Composition[i].Info, SuffixPath + script.Name);
            }
        }

        public BranchesProperty Branches { get; protected set; }
        [Serializable]
        public class BranchesProperty
        {
            public Script Script { get; protected set; }
            public Composer.Data.BranchesData Composition => Script.Composition.Branches;

            public Branch[] List { get; protected set; }

            public Dictionary<string, Branch> Dictionary { get; protected set; }

            public bool TryGet(string id, out Branch branch) => Dictionary.TryGetValue(id, out branch);
            public Branch this[string id] => Dictionary[id];

            public Branch this[int index] => List[index];
            public int Count => List.Length;

            public Branch First => List.First();
            public Branch Last => List.Last();

            public BranchesProperty(Script script)
            {
                this.Script = script;

                var functions = Composition.RetrieveFunctions(script);

                List = new Branch[functions.Length];

                for (int i = 0; i < functions.Length; i++)
                    List[i] = new Branch(script, i, functions[i]);

                Dictionary = List.ToDictionary(x => x.ID);
            }
        }

        public NodesProperty Nodes { get; protected set; }
        [Serializable]
        public class NodesProperty
        {
            public Script Script { get; protected set; }

            public List<Node> List { get; protected set; }

            public Node this[int index] => List[index];
            public int Count => List.Count;

            public Node First => List.First();
            public Node Last => List.Last();

            public Node Selection { get; protected set; }
            public void Set(Node target)
            {
                Selection = target;
            }

            public NodesProperty(Script script)
            {
                this.Script = script;

                List = new List<Node>();

                for (int x = 0; x < Script.Branches.Count; x++)
                {
                    List.Capacity += Script.Branches[x].Nodes.Count;

                    for (int y = 0; y < Script.Branches[x].Nodes.Count; y++)
                        List.Add(Script.Branches[x].Nodes[y]);
                }
            }
        }

        Node Selection
        {
            get => Nodes.Selection;
            set => Nodes.Set(value);
        }

        public bool Ready { get; protected set; }

        protected bool IsPlaying => Application.isPlaying;
        protected bool IsComposing => !IsPlaying;

        public virtual void Prepare()
        {
            if (Ready)
            {
                Debug.LogWarning("Script already Ready");
                return;
            }

            Composition = Composer.Retrieve(this);

            Variables = new VariablesProeprty(this);
            Branches = new BranchesProperty(this);
            Nodes = new NodesProperty(this);

            Ready = true;
        }

        #region Flow Logic
        protected internal virtual void Play()
        {
            if (Ready == false) Prepare();

            Reset();

            if (Branches.Count == 0)
            {
                Debug.LogWarning($"{this} Has No Branches Defined");
                return;
            }

            Invoke(Branches.First);
        }

        protected virtual void Reset()
        {
            
        }

        public void Invoke(Branch.Delegate branch)
        {
            var id = Branch.Format.ID(branch);

            if (Branches.TryGet(id, out var instance) == false)
                throw new Exception($"Couldn't Find Branch with ID {id} On {this}");

            Invoke(instance);
        }
        internal void Invoke(Branch branch)
        {
            Invoke(branch.Nodes.First);
        }

        void Invoke(Node selection)
        {
            Nodes.Set(selection);
            selection.Invoke();
        }

        public void Continue()
        {
            if(Selection.Next == null)
            {
                if (Selection.Branch.Next == null)
                    End();
                else
                    Invoke(Selection.Branch.Next);
            }
            else
            {
                Invoke(Selection.Next);
            }
        }

        public virtual void Stop()
        {
            End();
        }

        public event Action OnEnd;
        protected virtual void End()
        {
            Selection = null;

            OnEnd?.Invoke();
        }
        #endregion

        public override string ToString() => Name;

        public Script()
        {
            Name = Format.Name.Retrieve(this);
        }

        #region Static Utility
        protected static T[] Arrange<T>(params T[] array) => array;

        public static class Composer
        {
            public static Dictionary<Type, Data> Dictionary { get; private set; }

            public class Data
            {
                public VariablesData Variables { get; protected set; }
                public class VariablesData
                {
                    public const BindingFlags Flags = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

                    public List<Data> List { get; protected set; }
                    public class Data
                    {
                        public VariableInfo Info { get; protected set; }

                        public Data(VariableInfo variable)
                        {
                            this.Info = variable;
                        }
                    }

                    public int Count => List.Count;
                    public Data this[int index] => List[index];

                    public VariablesData(Type type)
                    {
                        List = ParseAll(type);
                    }

                    public static List<Data> ParseAll(Type type)
                    {
                        var list = new List<Data>();

                        var variables = type.GetVariables(Flags);

                        for (int i = 0; i < variables.Count; i++)
                        {
                            if (typeof(Variable).IsAssignableFrom(variables[i].ValueType) == false) continue;

                            var data = new Data(variables[i]);

                            list.Add(data);
                        }

                        return list;
                    }
                }

                public BranchesData Branches { get; protected set; }
                public class BranchesData
                {
                    public const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

                    public List<Data> List { get; protected set; }
                    public class Data
                    {
                        public BranchAttribute Attribute { get; protected set; }

                        public MethodInfo Method { get; protected set; }

                        public Branch.Delegate CreateFunction(Script target) => Method.CreateDelegate<Branch.Delegate>(target);

                        public Data(BranchAttribute attribute, MethodInfo method)
                        {
                            this.Attribute = attribute;
                            this.Method = method;
                        }
                    }

                    public int Count => List.Count;
                    public Data this[int index] => List[index];

                    public Branch.Delegate[] RetrieveFunctions(Script target)
                    {
                        var functions = new Branch.Delegate[List.Count];

                        for (int i = 0; i < List.Count; i++)
                            functions[i] = List[i].CreateFunction(target);

                        return functions;
                    }

                    public BranchesData(Type type)
                    {
                        List = ParseAll(type);
                    }

                    public static List<Data> ParseAll(Type type)
                    {
                        var tree = ReadInheritanceTree(type);

                        var list = new List<Data>();

                        foreach (var item in tree)
                        {
                            var range = ParseSelf(item);
                            list.AddRange(range);
                        }

                        return list;
                    }

                    public static List<Data> ParseSelf(Type type)
                    {
                        var methods = type.GetMethods(Flags);

                        var list = new List<Data>();

                        for (int i = 0; i < methods.Length; i++)
                        {
                            if (BranchAttribute.TryGet(methods[i], out var attribute) == false)
                            {
                                if(methods[i].ReturnType == typeof(IEnumerator<Node>))
                                    Debug.LogWarning($"Method '{Format.Name.Retrieve(type)}->{methods[i].Name}' Has Return Type of an IEnumerator<Node> but isn't Marked as a Branch");

                                continue;
                            }

                            var data = new Data(attribute, methods[i]);

                            list.Add(data);
                        }

                        list.Sort((right, left) => right.Attribute.Line - left.Attribute.Line);

                        return list;
                    }
                }

                public Data(Type type)
                {
                    Variables = new VariablesData(type);
                    Branches = new BranchesData(type);
                }
            }

            public static Data Retrieve(Script script)
            {
                var type = script.GetType();

                return Retrieve(type);
            }
            public static Data Retrieve(Type type)
            {
                if (Dictionary.TryGetValue(type, out var composition))
                    return composition;

                composition = new Data(type);
                Dictionary[type] = composition;

                return composition;
            }

            static Stack<Type> ReadInheritanceTree(Type type)
            {
                var stack = new Stack<Type>();

                while (true)
                {
                    if (type == typeof(Script)) break;

                    stack.Push(type);

                    type = type.BaseType;

                    if (type == null) break;
                }

                return stack;
            }

            static Composer()
            {
                Dictionary = new Dictionary<Type, Data>();
            }
        }

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
            public class Drawer : PropertyDrawer
            {
                SerializedProperty FindFileProperty(SerializedProperty property) => property.FindPropertyRelative(nameof(file));

                public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
                {
                    var file = FindFileProperty(property);

                    var height = 0f;

                    height += EditorGUIUtility.singleLineHeight;

                    if (file.objectReferenceValue != null && Validate(file.objectReferenceValue) == false)
                        height += EditorGUIUtility.singleLineHeight;

                    return height;
                }

                public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
                {
                    var file = FindFileProperty(property);

                    DrawField(ref rect, label, file);

                    if (file.objectReferenceValue != null && Validate(file.objectReferenceValue) == false)
                        DrawHelpBox(ref rect);
                }

                void DrawField(ref Rect rect, GUIContent label, SerializedProperty file)
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