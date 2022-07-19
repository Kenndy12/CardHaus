using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

using TMPro;

using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Storage;

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

    private int length = 0;

    [Serializable]
    public struct cardTemplate
    {
        public string cardName;
        public string cardID;
        public string imageURL;
        public bool isTemplate;
        public bool isVideoTemplate;
        public Texture2D cardImage;
    }

    cardTemplate[] allTemplates;
    ArrayList templateArray = new ArrayList();

    FirebaseFirestore db;
    FirebaseStorage storage;
    StorageReference storageRef;
    private StorageReference tempRef;

    private bool called = false;

    public RawImage n;

    // Start is called before the first frame update
    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;

        storage = FirebaseStorage.DefaultInstance;
        storageRef = storage.GetReferenceFromUrl("gs://cardhaus-1ed70.appspot.com");

        insertIntoTemplateArray();
        
    }

    void Update()
    {
        if (called)
        {
            instantiateArray();
            called = false;
        }
    }

    IEnumerator DownloadImage(cardTemplate tmp, RawImage m)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(tmp.imageURL);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            Debug.Log(request.error);
        else
        {
            tmp.cardImage = new Texture2D(2, 2);
            tmp.cardImage = ((DownloadHandlerTexture)request.downloadHandler).texture;
            m.texture = tmp.cardImage;
            Debug.Log("ds");
        }
    }

    public async void insertIntoTemplateArray()
    {
        Query allCitiesQuery = db.Collection("CardHausTemplate");
        QuerySnapshot allCitiesQuerySnapshot = await allCitiesQuery.GetSnapshotAsync();

        foreach (DocumentSnapshot documentSnapshot in allCitiesQuerySnapshot.Documents)
        {
            Dictionary<string, object> documentDictionary = documentSnapshot.ToDictionary();
            Debug.Log(documentDictionary["templateName"]);
            cardTemplate tmp = new cardTemplate();
            
            tmp.cardName = (string)documentDictionary["templateName"];
            tmp.cardID = (string)documentDictionary["templateID"];
            tmp.isTemplate = (bool)documentDictionary["isTemplate"];
            tmp.isVideoTemplate = (bool)documentDictionary["isVideoCard"];
            tmp.imageURL = (string)documentDictionary["imageURL"];     
            templateArray.Add(tmp);
            length++;
        }
        called = true;
    }


    public void instantiateArray()
    {
        Debug.Log("gety");
        GameObject buttonTemplate = transform.GetChild(0).gameObject;
        GameObject g;
        for (int i = 0; i < length; i++)
        {
            cardTemplate tmp = (cardTemplate)templateArray[i];
            
            g = Instantiate(buttonTemplate, transform);
            g.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = tmp.cardName;
            StartCoroutine(DownloadImage(tmp, g.transform.GetChild(1).GetComponent<RawImage>()));
            g.GetComponent<Button>().AddEventListener(i, ItemClicked);
        }

        Destroy(buttonTemplate);
    }

    void ItemClicked(int index)
    {
        Debug.Log(index);
        switch(index)
        {
            case 0:
                if (checkLoggedIn())
                {
                    SceneManager.LoadScene("BirthdayCardTemplate");
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
