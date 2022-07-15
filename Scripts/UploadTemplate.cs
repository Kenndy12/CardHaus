using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

using MongoDB.Driver;
using MongoDB.Bson;

using SimpleFileBrowser;

public class UploadTemplate : MonoBehaviour
{
    //MongoDB
    MongoClient client = new MongoClient("mongodb+srv://CardHaus:cardHaus321@cardhauscluster.lznis.mongodb.net/?retryWrites=true&w=majority");
    IMongoDatabase database;
    IMongoCollection<BsonDocument> collection;

    public RawImage image;

    public Toggle videoCardToggle;
    public TMP_InputField templateNameField;
    public TMP_InputField templateIDField;
   
    private byte[] imageBytes;
    
    private string templateName;
    private string templateID;
    private bool isVideoCard;

    // Start is called before the first frame update
    void Start()
    {
        database = client.GetDatabase("CardHausDatabase");
        collection = database.GetCollection<BsonDocument>("CardHausTemplate");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void openFileExplorer()
    {
        // Set filters (optional)
        // It is sufficient to set the filters just once (instead of each time before showing the file browser dialog), 
        // if all the dialogs will be using the same filters
        FileBrowser.SetFilters(false, new FileBrowser.Filter("Images", ".jpeg",".jpg",".png"));

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

    public void uploadToDatabase()
    {
        if (checkFields())
        {
            templateName = templateNameField.text;
            templateID = templateIDField.text;
            isVideoCard = videoCardToggle.isOn;

            Debug.Log(templateName);
            Debug.Log(templateID);
            Debug.Log(isVideoCard);

            var document = new BsonDocument { { "templateName", templateName }, {"templateID",templateID}, {"isTemplate",true},
                {"isVideoCard",isVideoCard }, { "templateImage", imageBytes} };
            collection.InsertOne(document);
        }
        else
        {
            Debug.Log("One of the fields is empty");
        }
    }

    private bool checkFields()
    {
        if(templateNameField.text != "")
        {
            if(templateIDField.text != "")
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
