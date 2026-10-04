/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;

namespace SHUU._Editor.Utils.SceneSwitcher
{
    internal readonly struct SceneEntry
    {
        public readonly string guid;
        public readonly string path;
        public readonly string name;
        public readonly string folder;

        public readonly string nameKey;
        public readonly string pathKey;


        public SceneEntry(string guid, string path)
        {
            this.guid = guid;
            this.path = path;

            string relative = path.StartsWith("Assets/", StringComparison.Ordinal) ? path.Substring("Assets/".Length) : path;
            string directory = Path.GetDirectoryName(relative);

            name = Path.GetFileNameWithoutExtension(relative);
            folder = string.IsNullOrEmpty(directory) ? "" : directory.Replace('\\', '/');

            nameKey = name.ToLowerInvariant();
            pathKey = Path.ChangeExtension(relative, null).Replace('\\', '/').ToLowerInvariant();
        }
    }




    internal readonly struct SceneRow
    {
        public readonly int entry;
        public readonly string title;

        public bool IsHeader => entry < 0;


        private SceneRow(int entry, string title)
        {
            this.entry = entry;
            this.title = title;
        }

        public static SceneRow Header(string title) => new SceneRow(-1, title);
        public static SceneRow Scene(int entry) => new SceneRow(entry, null);
    }




    internal sealed class SceneSwitcherModel
    {
        #region Variables
        public const string FavouritesTitle = "Favourites";
        public const string ScenesTitle = "Scenes";


        private readonly List<SceneEntry> entries;

        public IReadOnlyList<SceneEntry> Entries => entries;


        private struct Match
        {
            public int entry;
            public int rank;
            public bool favourite;
        }
        #endregion




        #region Main
        public SceneSwitcherModel(IEnumerable<SceneEntry> scenes)
        {
            entries = new List<SceneEntry>(scenes);

            entries.Sort((a, b) => string.CompareOrdinal(a.path, b.path));
        }


        public List<SceneRow> Build(string search, Func<string, bool> isFavourite)
        {
            string[] tokens = Tokenize(search);

            return tokens.Length == 0 ? BuildBrowse(isFavourite) : BuildSearch(tokens, isFavourite);
        }
        #endregion




        #region Logic
        private List<SceneRow> BuildBrowse(Func<string, bool> isFavourite)
        {
            List<int> favourites = new List<int>();
            List<int> others = new List<int>();

            for (int i = 0; i < entries.Count; i++)
                (isFavourite(entries[i].guid) ? favourites : others).Add(i);

            favourites.Sort((a, b) =>
            {
                int byName = string.Compare(entries[a].name, entries[b].name, StringComparison.OrdinalIgnoreCase);

                return byName != 0 ? byName : a.CompareTo(b);
            });


            List<SceneRow> rows = new List<SceneRow>(entries.Count + 2);

            if (favourites.Count > 0)
            {
                rows.Add(SceneRow.Header(FavouritesTitle));
                foreach (int entry in favourites) rows.Add(SceneRow.Scene(entry));

                if (others.Count > 0) rows.Add(SceneRow.Header(ScenesTitle));
            }

            foreach (int entry in others) rows.Add(SceneRow.Scene(entry));

            return rows;
        }


        private List<SceneRow> BuildSearch(string[] tokens, Func<string, bool> isFavourite)
        {
            List<Match> matches = new List<Match>();

            for (int i = 0; i < entries.Count; i++)
            {
                SceneEntry scene = entries[i];

                if (!ContainsAll(scene.pathKey, tokens)) continue;

                int rank = scene.nameKey.StartsWith(tokens[0], StringComparison.Ordinal) ? 0
                        : ContainsAll(scene.nameKey, tokens) ? 1
                        : 2;

                matches.Add(new Match { entry = i, rank = rank, favourite = isFavourite(scene.guid) });
            }

            matches.Sort((a, b) =>
            {
                if (a.rank != b.rank) return a.rank.CompareTo(b.rank);
                if (a.favourite != b.favourite) return a.favourite ? -1 : 1;

                return a.entry.CompareTo(b.entry);
            });


            List<SceneRow> rows = new List<SceneRow>(matches.Count);
            foreach (Match match in matches) rows.Add(SceneRow.Scene(match.entry));

            return rows;
        }


        private static string[] Tokenize(string search)
        {
            if (string.IsNullOrWhiteSpace(search)) return Array.Empty<string>();

            return search.ToLowerInvariant().Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        }

