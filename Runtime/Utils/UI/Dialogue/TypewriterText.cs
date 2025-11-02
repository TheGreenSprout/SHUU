using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;
using SHUU.Utils.Helpers;
using SHUU.UserSide;

namespace SHUU.Utils.UI
{

    public struct PauseCommand
    {
        public int indexInText;
        public float duration;
    }

    [RequireComponent(typeof(TMP_Text))]
    public class TypewriterText : MonoBehaviour
    {
        private readonly char[] InterpunctuationChars = { '?', '.', ':', ';', '!' };
        private readonly char[] SemiInterpunctuationChars = { ',', '-' };

        public TMP_Text _textBox;

        private int _currentVisibleCharacterIndex;
        private Coroutine _typewriterCoroutine;
        public bool _readyForNewText = true;

        private WaitForSeconds _simpleDelay;
        private WaitForSeconds _interpunctuationDelay;
        private WaitForSeconds _semi_interpunctuationDelay;

        [Header("Typewriter Settings")]
        [SerializeField] private float charactersPerSecond = 20;
        [SerializeField] private float interpunctuationDelay = 0.5f;
        [SerializeField] private float semi_interpunctuationDelay = 0.5f;

        public bool CurrentlySkipping { get; private set; }
        private WaitForSeconds _skipDelay;

        [Header("Skip options")]
        [SerializeField] private bool quickSkip;
        [SerializeField][Min(1)] private int skipSpeedup = 5;

        private WaitForSeconds _textboxFullEventDelay;
        [SerializeField][Range(0.1f, 0.5f)] private float sendDoneDelay = 0.25f;

        public event Action CompleteTextRevealed;
        public event Action<char> CharacterRevealed;

        public TalkingSounds currentTextSounds = null;

        public void SetTextSounds(TalkingSounds sounds) => currentTextSounds = sounds;

        private struct CustomTag
        {
            public enum TagType { Pause, ShakeStart, ShakeEnd }
            public TagType Type;
            public int Position;
            public float Value;
        }
        private List<CustomTag> _customTags = new();
        private List<(int start, int end, float intensity)> _activeShakes = new();

        private void Awake()
        {
            _readyForNewText = true;
            _textBox = GetComponent<TMP_Text>();

            _simpleDelay = new WaitForSeconds(1 / charactersPerSecond);
            _interpunctuationDelay = new WaitForSeconds(interpunctuationDelay);
            _semi_interpunctuationDelay = new WaitForSeconds(semi_interpunctuationDelay);
            _skipDelay = new WaitForSeconds(1 / (charactersPerSecond * skipSpeedup));
            _textboxFullEventDelay = new WaitForSeconds(sendDoneDelay);


            //CustomInputManager.AddSkipPressedCallback(Skip);
            CustomInputManager.AddFastForwardPressedCallback(QuickWriting);
        }
        
        private void OnDestroy()
        {
            //CustomInputManager.RemoveSkipPressedCallback(Skip);
            CustomInputManager.RemoveFastForwardPressedCallback(QuickWriting);
        }

        private void OnEnable() => TMPro_EventManager.TEXT_CHANGED_EVENT.Add(PrepareForNewText);
        private void OnDisable() => TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(PrepareForNewText);

        public void ClearText()
        {
            if (_readyForNewText)
            {
                _textBox.text = "";
                _textBox.maxVisibleCharacters = 0;
                _activeShakes.Clear();
            }
        }

        private void LateUpdate()
        {
            if (_textBox == null || _activeShakes.Count == 0 || !_textBox.IsActive()) return;

            _textBox.ForceMeshUpdate();
            TMP_TextInfo textInfo = _textBox.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                if (!textInfo.characterInfo[i].isVisible) continue;

                float intensity = GetShakeIntensity(i);
                if (intensity == 0) continue;

                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                int matIndex = charInfo.materialReferenceIndex;
                int vertIndex = charInfo.vertexIndex;
                Vector3[] verts = textInfo.meshInfo[matIndex].vertices;

                Vector3 shakeOffset = new Vector3(
                    UnityEngine.Random.Range(-0.2f, 0.2f),
                    UnityEngine.Random.Range(-0.5f, 0.5f),
                    0
                ) * intensity;

                for (int j = 0; j < 4; j++)
                    verts[vertIndex + j] += shakeOffset;
            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                _textBox.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
        }

        private float GetShakeIntensity(int charIndex)
        {
            foreach (var (start, end, intensity) in _activeShakes)
            {
                if (charIndex >= start && charIndex < end)
                    return intensity * 0.1f;
            }
            return 0f;
        }

        public void StartTypewriterEffect()
        {
            if (!_readyForNewText) return;
            _textBox.ForceMeshUpdate();
            _textBox.maxVisibleCharacters = 0;
            PrepareForNewText(_textBox);
        }

