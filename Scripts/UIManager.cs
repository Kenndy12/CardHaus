using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject registerPanel;
    public GameObject loginPanel;

    private void Awake()
    {
        if(instance==null)
        {
            instance = this;
        }
        else if(instance != null)
        {
            Debug.Log("Instance already exist, destroying object");
            Destroy(this);
        }
    }
    public void goToRegisterScreen()
    {
        if(registerPanel !=null)
        {
            registerPanel.SetActive(true);
            loginPanel.SetActive(false);
        }
    }

    public void goToLoginScreen()
    {
        if(loginPanel != null)
        {
            registerPanel.SetActive(false);
            loginPanel.SetActive(true);
        }
    }

}
