using System;
using System.Collections;
using System.Collections.Generic;


using UnityEditor;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;

using System.IO;

using Firebase;
using Firebase.Extensions;
using Firebase.Storage;
using Firebase.Auth;
using Firebase.Firestore;


using SimpleFileBrowser;


public class UploadVideoScript : MonoBehaviour
{
    //Firebase
    FirebaseStorage storage;
    StorageReference storageRef;
    private StorageReference tempRef;
    FirebaseFirestore db;

    public TMP_Text resultText;
    public static string downloadLink;

    byte[] videoBytes;

    public FirebaseUser user;

    // Start is called before the first frame update
    void Start()
    {
        storage = FirebaseStorage.DefaultInstance;
        storageRef = storage.GetReferenceFromUrl("gs://cardhaus-1ed70.appspot.com");

        db = FirebaseFirestore.DefaultInstance;
    }

    public void openFileExplorer()
    {
        // Set filters (optional)
        // It is sufficient to set the filters just once (instead of each time before showing the file browser dialog), 
        // if all the dialogs will be using the same filters
        FileBrowser.SetFilters(false, new FileBrowser.Filter("Videos", ".mp4"));

        // Set excluded file extensions (optional) (by default, .lnk and .tmp extensions are excluded)
        // Note that when you use this function, .lnk and .tmp extensions will no longer be
        // excluded unless you explicitly add them as parameters to the function
        FileBrowser.SetExcludedExtensions(".*",".lnk", ".tmp", ".zip", ".rar", ".exe");

        // Add a new quick link to the browser (optional) (returns true if quick link is added successfully)
        // It is sufficient to add a quick link just once
        // Name: Users
        // Path: C:\Users
        // Icon: default (folder icon)
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);

        // Coroutine example
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: both, Allow multiple selection: true
        // Initial path: default (Documents), Initial filename: empty
        // Title: "Load File", Submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, null, null, "Load Files and Folders", "Load");

        // Dialog is closed
        // Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
        {
            // Print paths of the selected files (FileBrowser.Result) (null, if FileBrowser.Success is false)
            for (int i = 0; i < FileBrowser.Result.Length; i++)
            {
                Debug.Log(FileBrowser.Result[i]);
            }

            // Read the bytes of the first file via FileBrowserHelpers
            // Contrary to File.ReadAllBytes, this function works on Android 10+, as well
            videoBytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);
            resultText.text = "Success";
        }
    }

    public async void  uploadFile()
    {
        var todayDate = DateTime.Now;
        string strToday = todayDate.ToString("MM/dd/yyyy h:mm tt");
        strToday = strToday.Replace("/", "-");
        string videoID = FirebaseAuth.DefaultInstance.CurrentUser.DisplayName + "/" + strToday + ".mp4";
        tempRef = storageRef.Child(videoID);
        var newMetadata = new MetadataChange();
        newMetadata.ContentType = "video/mp4";

        await tempRef.PutBytesAsync(videoBytes, newMetadata).ContinueWithOnMainThread((task) => {
            if (task.IsFaulted || task.IsCanceled)
            {   
                Debug.Log(task.Result);
                Debug.Log(task.Exception.ToString());
            }
            else
            {
                Debug.Log(task.Result);
                Debug.Log("File Uploaded Successfully!");
            }
        });

        await tempRef.GetDownloadUrlAsync().ContinueWithOnMainThread(task => {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                downloadLink = task.Result.ToString();
                Debug.Log("Download URL: " + downloadLink);
                DocumentReference docRef = db.Collection("UserVideos").Document();
                Dictionary<string, object> template = new Dictionary<string, object>
                {
                    { "videoLink", downloadLink },
                };
                docRef.SetAsync(template).ContinueWithOnMainThread(task => {
                Debug.Log("Added document to the collection.");
                });
            }
        });

        resultText.text = "";
        SceneManager.LoadScene("PreviewUploadedVideoPage");
    }

    public string getLink()
    {
        return downloadLink;
    }

}