        private void PrepareForNewText(Object obj)
        {
            if (obj != _textBox || !_readyForNewText || _textBox.maxVisibleCharacters >= _textBox.textInfo.characterCount)
                return;

            CurrentlySkipping = false;
            _readyForNewText = false;

            if (_typewriterCoroutine != null)
                StopCoroutine(_typewriterCoroutine);

            _textBox.text = StripAndParseTags(_textBox.text);
            _textBox.maxVisibleCharacters = 0;
            _currentVisibleCharacterIndex = 0;

            _activeShakes.Clear();
            var shakeStarts = _customTags.Where(t => t.Type == CustomTag.TagType.ShakeStart).ToList();
            foreach (var startTag in shakeStarts)
            {
                var endTag = _customTags
                    .Where(t => t.Type == CustomTag.TagType.ShakeEnd && t.Position > startTag.Position)
                    .OrderBy(t => t.Position)
                    .FirstOrDefault();

                if (endTag.Position > 0)
                    _activeShakes.Add((startTag.Position, endTag.Position, startTag.Value));
            }

            _typewriterCoroutine = StartCoroutine(Typewriter());
        }

        private string StripAndParseTags(string input)
        {
            _customTags.Clear();
            Stack<(int Index, float Intensity)> shakeStack = new();

            input = Regex.Replace(input, "<pause=(\\d+)>", match =>
            {
                int index = match.Index - CountRemovedChars(input, match.Index);
                float duration = int.Parse(match.Groups[1].Value) / 10f;
                _customTags.Add(new CustomTag { Type = CustomTag.TagType.Pause, Position = index, Value = duration });
                return "";
            });

            input = Regex.Replace(input, "<shake=(\\d+)>", match =>
            {
                int index = match.Index - CountRemovedChars(input, match.Index);
                float intensity = float.Parse(match.Groups[1].Value);
                shakeStack.Push((index, intensity));
                return "";
            });

            input = Regex.Replace(input, "</shake>", match =>
            {
                int endIndex = match.Index - CountRemovedChars(input, match.Index);
                if (shakeStack.Count > 0)
                {
                    var (startIndex, intensity) = shakeStack.Pop();
                    _customTags.Add(new CustomTag { Type = CustomTag.TagType.ShakeStart, Position = startIndex, Value = intensity });
                    _customTags.Add(new CustomTag { Type = CustomTag.TagType.ShakeEnd, Position = endIndex });
                }
                return "";
            });

            return input;
        }

        private int CountRemovedChars(string input, int upToIndex)
        {
            int count = 0;
            count += Regex.Matches(input.Substring(0, upToIndex), "<pause:(\\d+)>|<shake:(\\d+)>|<shakeEnd>").Cast<Match>().Sum(m => m.Length);
            return count;
        }

        private IEnumerator Typewriter()
        {
            TMP_TextInfo textInfo = _textBox.textInfo;

            while (_currentVisibleCharacterIndex < textInfo.characterCount + 1)
            {
                var lastCharacterIndex = textInfo.characterCount - 1;

                var tag = _customTags.FirstOrDefault(t => t.Type == CustomTag.TagType.Pause && t.Position == _currentVisibleCharacterIndex);
                if (tag.Type == CustomTag.TagType.Pause)
                    yield return new WaitForSeconds(tag.Value);

                if (_currentVisibleCharacterIndex >= lastCharacterIndex)
                {
                    _textBox.maxVisibleCharacters++;
                    yield return _textboxFullEventDelay;
                    CompleteTextRevealed?.Invoke();
                    _readyForNewText = true;
                    yield break;
                }

                if (currentTextSounds != null)
                    AudioManager.PlayRandomAudioAt(Camera.main.transform, currentTextSounds.sounds, new AudioManager.AudioOptions { volume = currentTextSounds.volume });

                char character = textInfo.characterInfo[_currentVisibleCharacterIndex].character;
                _textBox.maxVisibleCharacters++;

                if (!CurrentlySkipping)
                {
                    if (InterpunctuationChars.Contains(character))
                        yield return _interpunctuationDelay;
                    else if (SemiInterpunctuationChars.Contains(character))
                        yield return _semi_interpunctuationDelay;
                    else
                        yield return _simpleDelay;
                }
                else
                {
                    yield return _skipDelay;
                }

                CharacterRevealed?.Invoke(character);
                _currentVisibleCharacterIndex++;
            }
        }

        private void QuickWriting()
        {
            if (CurrentlySkipping) return;
            CurrentlySkipping = true;

            if (!quickSkip)
            {
                StartCoroutine(QuickWritingSpeedupReset());
                return;
            }

            StopCoroutine(_typewriterCoroutine);
            _textBox.maxVisibleCharacters = _textBox.textInfo.characterCount;
            _readyForNewText = true;
            CompleteTextRevealed?.Invoke();
        }

        private IEnumerator QuickWritingSpeedupReset()
        {
            yield return new WaitUntil(() => _textBox.maxVisibleCharacters == _textBox.textInfo.characterCount - 1);
            CurrentlySkipping = false;
        }

        public void StopTypewriter()
        {
            if (_typewriterCoroutine != null)
            {
                StopCoroutine(_typewriterCoroutine);
                _typewriterCoroutine = null;
            }

            // Don't reveal more characters — just freeze where it is.
            CurrentlySkipping = false;

            _textBox.text = "";
            _textBox.maxVisibleCharacters = 0;
            _activeShakes.Clear();

            _readyForNewText = true;
        }
    }

}