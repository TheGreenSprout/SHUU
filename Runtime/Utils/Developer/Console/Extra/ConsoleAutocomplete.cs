using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SHUU.Utils.Developer.Console
{
    public static class ConsoleAutocomplete
    {
        #region Logic
        public static Result Complete(string text, int caret, IEnumerable<string> commandNames, Func<string, ParameterInfo[]> getParameters, IEnumerable<string> variableNames)
        {
            text ??= "";
            caret = Math.Clamp(caret, 0, text.Length);

            Result result = new Result { text = text, caret = caret };


            int wordStart = ConsoleTokenizer.FindWordStart(text, caret, out int wordsBefore, out bool insideQuotes);

            string word = text.Substring(wordStart, caret - wordStart);


            int fragmentStart;
            IEnumerable<string> values;
            bool endsWord;

            int dollar = word.LastIndexOf('$');

            if (dollar >= 0 && IsName(word, dollar + 1))
            {
                fragmentStart = wordStart + dollar + 1;
                values = variableNames ?? Enumerable.Empty<string>();
                endsWord = dollar == 0 && !insideQuotes;
            }
            else if (word.IndexOf('"') >= 0) return result;
            else if (wordsBefore == 0)
            {
                fragmentStart = wordStart;
                values = commandNames ?? Enumerable.Empty<string>();
                endsWord = true;
            }
            else
            {
                ConsoleTokenizer.TryTokenize(text.Substring(0, wordStart), out string[] previousWords, out _);

                fragmentStart = wordStart;
                values = ValuesOf(getParameters?.Invoke(previousWords[0].ToLowerInvariant()), wordsBefore - 1);
                endsWord = true;
            }

            string prefix = text.Substring(fragmentStart, caret - fragmentStart);

            List<string> matches = values
                .Where(value => value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (matches.Count == 0) return result;


            bool unique = matches.Count == 1;
            string fill = unique ? matches[0] : CommonStart(matches);

            if (!unique && fill.Length <= prefix.Length)
            {
                result.options = matches;

                return result;
            }


            string after = text.Substring(caret);

            bool addSpace = unique && endsWord && after.Length == 0;

            string start = text.Substring(0, fragmentStart) + fill;

            result.text = start + (addSpace ? " " : "") + after;
            result.caret = start.Length + (addSpace ? 1 : 0);

            return result;
        }


        private static IEnumerable<string> ValuesOf(ParameterInfo[] parameters, int position)
        {
            if (parameters == null || parameters.Length == 0) return Enumerable.Empty<string>();

            ParameterInfo parameter = position < parameters.Length ? parameters[position] : parameters[parameters.Length - 1];

            bool isParams = Attribute.IsDefined(parameter, typeof(ParamArrayAttribute));

            if (position >= parameters.Length && !isParams) return Enumerable.Empty<string>();


            Type type = parameter.ParameterType;

            if (isParams) type = type.GetElementType();

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(OptionalParameter<>)) type = type.GetGenericArguments()[0];


            if (type.IsEnum) return Enum.GetNames(type).Select(name => name.ToLowerInvariant());

            if (type == typeof(bool)) return new[] { "true", "false" };

            return Enumerable.Empty<string>();
        }

        private static string CommonStart(List<string> matches)
        {
            string first = matches[0];
            int length = first.Length;

            foreach (string match in matches)
            {
                int same = 0;

                while (same < length && same < match.Length && char.ToLowerInvariant(first[same]) == char.ToLowerInvariant(match[same])) same++;

                length = same;
            }

            return first.Substring(0, length);
        }

        private static bool IsName(string text, int from)
        {
            for (int i = from; i < text.Length; i++)
                if (!char.IsLetterOrDigit(text[i]) && text[i] != '_') return false;

            return true;
        }
        #endregion
    
    
    
    
        #region Helper class
        public sealed class Result
        {
            public string text;
            public int caret;

            public List<string> options = new List<string>();
        }
        #endregion
    }
}
