using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace SHUU.Utils.UI
{
    #region XML doc
    /// <summary>
    /// Supported tags (= and : interchangeble):
    ///     <pause=N> --> Pause for N tenths of a second
    ///     <shake=N>...</shake> --> Shake the enclosed characters with intensity N
    ///     <event=id> --> Fires an event right before the next character is revealed
    ///     <nopause>...</nopause> --> Disables the automatic punctuation delays for the enclosed characters
    /// </summary>
    #endregion
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TMP_Text), typeof(TypewriterTextInput), typeof(TypewriterTextEvents))]
    public class TypewriterText : MonoBehaviour
    {
        #region Variables

        #region Inspector
        [Header("Speed")]
        [Min(1f)] public float charactersPerSecond = 20f;

        public float punctuationDelay = 0.5f; // ? . : ; !
        public float semiPunctuationDelay = 0.25f; // , -


        [Header("Fast forward / skip")]
        [Min(1)] public int fastForwardSpeedup = 5;

        [Tooltip("When the text is skipped, still fire the <event=...> tags that hadn't been reached yet.")]
        [SerializeField] private bool fireEventsOnSkip = true;


        [Header("Misc")]
        [Range(0.1f, 0.5f)] public float completeDelay = 0.25f;
        [SerializeField] private bool useUnscaledTime = true;

        [SerializeField] private UnityEvent<AudioClip> playSound;


        [Header("Shake")]
        [SerializeField] private float shakeScale = 0.1f;
        [SerializeField] private Vector2 shakeRange = new(0.2f, 0.5f);
        #endregion



        #region Public
        public Func<AudioClip> talkingSounds;
        private AudioClip talkingSound => talkingSounds?.Invoke();


        public bool isTyping { get; private set; }
        public bool isFastForwarding { get; private set; }


        public event Action completeTextRevealed;
        public event Action<char> characterRevealed;
        public event Action<string> eventTagReached;
        #endregion



        #region Internal

        #region Data structs
        private struct Shake
        {
            public int start, end;
            public float intensity;
        }

        private struct CharRange
        {
            public int start, end;
        }

        private struct TextTag
        {
            public int position;
            public float pauseSeconds;
            public string eventId;
            public bool IsEvent => eventId != null;
        }
        #endregion


        private static readonly Regex TagRegex = new(@"<pause[=:](\d+)>|<shake[=:](\d+)>|<event[=:]([^>]+)>|<nopause>|</nopause>|</shake>", RegexOptions.Compiled);


        private TMP_Text text;
        private TypewriterTextInput input;
        private TypewriterTextEvents events;
        private Coroutine routine;

        private readonly List<TextTag> tags = new();
        private int nextTag;
        private readonly List<Shake> shakes = new();
        private float[] shakeIntensity = Array.Empty<float>();

        private readonly List<CharRange> noPauses = new();
        private bool[] suppressPause = Array.Empty<bool>();

        private int token;


        private float deltaTime => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        #endregion

        #endregion




        #region Main
        private void Awake()
        {
            text = GetComponent<TMP_Text>();
            input = GetComponent<TypewriterTextInput>();
            TryGetComponent(out events);
        }


        private void OnEnable()
        {
            input.fastForwardRequested += FastForward;
            input.skipRequested += Skip;
        }

        private void OnDisable()
        {
            input.fastForwardRequested -= FastForward;
            input.skipRequested -= Skip;
            Stop();
        }


        private void LateUpdate()
        {
            if (shakeIntensity.Length == 0 || !text.IsActive()) return;

            text.ForceMeshUpdate();
            TMP_TextInfo info = text.textInfo;

            int count = Mathf.Min(info.characterCount, shakeIntensity.Length, text.maxVisibleCharacters);

            for (int i = 0; i < count; i++)
            {
                float k = shakeIntensity[i];
                if (k == 0f) continue;

                TMP_CharacterInfo ch = info.characterInfo[i];
                if (!ch.isVisible) continue;

                Vector3[] verts = info.meshInfo[ch.materialReferenceIndex].vertices;
                var offset = new Vector3(
                    UnityEngine.Random.Range(-shakeRange.x, shakeRange.x),
                    UnityEngine.Random.Range(-shakeRange.y, shakeRange.y),
                    0f) * k;

                for (int j = 0; j < 4; j++)
                    verts[ch.vertexIndex + j] += offset;
            }

            text.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }
        #endregion



        #region Logic
        
        #region Public API
        public void Play(string rawText)
        {
            Stop();
            int _token = token;

            text.maxVisibleCharacters = 0;
            text.text = ParseTags(rawText);
            text.ForceMeshUpdate();

            TMP_TextInfo info = text.textInfo;
            ResolveToCharacterIndices(info);
            BuildShakeLookup(info.characterCount);
            BuildSuppressLookup(info.characterCount);
            nextTag = 0;

            Coroutine _routine = StartCoroutine(TypeRoutine(token));

            if (_token == token) routine = _routine;
        }

        public void Stop()
        {
            token++;
            StopRoutine();
            isTyping = false;
            isFastForwarding = false;
        }


        public void FastForward()
        {
            if (isTyping) isFastForwarding = true;
        }

        public void Skip()
        {
            if (!isTyping) return;

            int _token = token;
            StopRoutine();
            text.maxVisibleCharacters = text.textInfo.characterCount;

            if (fireEventsOnSkip)
            {
                while (nextTag < tags.Count)
                {
                    TextTag tag = tags[nextTag++];
                    if (!tag.IsEvent) continue;

                    RaiseEvent(tag.eventId);
                    if (_token != token) return;
                }
            }

            Finish();
        }


        public void Clear()
        {
            Stop();
            text.text = "";
            text.maxVisibleCharacters = 0;
            tags.Clear();
            nextTag = 0;
            shakes.Clear();
            shakeIntensity = Array.Empty<float>();
            noPauses.Clear();
            suppressPause = Array.Empty<bool>();
        }
        #endregion

        

        #region Typing
        private IEnumerator TypeRoutine(int _token)
        {
            isTyping = true;
            isFastForwarding = false;

            TMP_TextInfo info = text.textInfo;
            int total = info.characterCount;

            float wait = 0f;

            for (int i = 0; i <= total; i++)
            {
                if (i == total) wait = 0f;

                while (wait > 0f)
                {
                    wait -= deltaTime;
                    yield return null;
                }

                while (nextTag < tags.Count && tags[nextTag].position <= i)
                {
                    TextTag tag = tags[nextTag++];

                    if (tag.IsEvent)
                    {
                        RaiseEvent(tag.eventId);
                        if (_token != token) yield break;
                    }
                    else
                    {
                        wait += tag.pauseSeconds;
                        while (wait > 0f)
                        {
                            wait -= deltaTime;
                            yield return null;
                        }
                    }
                }

                if (i == total) break;

                char c = info.characterInfo[i].character;
                text.maxVisibleCharacters = i + 1;

                PlayTypeSound(c);
                characterRevealed?.Invoke(c);
                if (_token != token) yield break;

                wait += DelayAfter(c, i);
            }

            for (float t = 0f; t < completeDelay; t += deltaTime)
                yield return null;

            routine = null;
            Finish();
        }
        

        private float DelayAfter(char c, int index)
        {
            if (isFastForwarding)
                return 1f / (charactersPerSecond * fastForwardSpeedup);

            bool suppressed = index < suppressPause.Length && suppressPause[index];

            switch (c)
            {
                case '?': case '.': case ':': case ';': case '!':
                    return suppressed ? 1f / charactersPerSecond : punctuationDelay;
                case ',': case '-':
                    return suppressed ? 1f / charactersPerSecond : semiPunctuationDelay;
                default:
                    return 1f / charactersPerSecond;
            }
        }

        private void RaiseEvent(string id)
        {
            eventTagReached?.Invoke(id);
            if (events != null) events.CallEvent(id);
        }


        private void PlayTypeSound(char c)
        {
            if (talkingSounds == null || char.IsWhiteSpace(c)) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            playSound?.Invoke(talkingSound);
        }


        private void Finish()
        {
            isTyping = false;
            isFastForwarding = false;
            completeTextRevealed?.Invoke();
        }

        private void StopRoutine()
        {
            if (routine == null) return;
            StopCoroutine(routine);
            routine = null;
        }
        #endregion



        #region Tag parsing
        private void ResolveToCharacterIndices(TMP_TextInfo info)
        {
            for (int i = 0; i < tags.Count; i++)
            {
                TextTag t = tags[i];
                t.position = SourceToCharacterIndex(info, t.position);
                tags[i] = t;
            }

            for (int i = 0; i < shakes.Count; i++)
            {
                Shake s = shakes[i];
                s.start = SourceToCharacterIndex(info, s.start);
                s.end = SourceToCharacterIndex(info, s.end);
                shakes[i] = s;
            }

            for (int i = 0; i < noPauses.Count; i++)
            {
                CharRange r = noPauses[i];
                r.start = SourceToCharacterIndex(info, r.start);
                r.end = SourceToCharacterIndex(info, r.end);
                noPauses[i] = r;
            }
        }

        private static int SourceToCharacterIndex(TMP_TextInfo info, int sourceIndex)
        {
            int lo = 0, hi = info.characterCount;
            while (lo < hi)
            {
                int mid = (lo + hi) >> 1;
                if (info.characterInfo[mid].index < sourceIndex) lo = mid + 1;
                else hi = mid;
            }
            return lo;
        }


        private string ParseTags(string input)
        {
            tags.Clear();
            shakes.Clear();
            noPauses.Clear();

            var sb = new StringBuilder(input.Length);
            var openShakes = new Stack<(int start, float intensity)>();
            int noPauseStart = -1, noPauseDepth = 0;
            int last = 0;

            foreach (Match m in TagRegex.Matches(input))
            {
                sb.Append(input, last, m.Index - last);
                last = m.Index + m.Length;

                int pos = sb.Length;

                if (m.Groups[1].Success)
                {
                    float seconds = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) / 10f;
                    tags.Add(new TextTag { position = pos, pauseSeconds = seconds });
                }
                else if (m.Groups[2].Success)
                {
                    float intensity = int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
                    openShakes.Push((pos, intensity));
                }
                else if (m.Groups[3].Success)
                {
                    string id = m.Groups[3].Value.Trim();
                    if (id.Length > 0)
                        tags.Add(new TextTag { position = pos, eventId = id });
                }
                else if (m.Value == "<nopause>")
                {
                    if (noPauseDepth++ == 0) noPauseStart = pos;
                }
                else if (m.Value == "</nopause>")
                {
                    if (noPauseDepth > 0 && --noPauseDepth == 0)
                        noPauses.Add(new CharRange { start = noPauseStart, end = pos });
                }
                else if (openShakes.Count > 0)
                {
                    var (start, intensity) = openShakes.Pop();
                    shakes.Add(new Shake { start = start, end = pos, intensity = intensity });
                }
            }

            sb.Append(input, last, input.Length - last);

            while (openShakes.Count > 0)
            {
                var (start, intensity) = openShakes.Pop();
                shakes.Add(new Shake { start = start, end = sb.Length, intensity = intensity });
            }

            if (noPauseDepth > 0)
                noPauses.Add(new CharRange { start = noPauseStart, end = sb.Length });

            return sb.ToString();
        }
        

        private void BuildShakeLookup(int characterCount)
        {
            shakeIntensity = shakes.Count == 0 ? Array.Empty<float>() : new float[characterCount];

            foreach (Shake s in shakes)
            {
                int end = Mathf.Min(s.end, characterCount);
                for (int i = Mathf.Max(0, s.start); i < end; i++)
                    shakeIntensity[i] = Mathf.Max(shakeIntensity[i], s.intensity * shakeScale);
            }
        }

        private void BuildSuppressLookup(int characterCount)
        {
            suppressPause = noPauses.Count == 0 ? Array.Empty<bool>() : new bool[characterCount];

            foreach (CharRange r in noPauses)
            {
                int end = Mathf.Min(r.end, characterCount);
                for (int i = Mathf.Max(0, r.start); i < end; i++)
                    suppressPause[i] = true;
            }
        }
        #endregion

        #endregion
    }
}