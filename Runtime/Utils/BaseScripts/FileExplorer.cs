using Ookii.Dialogs;
using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

// PARTIAL CREDIT TO: CodeMonkey

// File explorer only works in windows
#if PLATFORM_STANDALONE_WIN

namespace SHUU.Utils.BaseScripts
{

#region File browser interactivity

#region XML doc
/// <summary>
/// Handles file explorer interaction (credits to CodeMonkey).
/// </summary>
#endregion
public class FileExplorer{
    #region Setup
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();


    // Constructor
    public FileExplorer() { }
    #endregion



    #region Interaction functions

    #region XML doc
    /// <summary>
    /// File explorer interaction to get a single file's directory address.
    /// </summary>
    /// <param name="browserProperties">Properties of the file dialogue.</param>
    /// <param name="newFile">If true, the file is being created, if not it is being loaded.</param>
    #endregion
    public string GetFileAddress(BrowserProperties browserProperties, bool newFile){
        VistaFileDialog fileDialog;
        
        if (newFile)
        {
            fileDialog = new VistaSaveFileDialog();
        }
        else
        {
            fileDialog = new VistaOpenFileDialog();
        }
        


        #region Variable setup
        // Disable multi-selection if possible
        if (!newFile){
            ((VistaOpenFileDialog) fileDialog).Multiselect = false;
        }
        fileDialog.Title = browserProperties.title == null ? "Select a File" : browserProperties.title; // Title, default is "Select a File"
        fileDialog.InitialDirectory = browserProperties.initialDir == null ? @"C:\" : browserProperties.initialDir; // Initial directory, default is C:\
        fileDialog.Filter = browserProperties.filter == null ? "All files (*.*)|*.*" : browserProperties.filter; // Filter, default is all files.
        fileDialog.FilterIndex = browserProperties.filterIndex + 1; // Filter index
        fileDialog.RestoreDirectory = browserProperties.restoreDirectory; // Use last directory?
        #endregion



        // Get directory from user
        if (fileDialog.ShowDialog(new WindowWrapper(GetActiveWindow())) == DialogResult.OK)
        {
            return fileDialog.FileName;
        }
        else
        {
            return null;
        }
    }



    #region XML doc
    /// <summary>
    /// File explorer interaction to retrieve a single file.
    /// </summary>
    /// <param name="browserProperties">Properties of the file dialogue.</param>
    /// <param name="filepath">User picked path (Callback).</param>
    #endregion
    public void GetFileFromBrowser(BrowserProperties browserProperties, Action<string> filepath){
        var fileDialog = new VistaOpenFileDialog();


        #region Variable setup
        fileDialog.Multiselect = false; // Can select multiple files
        fileDialog.Title = browserProperties.title == null ? "Select a File" : browserProperties.title; // Title, default is "Select a File"
        fileDialog.InitialDirectory = browserProperties.initialDir == null ? @"C:\" : browserProperties.initialDir; // Initial directory, default is C:\
        fileDialog.Filter = browserProperties.filter == null ? "All files (*.*)|*.*" : browserProperties.filter; // Filter, default is all files.
        fileDialog.FilterIndex = browserProperties.filterIndex + 1; // Filter index
        fileDialog.RestoreDirectory = browserProperties.restoreDirectory; // Use last directory?
        #endregion



        // Get directory from user
        if (fileDialog.ShowDialog(new WindowWrapper(GetActiveWindow())) == DialogResult.OK)
        {
            filepath(fileDialog.FileName);
        }
    }

    #endregion
}

#endregion



#region Utils

public class BrowserProperties{
    public string title; // Title of the dialog.
    public string initialDir; // Where dialogue will be opened initially.
    public string filter; // File extension filter.
    public int filterIndex; // Index of filter, if there is multiple filter. / Default is 0 
    public bool restoreDirectory = true; // Restore to last return directory.




    public BrowserProperties() { }

    public BrowserProperties(string title) {
        this.title = title;
    }
}


public class WindowWrapper : IWin32Window{
    public WindowWrapper(IntPtr handle){
        _hwnd = handle;
    }

    public IntPtr Handle{
        get { return _hwnd; }
    }

    private IntPtr _hwnd;
}

#endregion

}

#endif
