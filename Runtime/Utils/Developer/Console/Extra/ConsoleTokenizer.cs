/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SHUU.Utils.Developer.Console
{
    public static class ConsoleTokenizer
    {
        #region Logic

        #region Splitting
        public static bool TryTokenize(string line, out string[] tokens, out string error)
        {
            Scanned scanned = Scan(line ?? "", (line ?? "").Length);

            tokens = scanned.tokens.ToArray();
            error = scanned.inQuotes ? "A quote (\") was opened but never closed." : null;

            return error == null;
        }

        public static int FindWordStart(string text, int caret, out int wordsBefore, out bool insideQuotes)
        {
            text ??= "";
            caret = Math.Clamp(caret, 0, text.Length);

            Scanned scanned = Scan(text, caret);

            insideQuotes = scanned.inQuotes;

            if (!scanned.open)
            {
                wordsBefore = scanned.tokens.Count;

                return caret;
            }

            wordsBefore = scanned.tokens.Count - 1;

            return scanned.starts[scanned.starts.Count - 1];
        }


        private static Scanned Scan(string text, int length)
        {
            Scanned scanned = new Scanned();

            StringBuilder current = new StringBuilder();
            bool started = false;
            int start = 0;

            int i = 0;

            while (i < length)
            {
                char c = text[i];

                if (c == '\\')
                {
                    int run = 0;
                    int runStart = i;

                    while (i < length && text[i] == '\\')
                    {
                        run++;
                        i++;
                    }

                    if (!started)
                    {
                        started = true;
                        start = runStart;
                    }

                    if (i < length && text[i] == '"')
                    {
                        current.Append('\\', run / 2);

                        if (run % 2 == 1)
                        {
                            current.Append('"');
                            i++;
                        }
                    }
                    else current.Append('\\', run);

                    continue;
                }

                if (c == '"')
                {
                    if (!started)
                    {
                        started = true;
                        start = i;
                    }

                    scanned.inQuotes = !scanned.inQuotes;
                    i++;

                    continue;
                }

                if (c == ' ' && !scanned.inQuotes)
                {
                    if (started)
                    {
                        scanned.tokens.Add(current.ToString());
                        scanned.starts.Add(start);

                        current.Clear();
                        started = false;
                    }

                    i++;

                    continue;
                }

                if (!started)
                {
                    started = true;
                    start = i;
                }

                current.Append(c);
                i++;
            }

            if (started)
            {
                scanned.tokens.Add(current.ToString());
                scanned.starts.Add(start);

                scanned.open = true;
            }

            return scanned;
        }
        #endregion



        #region Joining
        public static string Join(IEnumerable<string> words) => string.Join(" ", words.Select(QuoteWord));

        public static string QuoteWord(string word)
        {
            word ??= "";

            if (word.Length > 0 && word.IndexOf(' ') < 0 && word.IndexOf('"') < 0) return word;

            return "\"" + EscapeForQuotes(word, true) + "\"";
        }

        public static string EscapeForQuotes(string text, bool beforeClosingQuote)
        {
            StringBuilder result = new StringBuilder(text.Length + 2);
            int backslashes = 0;

            foreach (char c in text)
            {
                if (c == '\\')
                {
                    backslashes++;
                    continue;
                }

                if (c == '"')
                {
                    result.Append('\\', backslashes * 2 + 1);
                    result.Append('"');
                }
                else
                {
                    result.Append('\\', backslashes);
                    result.Append(c);
                }

                backslashes = 0;
            }

            result.Append('\\', beforeClosingQuote ? backslashes * 2 : backslashes);

            return result.ToString();
        }
        #endregion
    
        #endregion




        #region Helper class
        private sealed class Scanned
        {
            public readonly List<string> tokens = new List<string>();
            public readonly List<int> starts = new List<int>();

            public bool open;
            public bool inQuotes;
        }
        #endregion
    }
}
