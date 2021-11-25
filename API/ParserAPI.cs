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
#if UNITY_EDITOR
        public static class Parser
        {
            public static class Executable
            {
                public const string RelativePath = "External/Narrative Parser/MB.Narrative-System.Parser.exe";

                public static Process Start()
                {
                    var target = System.IO.Path.GetFullPath(RelativePath);

                    var solution = GetSolutionPath();
                    var arguments = MUtility.FormatProcessArguments(solution, Pipe.Name);

                    var info = new ProcessStartInfo(target, arguments);
                    info.CreateNoWindow = true;
                    info.UseShellExecute = false;
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

                    Server = new NamedPipeServerStream(Name, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                    Debug.Log("Pipe Server Started");

                    return Server;
                }

                public static void Stop()
                {
                    if (IsRunning == false) return;

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

            static bool IsRunning = false;
            public static async Task<Structure> Process()
            {
                if (IsRunning) throw new InvalidOperationException("Narrative Parsing Already in Progress");
                IsRunning = true;

                Pipe.Start();

                try
                {
                    using var parser = Executable.Start();

                    var cancellation = new CancellationTokenSource();

                    parser.Exited += InvokeCancellation;
                    void InvokeCancellation(object sender, EventArgs args)
                    {
                        Debug.LogError($"Narrative System Parsing Cancelled, Process Halted Early");
                        cancellation.Cancel();
                    }

                    await Pipe.Server.WaitForConnectionAsync(cancellationToken: cancellation.Token);
                    Debug.Log("Pipe Server Connected");

                    var marker = new byte[sizeof(int)];
                    await Pipe.Server.ReadAsync(marker, cancellationToken: cancellation.Token);

                    var length = BitConverter.ToInt32(marker);

                    var raw = new byte[length];
                    await Pipe.Server.ReadAsync(raw, cancellationToken: cancellation.Token);
                    Debug.Log("Pipe Server Recieved Data");

                    parser.Exited -= InvokeCancellation;

                    await Pipe.Server.WriteAsync(new byte[1]);

                    var text = Encoding.UTF8.GetString(raw);

                    var structure = Structure.Parse(text);
                    return structure;
                }
                finally
                {
                    Pipe.Stop();
                    IsRunning = false;
                }
            }

            [JsonObject]
            public class Structure
            {
                [JsonProperty]
                public HashSet<string> Text { get; set; }

                public Structure()
                {

                }

                public static Structure Parse(string json)
                {
                    if (string.IsNullOrEmpty(json))
                        return null;

                    var structure = JsonConvert.DeserializeObject<Structure>(json);
                    return structure;
                }
            }

            static string GetSolutionPath()
            {
                var file = System.IO.Path.GetFullPath($"{Application.productName}.sln");

                return file;
            }

            public class LocalizationProcessor : Localization.Extraction.Processor
            {
                public override async Task Modify(Localization.Extraction.Content content)
                {
                    var strucutre = await Process();

                    content.Text.UnionWith(strucutre.Text);
                }
            }
        }
#endif
    }
}