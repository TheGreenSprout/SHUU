#if UNITY_EDITOR
using System.Globalization;

namespace SHUU._Editor.Utils.PlayModeSaver
{
    internal static class PropertyCodec
    {
        #region Variables
        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
        #endregion




        #region Logic
        public static string FromDouble(double value) => value.ToString("G17", Culture);
        public static double ToDouble(string text) => double.Parse(text, NumberStyles.Float, Culture);

        public static string FromLong(long value) => value.ToString(Culture);
        public static long ToLong(string text) => long.Parse(text, NumberStyles.Integer, Culture);


        public static string JoinFloats(params float[] values)
        {
            string[] parts = new string[values.Length];

            for (int i = 0; i < values.Length; i++)
                parts[i] = values[i].ToString("G9", Culture);

            return string.Join(",", parts);
        }

        public static float[] SplitFloats(string text)
        {
            if (string.IsNullOrEmpty(text)) return new float[0];

            string[] parts = text.Split(',');
            float[] values = new float[parts.Length];

            for (int i = 0; i < parts.Length; i++)
                values[i] = float.Parse(parts[i], NumberStyles.Float, Culture);

            return values;
        }


        public static string JoinInts(params int[] values)
        {
            string[] parts = new string[values.Length];

            for (int i = 0; i < values.Length; i++)
                parts[i] = values[i].ToString(Culture);

            return string.Join(",", parts);
        }

        public static int[] SplitInts(string text)
        {
            if (string.IsNullOrEmpty(text)) return new int[0];

            string[] parts = text.Split(',');
            int[] values = new int[parts.Length];

            for (int i = 0; i < parts.Length; i++)
                values[i] = int.Parse(parts[i], NumberStyles.Integer, Culture);

            return values;
        }
        #endregion
    }
}
#endif
