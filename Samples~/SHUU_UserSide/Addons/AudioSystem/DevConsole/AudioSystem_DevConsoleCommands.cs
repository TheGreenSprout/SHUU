using System;
using System.Collections.Generic;
using UnityEngine;

using SHUU.UserSide.Addons.AudioSystem.ScriptableObjects;
using SHUU.Utils.Developer.Console;
using SHUU.Utils.Helpers;

namespace SHUU.UserSide.Addons.AudioSystem
{
    public class AudioSystem_DevConsoleCommands : MonoBehaviour
    {
        #region Variables
        private static SHUU_AudioInstance LastPlayed;
        private static CommandReturn NoInstance() => CommandReturn.Red("Nothing has been played yet (use playaudio first).");


        private static string ChannelPathOf(OptionalParameter<string> channelPath) => channelPath.TryGetValue(out string value) ? value : "";
        #endregion




        #region Commands

        #region Playback
        [DevConsoleCommand("playaudio", "Plays a clip by name from a channel's AudioLookup (path is relative to the root, e.g. 'SFX')", "Debug")]
        public static CommandReturn PlayAudio(string clipName, OptionalParameter<string> channelPath)
        {
            string path;
            if (!channelPath.TryGetValue(out path)) path = ChannelPathOf(channelPath);


            if (string.IsNullOrWhiteSpace(clipName)) return CommandReturn.Red("Give a clip name to play.");
            if (string.IsNullOrWhiteSpace(path)) return CommandReturn.Red("Give a channel path to play it on (e.g. 'SFX').");


            SHUU_AudioInstance instance;

            try { instance = SHUU_Audio.PlayAudio(clipName, path, null, new SHUU_AudioInstance.Options()); }
            catch (Exception e) { return CommandReturn.Red($"Couldn't play '{clipName}': {e.Message}"); }

            if (instance == null) return CommandReturn.Red($"Couldn't play '{clipName}' (check the console for details).");


            LastPlayed = instance;

            return CommandReturn.Green($"Playing '{clipName}' on '{channelPath}'.");
        }


        [DevConsoleCommand("audiostop", "Stops the sound started by the last playaudio call", "Debug")]
        public static CommandReturn StopAudio()
        {
            if (LastPlayed == null) return NoInstance();

            LastPlayed.Stop();

            return CommandReturn.Green("Stopped.");
        }

        [DevConsoleCommand("audiopause", "Pauses the sound started by the last playaudio call", "Debug")]
        public static CommandReturn PauseAudio()
        {
            if (LastPlayed == null) return NoInstance();

            LastPlayed.Pause();

            return CommandReturn.Green("Paused.");
        }

        [DevConsoleCommand("audioresume", "Resumes the sound paused with audiopause", "Debug")]
        public static CommandReturn ResumeAudio()
        {
            if (LastPlayed == null) return NoInstance();

            LastPlayed.UnPause();

            return CommandReturn.Green("Resumed.");
        }
        #endregion



        #region Tweaking
        [DevConsoleCommand("audiovolume", "Sets the volume (0-1) of the sound started by the last playaudio call", "Debug")]
        public static CommandReturn AudioVolume(float volume)
        {
            if (LastPlayed == null) return NoInstance();

            LastPlayed.volume = volume;

            return CommandReturn.Green($"Volume set to {volume}.");
        }

        [DevConsoleCommand("audiopitch", "Sets the pitch of the sound started by the last playaudio call", "Debug")]
        public static CommandReturn AudioPitch(float pitch)
        {
            if (LastPlayed == null) return NoInstance();

            LastPlayed.pitch = pitch;

            return CommandReturn.Green($"Pitch set to {pitch}.");
        }

        [DevConsoleCommand("audiomute", "Mutes or unmutes the sound started by the last playaudio call", "Debug")]
        public static CommandReturn AudioMute(OptionalParameter<bool> toggle)
        {
            if (LastPlayed == null) return NoInstance();

            if (toggle.TryGetValue(out var t)) LastPlayed.mute = t;
            else LastPlayed.mute = !LastPlayed.mute;

            return CommandReturn.Green(LastPlayed.mute ? "Muted." : "Unmuted.");
        }
        #endregion



        #region Diagnostics
        [DevConsoleCommand("audiotree", "Shows the tree of audio channels, with each one's object pool stats", "Information")]
        public static CommandReturn AudioTree()
        {
            IAudioChannel root;

            try { root = SHUU_Audio.GetChannel(""); }
            catch (Exception e) { return CommandReturn.Red($"Couldn't read the channel tree: {e.Message}"); }

            if (root == null) return CommandReturn.Red("There's no AudioChannelGroup assigned to SHUU_Audio (or SHUU_Audio isn't in the scene).");


            List<string> lines = new List<string>();
            AppendChannel(root, "", true, true, lines);

            return new CommandReturn(lines.ToArray());
        }

        #region Tree Drawing
        private static void AppendChannel(IAudioChannel channel, string prefix, bool isLast, bool isRoot, List<string> lines)
        {
            string connector = isRoot ? "" : (isLast ? "└── " : "├── ");

            lines.Add(prefix + connector + channel.GetID() + PoolSummary(channel));


            string childPrefix = isRoot ? "" : prefix + (isLast ? "    " : "│   ");

            List<IAudioChannel> children = new List<IAudioChannel>(channel.GetChildren());

            for (int i = 0; i < children.Count; i++)
                AppendChannel(children[i], childPrefix, i == children.Count - 1, false, lines);
        }

        private static string PoolSummary(IAudioChannel channel)
        {
            try
            {
                SHUU_ObjectPool<SHUU_AudioInstance> pool = channel.GetObjectPool();

                return pool == null ? "" : $"  [{pool.name}: {pool.activesCount}/{pool.totalCount}]";
            }
            catch (Exception e) { return $"  [pool error: {e.Message}]"; }
        }
        #endregion


        [DevConsoleCommand("audioclips", "Lists every clip name in a channel's AudioLookup.", "Information")]
        public static CommandReturn AudioClips(OptionalParameter<string> channelPath)
        {
            string path = ChannelPathOf(channelPath);


            IAudioChannel channel;

            try { channel = SHUU_Audio.GetChannel(path); }
            catch (Exception e) { return CommandReturn.Red($"Couldn't find the channel '{path}': {e.Message}"); }

            if (channel == null) return CommandReturn.Red($"No channel found at '{path}' (check the channel path).");


            AudioLookup lookup = channel.GetAudioLookup();
            if (lookup == null) return CommandReturn.Red($"'{path}' has no AudioLookup assigned (and doesn't inherit one).");


            List<string> ids = new List<string>(lookup.GetAllIDs());
            if (ids.Count == 0) return CommandReturn.Yellow($"'{path}' has an AudioLookup, but it has no entries.");


            return new CommandReturn(ids.ToArray());
        }


        [DevConsoleCommand("audiopool", "Shows a channel's object pool stats: active, pooled and total instances.", "Information")]
        public static CommandReturn AudioPool(OptionalParameter<string> channelPath)
        {
            string path = ChannelPathOf(channelPath);


            SHUU_ObjectPool<SHUU_AudioInstance> pool;

            try { pool = SHUU_Audio.GetPool(path); }
            catch (Exception e) { return CommandReturn.Red($"Couldn't find the channel '{path}': {e.Message}"); }

            if (pool == null) return CommandReturn.Red($"No object pool for the channel '{path}' (check the channel path).");


            return new CommandReturn($"'{pool.name}': {pool.activesCount} active, {pool.poolCount} pooled, {pool.totalCount} total.");
        }
        #endregion
    
        #endregion
    }
}
