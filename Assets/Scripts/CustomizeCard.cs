using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

using TMPro;

using SimpleFileBrowser;

using Firebase;
using Firebase.Extensions;
using Firebase.Storage;
using Firebase.Firestore;
using Firebase.Auth;

using ZXing;
using ZXing.QrCode;

public class CustomizeCard : MonoBehaviour
{
    public string templateLink;
    public string templateName;

    public RawImage image;
    private Texture2D texture;

    public GameObject warningPanel;

    public GameObject cardDetailPage;
    public GameObject cardStylePage;

    public TMP_Text mainText;
    public TMP_Text subText;

    public TMP_InputField mainTextField;
    public TMP_InputField subTextField;
    public TMP_InputField cardNameField;
    public TMP_Text warningMessage;

    private byte[] audioBytes;
    private string streamLink;

    private bool fileExist = true;
    private bool CallingUploadAudio = false;
    private string audioID;
    private string audioName;

    private Texture2D Card;

    public GameObject renameAudioField;
    public TMP_InputField audioNameField;

    public GameObject optionPanel;

    private bool finishUpload = false;
    private bool saveAndUpload = false;

    private byte[] bytearray;

    //Firebase
    FirebaseStorage storage;
    StorageReference storageRef;
    private StorageReference tempRef;
    private StorageReference checkIfFileExist;
    FirebaseFirestore db;

    // Start is called before the first frame update
    void Start()
    {
        storage = FirebaseStorage.DefaultInstance;
        storageRef = storage.GetReferenceFromUrl("gs://cardhaus-1ed70.appspot.com");
        db = FirebaseFirestore.DefaultInstance;
        StartCoroutine(DownloadImage());
    }

