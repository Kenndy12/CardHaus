using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Storage;


public class ExportCard : MonoBehaviour
{
    public string templateLink;
    public string templateName;
    public string customizeSceneName;

    public RawImage image;
    private Texture2D texture;

    FirebaseFirestore db;
    FirebaseStorage storage;
    StorageReference storageRef;
    private StorageReference tempRef;

    private bool isCoroutineExecuting = false;

    public GameObject saveConfirmPanel;
    void Awake()
    {
        db = FirebaseFirestore.DefaultInstance;      
        storage = FirebaseStorage.DefaultInstance;
        storageRef = storage.GetReferenceFromUrl("gs://cardhaus-1ed70.appspot.com");
        StartCoroutine(DownloadImage());
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
        SceneManager.LoadScene(customizeSceneName);
    }

    public void callExport()
    {
        StartCoroutine(export());
    }

    public void callSave()
    {
        StartCoroutine(saveToGallery());
    }

    private IEnumerator export()
    {
        yield return null;
        string filepath = Path.Combine(Application.temporaryCachePath, templateName + ".png");
        File.WriteAllBytes(filepath, texture.EncodeToPNG());
        
        Debug.Log(Application.temporaryCachePath);
        new NativeShare().AddFile(filepath).Share();
    }

    private IEnumerator saveToGallery()
    {
        yield return new WaitForEndOfFrame();

        // Save the screenshot to Gallery/Photos
        NativeGallery.Permission permission = NativeGallery.SaveImageToGallery(texture, "CardHausGallery", templateName +  ".jpeg", (success, path) => Debug.Log("Media save result: " + success + " " + path));

        Debug.Log("Permission result: " + permission);

        StartCoroutine(saveConfirmation(2));
    }


    IEnumerator saveConfirmation(float time)
    {
        if (isCoroutineExecuting)
            yield break;

        isCoroutineExecuting = true;
        saveConfirmPanel.SetActive(true);

        yield return new WaitForSeconds(time);
        saveConfirmPanel.SetActive(false);
        // Code to execute after the delay

        isCoroutineExecuting = false;
    }

    void onDestroy()
    {
        Destroy(texture);
    }
}
