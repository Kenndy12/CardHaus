using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

using TMPro;

using Firebase.Firestore;
using Firebase.Extensions;

public class ChooseAR : MonoBehaviour
{
    public GameObject footerBackground;
    public GameObject newFooter;

    public GameObject audioPanel;
    public GameObject thisPanel;

    public GameObject codePanel;

    public GameObject markerlessImageObject;
    public RawImage markerlessImage;
    private Texture2D markerlessTexture;

    public TMP_InputField codeField;
    public TMP_Text messageText;

    private string imageCode;
    private string imageLink;

    private bool finishGettingImageURL = false;
    FirebaseFirestore db;

    // Start is called before the first frame update
    void Start()
    {
        //Firebase
        db = FirebaseFirestore.DefaultInstance;
    }

    // Update is called once per frame
    void Update()
    {
        if(finishGettingImageURL)
        {
            finishGettingImageURL = false;
            StartCoroutine(DownloadImage(imageLink));
        }
    }

    //If option marker based is chosen
    public void markerBasedClicked()
    {
        thisPanel.SetActive(false);
        audioPanel.SetActive(true);
    }

    //If option markerless based is chosen
    public void markerlessBasedClicked()
    {

        thisPanel.SetActive(false);
        codePanel.SetActive(true);
    }

    //If finish is clicked
    public void finishClicked()
    {
        imageCode = codeField.text;

        DocumentReference docRef = db.Collection("UserTemplates").Document(imageCode);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                Dictionary<string, object> doc = snapshot.ToDictionary();
                imageLink = (string)doc["imageLink"];
                finishGettingImageURL = true;
                Debug.Log(imageLink);
            }
            else
            {
                DocumentReference cardHausRef = db.Collection("CardHausTemplate").Document(imageCode);
                cardHausRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
                {
                    DocumentSnapshot cardHausSnapshot = task.Result;
                    if (cardHausSnapshot.Exists)
                    {
                        Dictionary<string, object> doc = cardHausSnapshot.ToDictionary();
                        imageLink = (string)doc["imageURL"];
                        finishGettingImageURL = true;
                        Debug.Log(imageLink);
                    }
                    else
                    {
                        messageText.text = "Invalid Video Code";
                        Debug.Log(String.Format("Document {0} does not exist!", cardHausSnapshot.Id));
                    }
                });
            }
        });
        thisPanel.SetActive(false);
    }
   
    //If cancel is clicked
    public void cancelClicked()
    {
        thisPanel.SetActive(false);
        codePanel.SetActive(false);
    }

    private void changeFooter()
    {
        footerBackground.SetActive(false);
        newFooter.SetActive(true);
    }

    public void exitClicked()
    {
        footerBackground.SetActive(true);
        newFooter.SetActive(false);
        markerlessImageObject.SetActive(false);
    }

    //Download texture from a URL;
    IEnumerator DownloadImage(string imageURL)
    {
        Debug.Log("From downloadimage" + imageURL);
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageURL);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);
        else
        {
            markerlessTexture = new Texture2D(2, 2);
            markerlessTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            markerlessImage.texture = markerlessTexture;
            codePanel.SetActive(false);
            markerlessImageObject.SetActive(true);
            changeFooter();
            Debug.Log("ds");
        }
    }
}
