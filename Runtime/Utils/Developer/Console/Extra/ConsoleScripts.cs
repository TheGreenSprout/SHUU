/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

using static SHUU.InnerWorkings.SHUU_PackageUtils;

namespace SHUU.Utils.Developer.Console
{
    public static class ConsoleScripts
    {
        #region Variables
        private const int MaxDepth = 8;
        private const string AutoexecName = "autoexec";


        private static readonly string AutoexecTemplate = string.Join(Environment.NewLine,
            "// autoexec: the script that runs when you type  exec  with nothing after it.",
            "// One command per line, written exactly as you'd type it in the console.",
            "// Blank lines and lines starting with // or # are skipped. Type 'explain exec' in the console for more.");



        private sealed class Frame
        {
            public string name;
            public string[] lines;

            public int next;
            public int ran;
        }


        private static readonly List<Frame> Stack = new List<Frame>();

        private static bool Driving;
        private static float? PendingWait;
        private static Coroutine Waiter;


        public static string DataFolder => GetPath("DevConsole.exec");

        public static bool IsRunning => Stack.Count > 0;
        #endregion




        #region Main
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            Stack.Clear();

            Driving = false;
            PendingWait = null;
            Waiter = null;
        }
        #endregion




        #region Logic
        public static string ResolvePath(string file)
        {
            file = file.Trim();

            if (Path.GetExtension(file).Length == 0) file += ".txt";

            if (Path.IsPathRooted(file)) return file;

            return Path.GetFullPath(Path.Combine(DataFolder, file));
        }


        public static CommandReturn Run(string file)
        {
            if (string.IsNullOrWhiteSpace(file)) file = AutoexecName;

            if (DevConsoleManager.Instance == null) return CommandReturn.Red("There's no DevConsoleManager in the scene.");

            if (Stack.Count >= MaxDepth) return CommandReturn.Red($"Scripts are running each other more than {MaxDepth} levels deep. Does one of them run itself?");

            if (Stack.Count > 0 && !Driving) return CommandReturn.Red("A script is still waiting. Let it finish, or stop it with execstop.");


            string path = ResolvePath(file);
            if (!File.Exists(path)) return CommandReturn.Red($"Script not found: {path}");

            string[] lines;

            try { lines = File.ReadAllLines(path); }
            catch (Exception e) { return CommandReturn.Red($"Couldn't read {path}: {e.Message}"); }


            Frame frame = new Frame { name = Path.GetFileName(path), lines = lines };

            Stack.Add(frame);

            if (Driving) return CommandReturn.Green($"Running {frame.name}...");


            CommandReturn result = Advance();

            if (result != null) return result;

            return WaitInBackground(frame.name);
        }

        public static CommandReturn Wait(float seconds)
        {
            if (!Driving) return CommandReturn.Red("wait only works inside a script (see: explain exec).");

            if (float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds < 0f) return CommandReturn.Red("Give a number of seconds that's 0 or more.");


            PendingWait = seconds;

            return CommandReturn.Green($"Waiting {seconds.ToString(CultureInfo.InvariantCulture)} seconds...");
        }

        public static bool Cancel()
        {
            bool wasRunning = Stack.Count > 0;

            Stack.Clear();
            PendingWait = null;

            if (Waiter != null)
            {
                if (DevConsoleManager.Instance != null) DevConsoleManager.Instance.StopCoroutine(Waiter);

                Waiter = null;
            }

            return wasRunning;
        }


        private static CommandReturn Advance()
        {
            Driving = true;

            try { return Step(); }
            catch (Exception e)
            {
                Stack.Clear();
                PendingWait = null;

                return CommandReturn.Red($"The script stopped: {e.Message}");
            }
            finally { Driving = false; }
        }

        private static CommandReturn Step()
        {
            while (Stack.Count > 0)
            {
                Frame frame = Stack[Stack.Count - 1];

                if (frame.next >= frame.lines.Length)
                {
                    Stack.RemoveAt(Stack.Count - 1);

                    CommandReturn done = CommandReturn.Green($"Ran {frame.ran} command(s) from {frame.name}.");

                    if (Stack.Count == 0) return done;

                    DevConsoleManager.PrintOnConsole(done.output, done.color);

                    continue;
                }


                string line = frame.lines[frame.next++].Trim();

                if (line.Length == 0 || line.StartsWith("//") || line.StartsWith("#")) continue;


                bool worked = DevConsoleManager.Instance.ProcessInputDirect(line, out CommandReturn output);

                if (!worked || IsError(output)) return Fail();

                frame.ran++;

                if (PendingWait != null) return null;
            }

            return CommandReturn.Yellow("The script was stopped.");
        }

        private static CommandReturn Fail()
        {
            List<string> messages = new List<string>();

            for (int i = Stack.Count - 1; i >= 0; i--)
                messages.Add($"Stopped at line {Stack[i].next} of {Stack[i].name}: {Stack[i].lines[Stack[i].next - 1].Trim()}");

            Stack.Clear();
            PendingWait = null;

            return CommandReturn.Red(messages.ToArray());
        }


        private static CommandReturn WaitInBackground(string name)
        {
            Waiter = DevConsoleManager.Instance.StartCoroutine(Continue(TakeWait()));

            if (Waiter == null)
            {
                Cancel();

                return CommandReturn.Red($"{name} has a wait in it, but the console couldn't keep it going (is the DevConsoleManager object active?).");
            }

            return CommandReturn.Yellow($"{name} is waiting, and will carry on by itself (execstop cancels it).");
        }

        private static float TakeWait()
        {
            float seconds = PendingWait ?? 0f;

            PendingWait = null;

            return seconds;
        }

        private static IEnumerator Continue(float seconds)
        {
            while (true)
            {
                if (seconds > 0f) yield return new WaitForSecondsRealtime(seconds);
                else yield return null;

                if (Stack.Count == 0) yield break;


                CommandReturn result = Advance();

                if (result != null)
                {
                    Waiter = null;

                    DevConsoleManager.PrintOnConsole(result.output, result.color);

                    yield break;
                }

                seconds = TakeWait();
            }
        }

        public static void EnsureAutoexec()
        {
            try
            {
                string path = ResolvePath(AutoexecName);

                if (File.Exists(path)) return;


                Directory.CreateDirectory(Path.GetDirectoryName(path));

                File.WriteAllText(path, AutoexecTemplate);
            }
            catch (Exception e) { Debug.LogWarning($"Couldn't create the console's {AutoexecName}.txt: {e.Message}"); }
        }


        private static bool IsError(CommandReturn output) => output != null && output.color == Color.red;
        #endregion
    }
}
