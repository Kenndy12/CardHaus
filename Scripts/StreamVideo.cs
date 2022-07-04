using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class StreamVideo : MonoBehaviour
{
    public RawImage image;
    public GameObject playPauseIcon;

    public VideoClip videoToPlay;

    private VideoPlayer vidPlayer;
    private VideoSource videoSource;

    private bool isPaused = false;
    private bool firstRun = true;
    // Start is called before the first frame update

    IEnumerator playVideo()
    {
        playPauseIcon.SetActive(false);
        //set firsRun to false when video first plays
        firstRun = false;

        //Add vidPlayer to the GameObject
        vidPlayer = gameObject.AddComponent<VideoPlayer>();

        //Disable Play on Awake for both Video and Audio
        vidPlayer.playOnAwake = false;

        vidPlayer.source = VideoSource.VideoClip;

        //Set video To Play then prepare Audio to prevent Buffering
        vidPlayer.clip = videoToPlay;
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

}
