using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Firebase;
using Firebase.Extensions;
using Firebase.Storage;
using Firebase.Firestore;

using ZXing;
using ZXing.QrCode;

public class VideoAR : MonoBehaviour
{
    public GameObject videoCodePanel;
    public TMP_InputField videoCodeField;
    public TMP_Text messageText;
    private string videoCode;
    private string videoLink;

    private GameObject ARVideoPlane;
    private ARVideoBehaviour ARBehaviour;

    FirebaseFirestore db;

    private WebCamTexture camTexture;
    private Rect screenRect;

    void Awake()
    {
        //Firebase
        db = FirebaseFirestore.DefaultInstance;
        DontDestroyOnLoad(this);
    }

    public void videoArClicked()
    {
        videoCodePanel.SetActive(true);
    }

    public void deactivateVideoPanel()
    {
        videoCodePanel.SetActive(false);
        messageText.text = "";
    }

    public void enterClicked()
    {
        videoCode = videoCodeField.text;

        DocumentReference docRef = db.Collection("UserVideos").Document(videoCode);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {             
                Dictionary<string, object> doc = snapshot.ToDictionary();
                videoLink = (string)doc["videoLink"];
                Debug.Log(videoLink);
                deactivateVideoPanel();
            }
            else
            {
                messageText.text = "Invalid Video Code";
                Debug.Log(String.Format("Document {0} does not exist!", snapshot.Id));
            }
        });


    }

    public string passVideoLink()
    {
        return videoLink;
    }
}
