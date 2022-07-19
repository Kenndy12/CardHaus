using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

using SimpleFileBrowser;

using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Storage;

public class UploadTemplate : MonoBehaviour
{
    public RawImage image;

    public Toggle videoCardToggle;
    public TMP_InputField templateNameField;
    public TMP_InputField templateIDField;

    private byte[] imageBytes;

    private string templateName;
    private string templateID;
    private bool isVideoCard;
    private string downloadLink;

    FirebaseFirestore db;

    FirebaseStorage storage;
    StorageReference storageRef;
    private StorageReference tempRef;

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
        FileBrowser.SetFilters(false, new FileBrowser.Filter("Images", ".jpeg", ".jpg", ".png"));

        // Set excluded file extensions (optional) (by default, .lnk and .tmp extensions are excluded)
        // Note that when you use this function, .lnk and .tmp extensions will no longer be
        // excluded unless you explicitly add them as parameters to the function
        FileBrowser.SetExcludedExtensions(".*", ".lnk", ".tmp", ".zip", ".rar", ".exe");

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
            imageBytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);
            Debug.Log(imageBytes);
        }
        previewImage();
    }

    private void previewImage()
    {
        Debug.Log(imageBytes);
        Texture2D test = new Texture2D(2, 2);
        test.LoadImage(imageBytes);
        image.texture = test;
    }

    public async void uploadToDatabase()
    {
        if (checkFields())
        {
            templateName = templateNameField.text;
            templateID = templateIDField.text;
            isVideoCard = videoCardToggle.isOn;


            Debug.Log(templateName);
            Debug.Log(templateID);
            Debug.Log(isVideoCard);

            string templateNameWithoutSpace = templateName.Replace(" ", "");
            string templateImage = "CardHausTemplate/" + templateNameWithoutSpace;
            tempRef = storageRef.Child(templateImage);
            var newMetadata = new MetadataChange();
            newMetadata.ContentType = "image/jpeg";

            await tempRef.PutBytesAsync(imageBytes, newMetadata).ContinueWithOnMainThread((task) => {
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
                }
            });

            
            int index = downloadLink.IndexOf("&token");
            if (index >= 0)
                downloadLink = downloadLink.Substring(0, index);

            DocumentReference docRef = db.Collection("CardHausTemplate").Document(templateName);
            Dictionary<string, object> template = new Dictionary<string, object>
            {
                { "templateName", templateName },
                { "templateID", templateID },
                { "isTemplate", true},
                { "isVideoCard", isVideoCard },
                { "imageURL", downloadLink},
            };
            await docRef.SetAsync(template).ContinueWithOnMainThread(task => {
                Debug.Log("Added document to the collection.");
            });

        }
        else
        {
            Debug.Log("One of the fields is empty");
        }
    }

    private bool checkFields()
    {
        if (templateNameField.text != "")
        {
            if (templateIDField.text != "")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }
}
