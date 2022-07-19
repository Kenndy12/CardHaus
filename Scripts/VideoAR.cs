using System;
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
    public TMP_Text messageText;
    private string videoCode;
    private string videoLink;

    private GameObject ARVideoPlane;
    private ARVideoBehaviour ARBehaviour;

    MongoClient client = new MongoClient("mongodb+srv://CardHaus:cardHaus321@cardhauscluster.lznis.mongodb.net/?retryWrites=true&w=majority");
    IMongoDatabase database;
    IMongoCollection<BsonDocument> collection;

    void Awake()
    {
        database = client.GetDatabase("CardHausDatabase");
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
        messageText.text = "";
    }

    public void enterClicked()
    {
        videoCode = videoCodeField.text;


        try
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(videoCode));
            if (filter != null)
            {
                try
                {
                    var result = collection.Find(filter).FirstOrDefault().GetValue("videoURL");
                    Debug.Log(result.ToString());
                    videoLink = result.ToString();
                    deactivateVideoPanel();
                }
                catch (NullReferenceException)
                {
                    messageText.text = "Invalid video code";
                }
            }
            else
            {
                Debug.Log("Invalid video code");
            }
        }
        catch (FormatException)
        {
            messageText.text = "Invalid video code";
        }

    }

    public string passVideoLink()
    {
        return videoLink;
    }
}
