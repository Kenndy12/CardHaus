using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Firebase;
using Firebase.Extensions;
using Firebase.Storage;
using Firebase.Firestore;

public class VideoAR : MonoBehaviour
{
    //Video Code Panel
    public GameObject videoCodePanel;
    public TMP_InputField videoCodeField;
    public TMP_Text messageText;
    private string videoCode;
    private string videoLink;

    //Model AR Panel
    public GameObject audioPanel;
    private bool finishGettingAudio = false;
    public TMP_Dropdown audioDropdown;

    private GameObject ARVideoPlane;
    private ARVideoBehaviour ARBehaviour;

    public GameObject ARModePanel;

    //Audio
    public int optionChosen = 0;
    public static string audioChosen = "NULL";
    public static string audioLink = "-";

    FirebaseFirestore db;

    private WebCamTexture camTexture;
    private Rect screenRect;

    ArrayList audioArray = new ArrayList();

    [Serializable]
    public struct audio
    {
        public string audioName;
        public string audioLink;
    }

    void Awake()
    {
        //Firebase
        db = FirebaseFirestore.DefaultInstance;
        DontDestroyOnLoad(this);

        getAudioCodes();
    }

    void Update()
    {
        if(finishGettingAudio)
        {
            populateDropdown();
            finishGettingAudio = false;
        }
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

    public void modelArClicked()
    {
        if (ARModePanel.activeSelf)
        {
            ARModePanel.SetActive(false);
        }
        else
        {
            ARModePanel.SetActive(true);
        }
    }

    public void deactiveModelPanel()
    {
        audioPanel.SetActive(false);
    }

    public void enterVideoClicked()
    {
        videoCode = videoCodeField.text;

        DocumentReference docRef = db.Collection("UserTemplates").Document(videoCode);
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

    private async void getAudioCodes()
    {
        Query audioQuery= db.Collection("UserAudios");
        QuerySnapshot audioQuerySnapshot = await audioQuery.GetSnapshotAsync();

        foreach (DocumentSnapshot documentSnapshot in audioQuerySnapshot.Documents)
        {
            Dictionary<string, object> documentDictionary = documentSnapshot.ToDictionary();
            audio a = new audio();

            a.audioName = (string)documentDictionary["audioName"];
            a.audioLink = (string)documentDictionary["audioLink"];
            audioArray.Add(a);
        }
        finishGettingAudio = true;
    }

    private void populateDropdown()
    {
        List<string> dropOptions = new List<string>();

        dropOptions.Add("NONE");
        for(int i=0; i < audioArray.Count; i++)
        {
            audio tmp = (audio)audioArray[i];
            string tmpString = tmp.audioName;
            Debug.Log(tmpString);
            dropOptions.Add(tmpString);
        }
        audioDropdown.ClearOptions();
        audioDropdown.AddOptions(dropOptions);
    }

    public void dropDownChanged()
    {
        optionChosen = audioDropdown.value;

        if(optionChosen==0)
        {
            audioChosen = "NULL";
            audioLink = "-";
        }
        else
        {
            audio tmp = (audio)audioArray[optionChosen];
            string tmpAudioName = tmp.audioName;
            string tmpAudioLink = tmp.audioLink;
            audioChosen = tmpAudioName;
            audioLink = tmpAudioLink;
        }
    }

    public string passVideoLink()
    {
        return videoLink;
    }

    public string passAudioLink()
    {
        return audioLink;
    }
}
