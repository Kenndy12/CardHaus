using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using TMPro;


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
    [Serializable]
    public struct cardTemplate
    {
        public string cardName;
        public Sprite Image;
    }

    [SerializeField] cardTemplate[] allTemplates;

    // Start is called before the first frame update
    void Start()
    {
        GameObject buttonTemplate = transform.GetChild(0).gameObject;
        GameObject g;

        int n = allTemplates.Length;


        for(int i=0; i < n; i++)
        {
            g = Instantiate(buttonTemplate, transform);
            g.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = allTemplates[i].cardName;
            g.transform.GetChild(1).GetComponent<Image>().sprite = allTemplates[i].Image;

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
                SceneManager.LoadScene("VideoGreetingCard");
                break;
            case 1:
                SceneManager.LoadScene("WeddingCardDetails");
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
