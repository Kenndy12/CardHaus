using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Storage;

public class ModelARAudio : MonoBehaviour
{
    FirebaseFirestore db;
    FirebaseStorage storage;
    StorageReference storageRef;
    private StorageReference tempRef;

    // Start is called before the first frame update
    void Start()
    {
        db = FirebaseFirestore.DefaultInstance;

        storage = FirebaseStorage.DefaultInstance;
        storageRef = storage.GetReferenceFromUrl("gs://cardhaus-1ed70.appspot.com");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void listAudio()
    {

    }

    /*
    public async void insertIntoTemplateArray()
    {
        Query audioQuery = db.Collection("CardHausTemplate");
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
    }*/
}