        private static bool ContainsAll(string text, string[] tokens)
        {
            foreach (string token in tokens)
                if (text.IndexOf(token, StringComparison.Ordinal) < 0) return false;

            return true;
        }
        #endregion
    }




     internal sealed class SceneFavouritesStore
    {
        #region Variables
        private readonly string filePath;

        private HashSet<string> guids;

        public string SaveError { get; private set; }
        #endregion




        #region Main
        public SceneFavouritesStore(string filePath) => this.filePath = filePath;


        public bool Contains(string guid)
        {
            Load();

            return guids.Contains(guid);
        }

        public bool Toggle(string guid)
        {
            Load();

            bool nowFavourite = guids.Add(guid);
            if (!nowFavourite) guids.Remove(guid);

            Save();

            return nowFavourite;
        }
        #endregion




        #region Logic
        private void Load()
        {
            if (guids != null) return;

            guids = new HashSet<string>(StringComparer.Ordinal);

            try
            {
                if (!File.Exists(filePath)) return;

                foreach (string line in File.ReadAllLines(filePath))
                {
                    string guid = line.Trim();

                    if (guid.Length > 0) guids.Add(guid);
                }
            }
            catch (Exception) { /* Unreadable file: start with none. */ }
        }

        private void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

                List<string> sorted = new List<string>(guids);
                sorted.Sort(string.CompareOrdinal);

                File.WriteAllLines(filePath, sorted);

                SaveError = null;
            }
            catch (Exception exception) { SaveError = exception.Message; }
        }
        #endregion
    }




    internal static class SceneSwitcherStar
    {
        private const int Samples = 4;
        private const float InnerRadiusRatio = 0.45f;
        private const float Cos36Degrees = 0.809017f;


        public static byte[] Render(int size, bool filled)
        {
            float outer = size * 0.47f;
            float inner = outer * InnerRadiusRatio;

            float centerX = size * 0.5f;
            float centerY = size * 0.5f - outer * (1f - Cos36Degrees) * 0.5f;

            float[] xs = new float[10];
            float[] ys = new float[10];

            for (int i = 0; i < 10; i++)
            {
                double angle = (90d + i * 36d) * Math.PI / 180d;
                float radius = i % 2 == 0 ? outer : inner;

                xs[i] = centerX + radius * (float)Math.Cos(angle);
                ys[i] = centerY + radius * (float)Math.Sin(angle);
            }

            float lineWidth = size * 0.08f;

            byte[] pixels = new byte[size * size * 4];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int covered = 0;

                    for (int sy = 0; sy < Samples; sy++)
                    {
                        for (int sx = 0; sx < Samples; sx++)
                        {
                            float px = x + (sx + 0.5f) / Samples;
                            float py = y + (sy + 0.5f) / Samples;

                            bool hit = Inside(xs, ys, px, py) && (filled || DistanceToEdge(xs, ys, px, py) <= lineWidth);

                            if (hit) covered++;
                        }
                    }

                    int index = (y * size + x) * 4;

                    pixels[index] = 255;
                    pixels[index + 1] = 255;
                    pixels[index + 2] = 255;
                    pixels[index + 3] = (byte)(covered * 255 / (Samples * Samples));
                }
            }

            return pixels;
        }


        private static bool Inside(float[] xs, float[] ys, float px, float py)
        {
            bool inside = false;

            for (int i = 0, j = xs.Length - 1; i < xs.Length; j = i++)
            {
                bool crosses = (ys[i] > py) != (ys[j] > py);

                if (crosses && px < (xs[j] - xs[i]) * (py - ys[i]) / (ys[j] - ys[i]) + xs[i]) inside = !inside;
            }

            return inside;
        }

        private static float DistanceToEdge(float[] xs, float[] ys, float px, float py)
        {
            float best = float.MaxValue;

            for (int i = 0, j = xs.Length - 1; i < xs.Length; j = i++)
            {
                float dx = xs[i] - xs[j];
                float dy = ys[i] - ys[j];

                float t = ((px - xs[j]) * dx + (py - ys[j]) * dy) / (dx * dx + dy * dy);
                t = t < 0f ? 0f : (t > 1f ? 1f : t);

                float cx = xs[j] + t * dx - px;
                float cy = ys[j] + t * dy - py;

                float distance = (float)Math.Sqrt(cx * cx + cy * cy);

                if (distance < best) best = distance;
            }

            return best;
        }
    }
}
#endif
