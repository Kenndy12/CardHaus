using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

using TMPro;
public class ViewMyPacksDetails : MonoBehaviour
{
    //Informations about card
    private string docID;
    private string cardName;
    private string userID;
    private string imageURL;
    private bool isVideoTemplate;
    private string videoLink;

    //Scene details
    public TMP_Text titleText;
    public TMP_Text cardCodeText;
    public TMP_Text cardCode;
    public GameObject videoAttached;
    public RawImage previewImage;
    private Texture2D previewImageTexture;
    public GameObject saveConfirmPanel;

    //Video Player Related
    public RawImage image;
    public GameObject playPauseIcon;
    private VideoPlayer vidPlayer;
    private bool isPaused = false;
    private bool firstRun = true;

    
    private bool isCoroutineExecuting = false;
    // Start is called before the first frame update
    void Start()
    {
        populateVariables();
        StartCoroutine(DownloadImage(previewImage, imageURL));
        tidyScene();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void populateVariables()
    {
        docID = MyPacksScript.docIDToBeSaved;
        cardName = MyPacksScript.cardNameToBeSaved;
        userID = MyPacksScript.userIDToBeSaved;
        imageURL = MyPacksScript.imageURLToBeSaved;
        isVideoTemplate = MyPacksScript.isVideoTemplateToBeSaved;
        videoLink = MyPacksScript.videoLinkToBeSaved;

        if(!isVideoTemplate)
        {
            videoAttached.SetActive(false);
        }
        else
        {
            videoAttached.SetActive(true);
        }
    }

    private void tidyScene()
    {
        titleText.text = cardName;
        if (isVideoTemplate)
        {
            cardCodeText.text = "Your video AR code is ";
            cardCode.text = docID;
        }
        else
        {
            cardCodeText.text = "Your markerless AR code is ";
            cardCode.text = docID;
        }
    }

    IEnumerator DownloadImage(RawImage m, string imageURL)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageURL);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);
        else
        {
            previewImageTexture = new Texture2D(2, 2);
            previewImageTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            m.texture = previewImageTexture;
        }
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

    public void copyToClipboard()
    {
        Debug.Log(cardCode.text);
        string code = cardCode.text;
        Debug.Log(code);
        GUIUtility.systemCopyBuffer = code;
    }

    public void saveCalled()
    {
        StartCoroutine(saveToGallery());
    }

    public void shareCalled()
    {
        StartCoroutine(export());
    }

    private IEnumerator export()
    {
        yield return null;
        string filepath = Path.Combine(Application.temporaryCachePath, cardName + ".png");
        File.WriteAllBytes(filepath, previewImageTexture.EncodeToPNG());

        Debug.Log(Application.temporaryCachePath);
        new NativeShare().AddFile(filepath).Share();
    }

    private IEnumerator saveToGallery()
    {
        yield return new WaitForEndOfFrame();

        // Save the screenshot to Gallery/Photos
        NativeGallery.Permission permission = NativeGallery.SaveImageToGallery(previewImageTexture, "CardHausGallery", cardName + ".png", (success, path) => Debug.Log("Media save result: " + success + " " + path));

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
}
