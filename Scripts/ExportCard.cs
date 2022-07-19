using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

using MongoDB.Driver;
using MongoDB.Bson;


public class ExportCard : MonoBehaviour
{
    MongoClient client = new MongoClient("mongodb+srv://CardHaus:cardHaus321@cardhauscluster.lznis.mongodb.net/?retryWrites=true&w=majority");
    IMongoDatabase database;
    IMongoCollection<BsonDocument> collection;

    public int templateID;
    private string templateName;
    private byte[] imageBytes;

    void start()
    {
        database = client.GetDatabase("CardHausDatabase");
        collection = database.GetCollection<BsonDocument>("CardHausTemplate");
        getImageBytes();
    }
    private void getImageBytes()
    {
        try
        {
            var filter = Builders<BsonDocument>.Filter.Eq("templateID", templateID);
            if (filter != null)
            {
                try
                {
                    BsonDocument result = collection.Find(filter).FirstOrDefault();
                    imageBytes = (byte[])result.GetElement("templateImage").Value;
                    templateName = (string) result.GetElement("templateName").Value;
                }
                catch (NullReferenceException)
                {
                    Debug.Log("Null Reference Exception");
                }
            }
            else
            {
                Debug.Log("Error");
            }
        }
        catch (FormatException)
        {
            Debug.Log("Format Exception");
        }
    }

    public void callExport()
    {
        export();
    }

    private IEnumerator export()
    {
        Texture2D image = new Texture2D(2, 2);
        image.LoadImage(imageBytes);
        yield return null;

        string filepath = Path.Combine(Application.temporaryCachePath, templateName + ".png");
        File.WriteAllBytes(filepath, image.EncodeToPNG());

        new NativeShare().AddFile(filepath).Share();
    }
}
