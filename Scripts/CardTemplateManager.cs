using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using TMPro;
using Firebase;
using Firebase.Auth;

using MongoDB.Driver;
using MongoDB.Bson;

public static class ButtonExtension
{
    public static void AddEventListener<T>(this Button button, T param, Action<T> OnClick)
    {
        button.onClick.AddListener(delegate ()
        {
            OnClick(param);
        });
    }
}

public class CardTemplateManager : MonoBehaviour
{
    public GameObject warningPanel;

    MongoClient client = new MongoClient("mongodb+srv://CardHaus:cardHaus321@cardhauscluster.lznis.mongodb.net/?retryWrites=true&w=majority");
    IMongoDatabase database;
    IMongoCollection<BsonDocument> collection;

    private int length;

    private int counter = 0;

    [Serializable]
    public struct cardTemplate
    {
        public string cardName;
        public string cardID;
        public bool isTemplate;
        public bool isVideoTemplate;
        public Texture2D cardImage;

        public Sprite Image;
    }

    cardTemplate[] allTemplates;
    ArrayList templateArray = new ArrayList();

    // Start is called before the first frame update
    void Start()
    {
        database = client.GetDatabase("CardHausDatabase");
        collection = database.GetCollection<BsonDocument>("CardHausTemplate");

        getCountFromMongo();
        insertIntoTemplateArray();
        instantiateArray();

        /*for(int i=0; i < n; i++)
        {
            g = Instantiate(buttonTemplate, transform);
            g.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = allTemplates[i].cardName;
            g.transform.GetChild(1).GetComponent<Image>().sprite = allTemplates[i].Image;

            g.GetComponent<Button>().AddEventListener(i, ItemClicked); 
        }

        Destroy(buttonTemplate);*/
        
    }

    private void insertIntoTemplateArray()
    {
        var filter = Builders<BsonDocument>.Filter.Eq("isTemplate", true);

        foreach(BsonDocument post in collection.Find(filter).ToListAsync().Result)
        {
            cardTemplate tmp = new cardTemplate();
            tmp.cardName = (string)post.GetElement("templateName").Value;
            tmp.cardID = (string)post.GetElement("templateID").Value;
            tmp.isTemplate = (bool)post.GetElement("isTemplate").Value;
            tmp.isVideoTemplate = (bool)post.GetElement("isVideoCard").Value;
            tmp.cardImage = new Texture2D(2, 2);
            tmp.cardImage.LoadImage((byte[])post.GetElement("templateImage").Value);

            templateArray.Add(tmp);
            counter++;
        }
    }

    private void instantiateArray()
    {
        GameObject buttonTemplate = transform.GetChild(0).gameObject;
        GameObject g;
        for (int i = 0; i < length; i++)
        {
            Debug.Log("hi");
            cardTemplate tmp = (cardTemplate)templateArray[i];
            g = Instantiate(buttonTemplate, transform);
            g.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = tmp.cardName;
            g.transform.GetChild(1).GetComponent<RawImage>().texture = tmp.cardImage;

            g.GetComponent<Button>().AddEventListener(i, ItemClicked);
        }

        Destroy(buttonTemplate);
    }

    private void getCountFromMongo()
    {
        var filter = Builders<BsonDocument>.Filter.Eq("isTemplate", true);
        length = (int)collection.Count(filter);
    }

    void ItemClicked(int index)
    {
        Debug.Log(index);
        switch(index)
        {
            case 0:
                if (checkLoggedIn())
                {
                    Debug.Log(FirebaseAuth.DefaultInstance.CurrentUser.DisplayName);
                    SceneManager.LoadScene("VideoGreetingCard");
                }
                else
                {
                    callWarningPanel();
                }                             
                break;
            case 1:
                SceneManager.LoadScene("WeddingCardDetails");
                break;
        }
    }

    private bool checkLoggedIn()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser.DisplayName == "")
            return false;
        else
            return true;
    }

    private void callWarningPanel()
    {
        warningPanel.SetActive(true);
    }
}
