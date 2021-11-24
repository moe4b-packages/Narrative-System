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

using Newtonsoft.Json;

using TMPro;

using MB.LocalizationSystem;

using System.Threading.Tasks;
using System.IO.Pipes;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System.Text;
using System.Threading;
using System.Diagnostics.CodeAnalysis;

namespace MB.NarrativeSystem
{
    partial class Narrative
    {
        public static class Localization
        {
#if UNITY_EDITOR
            public static class Extraction
            {
                public static class Parser
                {
                    public const string RelativePath = "External/Narrative Parser/MB.Narrative-System.Parser.exe";

                    public static Process Start()
                    {
                        var target = System.IO.Path.GetFullPath(RelativePath);

                        var solution = GetSolutionPath();
                        var arguments = MUtility.FormatProcessArguments(solution, Pipe.Name);

                        var info = new ProcessStartInfo(target, arguments);
                        var process = Process.Start(info);

                        process.EnableRaisingEvents = true;

                        return process;
                    }
                }

                public static class Pipe
                {
                    public const string Name = "MB Narrative Analysis";

                    public static NamedPipeServerStream Server { get; private set; }

                    public static bool IsRunning => Server != null;

                    public static NamedPipeServerStream Start()
                    {
                        if (Server != null) Stop();

                        Server = new NamedPipeServerStream(Name, PipeDirection.InOut);

                        return Server;
                    }

                    public static void Stop()
                    {
                        Server.Close();
                        Server = null;
                    }

                    static void BeforeAssemblyReloadCallback()
                    {
                        if (IsRunning) Stop();
                    }

                    static Pipe()
                    {
                        AssemblyReloadEvents.beforeAssemblyReload += BeforeAssemblyReloadCallback;
                    }
                }

                public static async Task<string> AnalyzeNarrative()
                {
                    Pipe.Start();

                    try
                    {
                        Debug.Log("Pipe Server Started");

                        var parser = Parser.Start();

                        await Pipe.Server.WaitForConnectionAsync();

                        var marker = new byte[sizeof(int)];
                        await Pipe.Server.ReadAsync(marker);

                        var length = BitConverter.ToInt32(marker);
                        Debug.Log($"Receiving {length} Long Message");

                        var raw = new byte[length];
                        await Pipe.Server.ReadAsync(raw);

                        var text = Encoding.UTF8.GetString(raw);

                        Debug.Log($"Recieved JSON:" +
                            $"{Environment.NewLine}" +
                            $"{text}");

                        return text;
                    }
                    catch
                    {
                        return default;
                    }
                    finally
                    {
                        Pipe.Stop();
                    }
                }

                static string GetSolutionPath()
                {
                    var file = System.IO.Path.GetFullPath($"{Application.productName}.sln");

                    return file;
                }
            }
#endif
        }
    }

#if UNITY_EDITOR
    public class LocalizationExtractor : Localization.Extraction.Processor
    {
        public override async Task<HashSet<string>> RetrieveText()
        {
            //TODO implement new localization extractor for narrative system

            var hashset = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            return hashset;
        }
    }
#endif
}