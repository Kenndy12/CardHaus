using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MongoDB.Driver;
using MongoDB.Bson;

public class VideoAR : MonoBehaviour
{
    public GameObject videoCodePanel;
    public TMP_InputField videoCodeField;

    private string videoCode;
    private string videoLink;

    private GameObject ARVideoPlane;
    private ARVideoBehaviour ARBehaviour;

    MongoClient client = new MongoClient("mongodb+srv://CardHaus:cardHaus321@cardhauscluster.lznis.mongodb.net/?retryWrites=true&w=majority");
    IMongoDatabase database;
    IMongoCollection<BsonDocument> collection;

    void Awake()
    {
        database = client.GetDatabase("UserVideoDB");
        collection = database.GetCollection<BsonDocument>("UserVideoCollection");
        DontDestroyOnLoad(this);
    }

    public void videoArClicked()
    {
        videoCodePanel.SetActive(true);
    }

    public void deactivateVideoPanel()
    {
        videoCodePanel.SetActive(false);
    }

    public void enterClicked()
    {
        videoCode = videoCodeField.text;

        var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(videoCode));
        Debug.Log(filter);
        if (filter != null)
        {
            var result = collection.Find(filter).FirstOrDefault().GetValue("videoURL");
            Debug.Log(result.ToString());
            videoLink = "https://unity-youtube-dl-server.herokuapp.com/" + result.ToString();
        }
        else
        {
            Debug.Log("Invalid video code");
        }
        deactivateVideoPanel();
    }

    public string passVideoLink()
    {
        return videoLink;
    }
}
