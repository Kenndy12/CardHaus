using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

using TMPro;

using Firebase;
using Firebase.Extensions;
using Firebase.Storage;
using Firebase.Firestore;


public class PreviewUploadedVideo : MonoBehaviour
{
    public RawImage image;
    public GameObject playPauseIcon;
    public GameObject clipboardIcon;

    private VideoPlayer vidPlayer;

    private bool isPaused = false;
    private bool firstRun = true;

    private string videoLink;
    private string videoCode;

    public TMP_Text messageText;
    public TMP_Text videoCodeText;

    FirebaseFirestore db;
    // Start is called before the first frame update
    void Start()
    {
        videoLink = UploadVideoScript.downloadLink;
        Debug.Log(videoLink);

        db = FirebaseFirestore.DefaultInstance;
        getObjectID();
        //Task.Run(async () => { await getObjectID(); }).Wait();
       
    }

    private void getObjectID()
    {
        Query capitalQuery = db.Collection("UserVideos").WhereEqualTo("videoLink", videoLink);
        capitalQuery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            QuerySnapshot capitalQuerySnapshot = task.Result;
            foreach (DocumentSnapshot documentSnapshot in capitalQuerySnapshot.Documents)
            {
                Dictionary<string, object> doc = documentSnapshot.ToDictionary();
                if (videoLink == (string)doc["videoLink"])
                {
                    videoCode = (string)documentSnapshot.Id;
                    Debug.Log(documentSnapshot.Id);
                    messageText.text = "Your video code is ";
                    Debug.Log("hey" + videoCode);
                    videoCodeText.text = videoCode;
                    clipboardIcon.SetActive(true);
                }
                // Newline to separate entries
            };
        });

       
    }

    IEnumerator playVideo()
    {
        playPauseIcon.SetActive(false);
        //set firsRun to false when video first plays
        firstRun = false;

        //Add vidPlayer to the GameObject
        vidPlayer = gameObject.AddComponent<VideoPlayer>();

        //Disable Play on Awake for both Video and Audio
        vidPlayer.playOnAwake = true;

        vidPlayer.source = VideoSource.Url;


        //Set video To Play then prepare Audio to prevent Buffering
        vidPlayer.url = videoLink;
        vidPlayer.Prepare();

        //Wait until video is prepared
        while (!vidPlayer.isPrepared)
        {
            yield return null;
        }

        Debug.Log("Done Preparing Video");

        //Assign the Texture from Video to RawImage to be displayed
        image.texture = vidPlayer.texture;

        //Play Video
        vidPlayer.Play();
        while (vidPlayer.isPlaying)
        {
            yield return null;
        }
    }

    public void playOrPause()
    {
        if (!firstRun && !isPaused)
        {
            vidPlayer.Pause();
            playPauseIcon.SetActive(true);
            isPaused = true;
        }
        else if (!firstRun && isPaused)
        {
            vidPlayer.Play();
            playPauseIcon.SetActive(false);
            isPaused = false;
        }
        else
        {
            StartCoroutine(playVideo());
        }
    }

    public void finishButtonClicked()
    {
        messageText.text = "";
        videoCodeText.text = "";
        clipboardIcon.SetActive(false);
        SceneManager.LoadScene("LibraryPage");
    }

    public void copyToClipboard()
    {
        Debug.Log(videoCodeText.text);
        string code = videoCodeText.text;
        Debug.Log(code);
        GUIUtility.systemCopyBuffer = code;
    }
}
