using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardStyleChooser : MonoBehaviour
{
    public RawImage previewImage;
    public RawImage option1;
    public RawImage option2;

    private Texture option1Tex;
    private Texture option2Tex;

    public GameObject detailPanels;

    public GameObject option1Panel;
    public GameObject option2Panel;

    public GameObject thisPanel;

    private int chosen = 1;
    // Start is called before the first frame update
    void Start()
    {
        option1Tex = option1.texture;
        option2Tex = option2.texture;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void option1Chosen()
    {
        previewImage.texture = option1Tex;
        chosen = 1;
    }

    public void option2Chosen()
    {
        previewImage.texture = option2Tex;
        chosen = 2;
    }

    public void nextClicked()
    {
        if(chosen==1)
        {
            option1Panel.SetActive(true);
            thisPanel.SetActive(false);
        }
        else
        {
            option2Panel.SetActive(true);
            thisPanel.SetActive(false);
        }
    }

    public void backClicked()
    {
        detailPanels.SetActive(true);
        thisPanel.SetActive(false); ;
    }

    public void backToChooser()
    {
        thisPanel.SetActive(true);
        option1Panel.SetActive(false);
        option2Panel.SetActive(false);
    }

}
