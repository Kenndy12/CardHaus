using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

using System.IO;

using Firebase;
using Firebase.Extensions;
using Firebase.Storage;

using MongoDB.Driver;
using MongoDB.Bson;



public class UploadVideoScript : MonoBehaviour
{
    FirebaseStorage storage;
    StorageReference storageRef;

    MongoClient client = new MongoClient("mongodb+srv://CardHaus:cardHaus321@cardhauscluster.lznis.mongodb.net/?retryWrites=true&w=majority");
    IMongoDatabase database;
    IMongoCollection<BsonDocument> collection;

    public VideoPlayer video;
    private string path;
    private StorageReference tempRef;

    // Start is called before the first frame update
    void Start()
    {
        storage = FirebaseStorage.DefaultInstance;
        storageRef = storage.GetReferenceFromUrl("gs://cardhaus-1ed70.appspot.com");

        database = client.GetDatabase("UserVideoDB");
        collection = database.GetCollection<BsonDocument>("UserVideoCollection");
    }

    public void OpenFileExplorer()
    {
        path = EditorUtility.OpenFilePanel("Show all images (.mp4) ", "", "mp4");
        Debug.Log(path);
    }

    public async void  uploadFile()
    {
        bool uploaded = false;
        tempRef = storageRef.Child("test.mp4");
        var newMetadata = new MetadataChange();
        newMetadata.ContentType = "video/mp4";

        await tempRef.PutFileAsync(path, newMetadata).ContinueWithOnMainThread((task) => {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log(task.Result);
                Debug.Log(task.Exception.ToString());
            }
            else
            {
                Debug.Log(task.Result);
                Debug.Log("File Uploaded Successfully!");
            }
        });

        await tempRef.GetDownloadUrlAsync().ContinueWithOnMainThread(task => {
            if (!task.IsFaulted && !task.IsCanceled)
            {
                string link = task.Result.ToString();
                Debug.Log("Download URL: " + link);
                var document = new BsonDocument { { "videoURL", link } };
                collection.InsertOne(document);
            }
        });

        
    }

    public void uploadToMongoDB()
    {
        
    }
}