    void Update()
    {
        if(!fileExist && CallingUploadAudio)
        {
            showOptionPanel();
            CallingUploadAudio = false;
        }
        if(finishUpload)
        {
            SceneManager.LoadScene("LibraryPage");
        }
    }
    IEnumerator DownloadImage()
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(templateLink);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);
        else
        {
            texture = new Texture2D(2, 2);
            texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            image.texture = texture;
        }
    }

    public void customizeClicked()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser.DisplayName == "")
        {
            warningPanel.SetActive(true);
        }
        else
        {
            cardDetailPage.SetActive(false);
            cardStylePage.SetActive(true);
        }
    }

    public void changeMainText()
    {
        mainText.text = mainTextField.text;
    }

    public void changeSubText()
    {
        subText.text = subTextField.text;
    }

    public void openFileExplorer()
    {
        // Set filters (optional)
        // It is sufficient to set the filters just once (instead of each time before showing the file browser dialog), 
        // if all the dialogs will be using the same filters
        FileBrowser.SetFilters(false, new FileBrowser.Filter("Audio Files", ".mp3"));

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
            audioBytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);

            if(audioBytes != null)
            {
                renameAudioField.SetActive(true);
            }
        }
    }

    public void finishClicked()
    { 
        if(audioBytes!=null)
        {
            checkFileExist();
        }
        else
        {
            if (cardNameField.text != "")
            {
                showOptionPanel();
            }
            else if (cardNameField.text == "")
            {
                warningMessage.text = "Card name cannot be empty";
                Debug.Log("dsad");
                Debug.Log(cardNameField.text);
            }           
            //StartCoroutine(takeScreenshot());
        }     
    }

    public void showOptionPanel()
    {
        optionPanel.SetActive(true);
    }

    private void checkFileExist()
    {
        if (audioNameField.text != "")
        {
            audioName = audioNameField.text.Replace(" ","");
            audioID = "/Audios/" + audioName + ".mp3";
            checkIfFileExist = storageRef.Child(audioID);
            //Check if file exists
            checkIfFileExist.GetDownloadUrlAsync().ContinueWithOnMainThread(task => {
                if (!task.IsFaulted && !task.IsCanceled)
                {
                    warningMessage.text = "This file already exist";
                    fileExist = false;
                }
                else
                {
                    audioName = audioNameField.text;
                    if(cardNameField.text != "")
                    {
                        CallingUploadAudio = true;
                        fileExist = false;
                        Debug.Log("This is " + cardNameField.text);
                    }
                    else if(cardNameField.text == "")
                    {
                        warningMessage.text = "Card name cannot be empty";
                        Debug.Log("dsad");
                    }
                   
                }
            });
        }
        else
        {
            warningMessage.text = "Please enter an audio name";
            fileExist = true;
        }

        fileExist = true;
    }

    public async void uploadAudio()
    {
        tempRef = storageRef.Child(audioID);
           
        var newMetadata = new MetadataChange();
        newMetadata.ContentType = "audio/mp3";

        await tempRef.PutBytesAsync(audioBytes, newMetadata).ContinueWithOnMainThread((task) =>
        {
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

        await tempRef.GetDownloadUrlAsync().ContinueWithOnMainThread(task =>
        {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                streamLink = task.Result.ToString();
                Debug.Log("Download URL: " + streamLink);
                DocumentReference docRef = db.Collection("UserAudios").Document();
                Dictionary<string, object> template = new Dictionary<string, object>
                {
                    { "audioName", audioName},
                    { "audioLink", streamLink },
                };
                docRef.SetAsync(template).ContinueWithOnMainThread(task => {
                    Debug.Log("Added document to the collection.");
                });
            }
        });

        if(saveAndUpload)
        {
            StartCoroutine(saveAndUploadFunc());
        }
        else
        {
            StartCoroutine(upload());
        }
    }

    public void yesClicked()
    {
        saveAndUpload = true;
        optionPanel.SetActive(false);
        if(audioBytes==null)
        {
            StartCoroutine(saveAndUploadFunc());
        }
        else
        {
            uploadAudio();
        }
    }

    public void noClicked()
    {
        saveAndUpload = false;
        optionPanel.SetActive(false);
        if (audioBytes == null)
        {
            StartCoroutine(upload());
        }
        else
        {
            uploadAudio();
        }
    }

    private IEnumerator upload()
    {
        yield return new WaitForEndOfFrame();

        yield return new WaitForEndOfFrame();
        float width = Screen.width / (float)4.46428571429;
        float height = Screen.height / (float)1.625;

        int xSize = (int)(Screen.width / 1.8);
        int ySize = (int)(Screen.height / 2.8);
        Card = new Texture2D(xSize, ySize, TextureFormat.RGB24, false);
        Card.ReadPixels(new Rect(width, height, xSize, ySize), 0, 0);
        Card.Apply();

        bytearray = Card.EncodeToPNG();

        uploadToDatabase();
    }

    private IEnumerator saveAndUploadFunc()
    {
        
        yield return new WaitForEndOfFrame();
        float width = Screen.width / (float)4.46428571429;
        float height = Screen.height / (float)1.625;

        int xSize = (int) (Screen.width / 1.8);
        int ySize = (int) (Screen.height / 2.8);
        Card = new Texture2D(xSize, ySize, TextureFormat.RGB24, false);
        Card.ReadPixels(new Rect(width, height , xSize, ySize), 0, 0);
        Card.Apply();

        bytearray = Card.EncodeToPNG();

        // Save the screenshot to Gallery/Photos
        NativeGallery.Permission permission = NativeGallery.SaveImageToGallery(Card, "CardHausGallery", 
            templateName + ".png", (success, path) => Debug.Log("Media save result: " + success + " " + path));
        Debug.Log("Permission result: " + permission);
        Debug.Log("saving to gallery");
        uploadToDatabase();       
    }  

    private async void uploadToDatabase()
    {
        string cardName;

        if(cardNameField.text != null)
        {
            cardName = cardNameField.text;
        }
        else
        {
            cardName = cardNameField.placeholder.GetComponent<Text>().text;
        }

        //Save to firebase storage
        string imageName = cardNameField.text.Replace(" ","");
        string imageID = "UserTemplates/" + FirebaseAuth.DefaultInstance.CurrentUser.DisplayName + "/" + imageName + ".jpeg";
        tempRef = storageRef.Child(imageID);
        var newMetadata = new MetadataChange();
        newMetadata.ContentType = "image/jpeg";

        await tempRef.PutBytesAsync(bytearray, newMetadata).ContinueWithOnMainThread((task) => {
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

        //Save to firebase firestore
        string downloadLink;

        await tempRef.GetDownloadUrlAsync().ContinueWithOnMainThread(task => {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                downloadLink = task.Result.ToString();
                Debug.Log("Download URL: " + downloadLink);
                int index = downloadLink.IndexOf("&token");
                if (index >= 0)
                    downloadLink = downloadLink.Substring(0, index);
                DocumentReference docRef = db.Collection("UserTemplates").Document();
                Dictionary<string, object> template = new Dictionary<string, object>
                {
                    {"cardName", cardName},
                    {"userID",FirebaseAuth.DefaultInstance.CurrentUser.UserId},
                    {"isVideoAR", false},
                    {"imageLink", downloadLink },
                };
                docRef.SetAsync(template).ContinueWithOnMainThread(task => {
                    Debug.Log("Added document to the collection.");
                });
            }
        });

        finishUpload = true;
    }

    /*------------------------------------------------------------------------------------------------------*/

    [Header("Add Pictures Functions")]

    public RawImage customImage;
    public GameObject customImageGameObject;
    private Texture2D customTexture;
    private byte[] customImageBytes;

    public void openCustomImageExplorer()
    {

        FileBrowser.SetFilters(false, new FileBrowser.Filter("Images", ".jpeg", "jpg", "png"));

        FileBrowser.SetExcludedExtensions(".*", ".lnk", ".tmp", ".zip", ".rar", ".exe");

        FileBrowser.AddQuickLink("Users", "C:\\Users", null);

        // Coroutine example
        StartCoroutine(ShowCustomLoadDialogCoroutine());
    }

    IEnumerator ShowCustomLoadDialogCoroutine()
    {

        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, null, null, "Load Files and Folders", "Load");

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
            customImageBytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);
            loadImage();
        }    
    }

    private void loadImage()
    {
        customTexture = new Texture2D(2,2);
        customTexture.LoadImage(customImageBytes);

        customImage.texture = customTexture;
        customImageGameObject.SetActive(true);
    }

}

