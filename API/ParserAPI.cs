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
using Newtonsoft.Json.Linq;

namespace MB.NarrativeSystem
{
    partial class Narrative
    {
        public static class Parser
        {
#if UNITY_EDITOR
            public static class Executable
            {
                public const string RelativePath = "External/Narrative Parser/MB.Narrative-System.Parser.exe";

                public static Process Start()
                {
                    var target = System.IO.Path.GetFullPath(RelativePath);

                    var solution = GetSolutionPath();
                    var arguments = MUtility.FormatProcessArguments(solution, Pipe.Name);

                    var info = new ProcessStartInfo(target, arguments);
                    var process = System.Diagnostics.Process.Start(info);

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
                    if (IsRunning) Stop();

                    Server = new NamedPipeServerStream(Name, PipeDirection.InOut);
                    Debug.Log("Pipe Server Started");

                    return Server;
                }

                public static void Stop()
                {
                    Server.Close();
                    Debug.Log("Pipe Server Closed");
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

            public static async Task<string> Process()
            {
                Pipe.Start();

                try
                {
                    using (var parser = Executable.Start())
                    {
                        parser.Exited += ParserExitCallback;
                        static void ParserExitCallback(object sender, EventArgs args)
                        {
                            Pipe.Stop();
                        }

                        await Pipe.Server.WaitForConnectionAsync();
                        Debug.Log("Pipe Server Connected");

                        var marker = new byte[sizeof(int)];
                        await Pipe.Server.ReadAsync(marker);

                        var length = BitConverter.ToInt32(marker);

                        var raw = new byte[length];
                        await Pipe.Server.ReadAsync(raw);

                        var text = Encoding.UTF8.GetString(raw);

                        return text;
                    }
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

            public class LocalizationProcessor : Localization.Extraction.Processor
            {
                public override async Task Modify(Localization.Extraction.Data data)
                {
                    var json = await Process();

                    var hashset = JObject.Parse(json)["Text"].ToObject<HashSet<string>>();

                    data.Text.UnionWith(hashset);
                }
            }
#endif
        }
    }
}