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


public class MyPacksScript : MonoBehaviour
{
    [Serializable]
    public struct userTemplate
    {
        public string docID;
        public string cardName;
        public string userID; 
        public string imageURL;
        public bool isVideoTemplate;
        public string videoLink;
        public Texture2D cardImage;
    }

    ArrayList userTemplateArray = new ArrayList();

    //Firebase
    FirebaseFirestore db;
    FirebaseStorage storage;
    StorageReference storageRef;
    private StorageReference tempRef;

    private bool called = false;

    //Information to be saved
    public static string docIDToBeSaved;
    public static string cardNameToBeSaved;
    public static string userIDToBeSaved;
    public static string imageURLToBeSaved;
    public static bool isVideoTemplateToBeSaved;
    public static string videoLinkToBeSaved;

    // Start is called before the first frame update
    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;

        storage = FirebaseStorage.DefaultInstance;
        storageRef = storage.GetReferenceFromUrl("gs://cardhaus-1ed70.appspot.com");

        populateUserTemplateArray();
    }

    void Update()
    {
        if (called)
        {
            instantiateArray();
            called = false;
        }
    }

    private async void populateUserTemplateArray()
    {
        Query userTemplateQuery = db.Collection("UserTemplates").WhereEqualTo("userID", FirebaseAuth.DefaultInstance.CurrentUser.UserId) ;
        QuerySnapshot userTemplateQuerySnapshot = await userTemplateQuery.GetSnapshotAsync();
        foreach (DocumentSnapshot documentSnapshot in userTemplateQuerySnapshot.Documents)
        {
            Dictionary<string, object> documentDictionary = documentSnapshot.ToDictionary();

            userTemplate tmp = new userTemplate();
            tmp.docID = (string)documentSnapshot.Id;
            tmp.cardName = (string)documentDictionary["cardName"];
            tmp.userID = (string)documentDictionary["userID"];
            tmp.imageURL = (string)documentDictionary["imageLink"];
            tmp.isVideoTemplate = (bool)documentDictionary["isVideoAR"];

            if (tmp.isVideoTemplate)
            {
                tmp.videoLink = (string)documentDictionary["videoLink"];
            }
            else
            {
                tmp.videoLink = "NONE";
            }

            userTemplateArray.Add(tmp);
        }
        called = true;
    }

    IEnumerator DownloadImage(userTemplate tmp, RawImage m)
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
        }
    }

    public void instantiateArray()
    {
        GameObject buttonTemplate = transform.GetChild(0).gameObject;
        GameObject g;
        for (int i = 0; i < userTemplateArray.Count ; i++)
        {
            userTemplate tmp = (userTemplate)userTemplateArray[i];

            g = Instantiate(buttonTemplate, transform);
            g.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = tmp.cardName;
            StartCoroutine(DownloadImage(tmp, g.transform.GetChild(1).GetComponent<RawImage>()));

            if (tmp.isVideoTemplate)
            {
                g.transform.GetChild(2).gameObject.SetActive(true);
            }
            g.GetComponent<Button>().AddEventListener(i, ItemClicked);
        }
        Destroy(buttonTemplate);
    }

    void ItemClicked(int index)
    {
        userTemplate tmp = (userTemplate)userTemplateArray[index];
        docIDToBeSaved = tmp.docID;
        cardNameToBeSaved = tmp.cardName;
        userIDToBeSaved = tmp.userID;
        imageURLToBeSaved = tmp.imageURL;
        isVideoTemplateToBeSaved = tmp.isVideoTemplate;
        if(isVideoTemplateToBeSaved)
        {
            videoLinkToBeSaved = tmp.videoLink;
        }
        else
        {
            videoLinkToBeSaved = "NONE";
        }
        SceneManager.LoadScene("ViewMyPacksDetails");
    }
}
