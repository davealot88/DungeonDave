using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace DungeonDave.EditorTools
{
    /// <summary>
    /// Lets the project manager session drive a few editor actions by dropping a one-line text
    /// file at Logs/claude/request.txt. Supported requests:
    ///   refresh            re-import changed files
    ///   run-tests          run the edit-mode tests, results go to Logs/claude/tests.txt
    ///   menu Some/Menu     run a menu item by its path
    /// The outcome of each request is written to Logs/claude/response.txt.
    /// </summary>
    [InitializeOnLoad]
    public static class RequestBridge
    {
        private static double nextCheck;

        internal static string Folder
        {
            get { return Path.Combine(Directory.GetCurrentDirectory(), "Logs", "claude"); }
        }

        static RequestBridge()
        {
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (EditorApplication.timeSinceStartup < nextCheck) return;
            nextCheck = EditorApplication.timeSinceStartup + 1.0;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;

            string path = Path.Combine(Folder, "request.txt");
            if (!File.Exists(path)) return;

            string request;
            try
            {
                request = File.ReadAllText(path).Trim();
                File.Delete(path);
            }
            catch (Exception)
            {
                return; // file still being written; try again next tick
            }
            Handle(request);
        }

        private static void Handle(string request)
        {
            try
            {
                if (request == "refresh")
                {
                    AssetDatabase.Refresh();
                    Respond(request, "ok");
                }
                else if (request == "run-tests")
                {
                    RuleTestRun.Start();
                    Respond(request, "started");
                }
                else if (request.StartsWith("menu "))
                {
                    bool ran = EditorApplication.ExecuteMenuItem(request.Substring(5).Trim());
                    Respond(request, ran ? "ok" : "menu item not found");
                }
                else
                {
                    Respond(request, "unknown request");
                }
            }
            catch (Exception e)
            {
                Respond(request, "error: " + e);
            }
        }

        internal static void WriteFile(string name, string text)
        {
            try
            {
                Directory.CreateDirectory(Folder);
                File.WriteAllText(Path.Combine(Folder, name), text);
            }
            catch (Exception)
            {
                // Reporting must never break the editor.
            }
        }

        private static void Respond(string request, string outcome)
        {
            WriteFile("response.txt", DateTime.Now.ToString("s") + "\n" + request + "\n" + outcome + "\n");
        }
    }

    /// <summary>Runs the edit-mode tests and writes a plain-text summary.</summary>
    public sealed class RuleTestRun : ICallbacks
    {
        private static TestRunnerApi api;
        private readonly List<string> failures = new List<string>();

        [MenuItem("DungeonDave/Run Rule Tests")]
        public static void Start()
        {
            if (api == null) api = ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new RuleTestRun());
            api.Execute(new ExecutionSettings(new Filter { testMode = TestMode.EditMode }));
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
            RequestBridge.WriteFile("tests.txt", "RUNNING " + DateTime.Now.ToString("s") + "\n");
        }

        public void TestStarted(ITestAdaptor test)
        {
        }

        public void TestFinished(ITestResultAdaptor result)
        {
            if (!result.HasChildren && result.TestStatus == TestStatus.Failed)
                failures.Add(result.Test.FullName + "\n    " + result.Message);
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            var text = new StringBuilder();
            text.AppendLine((result.FailCount == 0 ? "PASSED " : "FAILED ") + DateTime.Now.ToString("s"));
            text.AppendLine("passed: " + result.PassCount + ", failed: " + result.FailCount +
                            ", skipped: " + result.SkipCount);
            foreach (var failure in failures) text.AppendLine(failure);
            RequestBridge.WriteFile("tests.txt", text.ToString());
            Debug.Log("[DungeonDave] Rule tests: " + result.PassCount + " passed, " + result.FailCount + " failed.");
            api.UnregisterCallbacks(this);
        }
    }
}
