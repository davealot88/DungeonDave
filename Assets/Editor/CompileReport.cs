using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Compilation;

namespace DungeonDave.EditorTools
{
    /// <summary>
    /// Writes the result of every script compile to Logs/claude/compile.txt in the project folder,
    /// so compile errors can be read from outside the editor without anyone copying them by hand.
    /// </summary>
    [InitializeOnLoad]
    public static class CompileReport
    {
        private static readonly List<string> Lines = new List<string>();
        private static int errors;
        private static int warnings;

        private static string Folder
        {
            get { return Path.Combine(Directory.GetCurrentDirectory(), "Logs", "claude"); }
        }

        static CompileReport()
        {
            CompilationPipeline.compilationStarted += OnStarted;
            CompilationPipeline.assemblyCompilationFinished += OnAssembly;
            CompilationPipeline.compilationFinished += OnFinished;
            if (!EditorUtility.scriptCompilationFailed)
                Write("LOADED", "Scripts compiled and loaded. Unity " + UnityEngine.Application.unityVersion);
        }

        private static void OnStarted(object context)
        {
            Lines.Clear();
            errors = 0;
            warnings = 0;
        }

        private static void OnAssembly(string assemblyPath, CompilerMessage[] messages)
        {
            foreach (var message in messages)
            {
                if (message.type == CompilerMessageType.Error) errors++;
                else if (message.type == CompilerMessageType.Warning) warnings++;
                else continue;
                Lines.Add(message.type + ": " + message.message);
            }
        }

        private static void OnFinished(object context)
        {
            var body = new StringBuilder();
            body.AppendLine("errors: " + errors + ", warnings: " + warnings);
            foreach (var line in Lines) body.AppendLine(line);
            Write(errors > 0 ? "FAILED" : "COMPILED", body.ToString());
        }

        private static void Write(string status, string body)
        {
            try
            {
                Directory.CreateDirectory(Folder);
                File.WriteAllText(Path.Combine(Folder, "compile.txt"),
                    status + " " + DateTime.Now.ToString("s") + "\n" + body + "\n");
            }
            catch (Exception)
            {
                // Reporting must never break the editor.
            }
        }
    }
}
