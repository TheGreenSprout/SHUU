/*
⚠️‼️ AI ASSISTED CODE

This code was written with the assistance of AI.
*/



using System.IO;
using UnityEditor;
using UnityEngine;

namespace SHUU.Editor
{
    public static class ForUserInstaller
    {
        private const string SourceFolder = "ForUser";              // folder to move
        private const string TargetFolder = "Assets/SHUU";     // destination in user's project
        private const string InstalledMarker = "Assets/SHUU/.foruser_installed"; // marker to prevent rerun

        [MenuItem("Tools/SHUU/Install ForUser Assets", true)]
        private static bool InstallForUserAssets_Validate()
        {
            // Disable the menu item if already installed
            return !File.Exists(InstalledMarker);
        }

        [MenuItem("Tools/SHUU/Install ForUser Assets")]
        public static void InstallForUserAssets()
        {
            // Only install if not already installed
            if (File.Exists(InstalledMarker))
            {
                Debug.Log("[SHUU] ForUser assets already installed.");
                return;
            }

            // -------------------------------
            // SOURCE PATH
            // -------------------------------
            
            // 1️⃣ Testing inside a normal project (current setup)
            //string sourcePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/SHUU", SourceFolder);

            // 2️⃣ Uncomment this for a proper UPM package
            string packagePath = Path.Combine(Directory.GetCurrentDirectory(), "Packages", "com.sproutinggames.sprouts.huu");
            string sourcePath = Path.Combine(packagePath, SourceFolder);

            if (!Directory.Exists(sourcePath))
            {
                Debug.LogWarning($"[SHUU] Source folder does not exist: {sourcePath}");
                return;
            }

            // -------------------------------
            // TARGET PATH
            // -------------------------------
            string targetPath = Path.Combine(Directory.GetCurrentDirectory(), TargetFolder, SourceFolder);

            // Ensure parent folder exists
            string targetParent = Path.GetDirectoryName(targetPath);
            if (!Directory.Exists(targetParent))
                Directory.CreateDirectory(targetParent);

            // Move entire folder
            if (Directory.Exists(targetPath))
            {
                Debug.LogWarning($"[SHUU] Target folder already exists: {targetPath}. Move skipped.");
            }
            else
            {
                Directory.Move(sourcePath, targetPath);
                Debug.Log("[SHUU] ForUser folder moved successfully into Assets.");
            }

            // Create marker to prevent rerun
            Directory.CreateDirectory(Path.GetDirectoryName(InstalledMarker));
            File.WriteAllText(InstalledMarker, "installed");

            AssetDatabase.Refresh();
        }

        private class TempMarker : ScriptableObject { }
    }
}
