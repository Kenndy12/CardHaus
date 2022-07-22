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
using Firebase.Auth;

using ZXing;
using ZXing.QrCode;

public class CustomizeCard : MonoBehaviour
{
    public string templateLink;

    public RawImage image;
    private Texture2D texture;

    public GameObject cardDetailPage;
    public GameObject cardCustomizePage;

    public TMP_Text mainText;
    public TMP_Text subText;

    public TMP_InputField mainTextField;
    public TMP_InputField subTextField;
    public TMP_Text warningMessage;

    private byte[] audioBytes;
    private string streamLink;

    private bool fileExist = true;
    private bool CallingUploadAudio = false;
    private string audioID;
    /*
    private Texture2D QrCodeTexture;
    public RawImage QRCode;
    public GameObject QRCodeActiveOrNot;*/
    private Texture2D Card;

    public GameObject renameAudioField;
    public TMP_InputField audioNameField;

    //Firebase
    FirebaseStorage storage;
    StorageReference storageRef;
    private StorageReference tempRef;
    private StorageReference checkIfFileExist;

    // Start is called before the first frame update
    void Start()
    {
        storage = FirebaseStorage.DefaultInstance;
        storageRef = storage.GetReferenceFromUrl("gs://cardhaus-1ed70.appspot.com");
        StartCoroutine(DownloadImage());
    }

    void Update()
    {
        if(!fileExist && CallingUploadAudio)
        {
            uploadAudio();
            CallingUploadAudio = false;
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
        cardDetailPage.SetActive(false);
        cardCustomizePage.SetActive(true);
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
            StartCoroutine(takeScreenshot());
        }

       
    }

    private void checkFileExist()
    {
        if (audioNameField.text != "")
        {
            audioID = "/Audios/" + audioNameField.text + ".mp3";
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
                    CallingUploadAudio = true;
                    fileExist = false;
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
            }
        });

        StartCoroutine(takeScreenshot());

        SceneManager.LoadScene("LibraryPage");
    }


    /*private void generateQR(string url)
    {
        string text = "THISISAUDIO + " + url;
        QrCodeTexture = new Texture2D(256, 256);
        var color32 = Encode(text, QrCodeTexture.width, QrCodeTexture.height);
        QrCodeTexture.SetPixels32(color32);
        QrCodeTexture.Apply();
    }

    private static Color32[] Encode(string textForEncoding,int width, int height)
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            }
        };
        return writer.Write(textForEncoding);
    }*/



    private IEnumerator takeScreenshot()
    {
        yield return new WaitForEndOfFrame();
        float width = Screen.width / (float)7.71428571429;
        float height = Screen.height / (float)2.26682408501;

        int xSize = (int) (Screen.width / 1.35);
        int ySize = (int) (Screen.height / 2.4);
        Card = new Texture2D(xSize, ySize, TextureFormat.RGB24, false);
        Card.ReadPixels(new Rect(width, height , xSize, ySize), 0, 0);
        Card.Apply();

        byte[] bytearray = Card.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.dataPath + "/hehe.png", bytearray);
        Debug.Log(Application.dataPath + "/hehe.png");

        // Save the screenshot to Gallery/Photos
        NativeGallery.Permission permission = NativeGallery.SaveImageToGallery(Card, "CardHausGallery", 
            "SavedCard.png", (success, path) => Debug.Log("Media save result: " + success + " " + path));


        Debug.Log("Permission result: " + permission);
    }

   
}
