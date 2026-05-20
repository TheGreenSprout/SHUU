using System;
using SFB;

namespace SHUU.Utils.BaseScripts
{
    public static class FileExplorer
    {
        #region Interaction functions
        public static string GetFileAddress(BrowserProperties browserProperties, bool newFile)
        {
            if (browserProperties == null) browserProperties = new BrowserProperties();


            var extensions = BuildFilters(browserProperties);

            if (newFile)
            {
                string path = StandaloneFileBrowser.SaveFilePanel(
                    browserProperties.title ?? "Save File",
                    browserProperties.initialDir ?? "",
                    browserProperties.defaultName ?? "NewFile",
                    browserProperties.defaultExtension ?? ""
                );

                return string.IsNullOrEmpty(path) ? null : path;
            }
            else
            {
                var paths = StandaloneFileBrowser.OpenFilePanel(
                    browserProperties.title ?? "Select File",
                    browserProperties.initialDir ?? "",
                    extensions,
                    false
                );

                return (paths != null && paths.Length > 0) ? paths[0] : null;
            }
        }


        public static void GetFileFromBrowser(BrowserProperties browserProperties, Action<string> filepath)
        {
            if (browserProperties == null) browserProperties = new BrowserProperties();


            var extensions = BuildFilters(browserProperties);

            var paths = StandaloneFileBrowser.OpenFilePanel(
                browserProperties.title ?? "Select File",
                browserProperties.initialDir ?? "",
                extensions,
                false
            );

            if (paths != null && paths.Length > 0) filepath?.Invoke(paths[0]);
        }
        #endregion



        #region Helpers
        private static ExtensionFilter[] BuildFilters(BrowserProperties props)
        {
            if (props.filters == null || props.filters.Length == 0) return new[] { new ExtensionFilter("All Files", "*") };

            return props.filters;
        }
        #endregion
    }




    #region Utils
    [Serializable]
    public class BrowserProperties
    {
        public string title;
        public string initialDir;


        public ExtensionFilter[] filters;


        public string defaultName;
        public string defaultExtension;

        public bool restoreDirectory = true;



        public BrowserProperties() { }

        public BrowserProperties(string title) { this.title = title; }
    }
    #endregion
}
