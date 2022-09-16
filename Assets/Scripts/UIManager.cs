using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject registerPanel;
    public GameObject loginPanel;
    public GameObject forgetPasswordPanel;

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
            forgetPasswordPanel.SetActive(false);
        }
    }

    public void goToLoginScreen()
    {
        if(loginPanel != null)
        {
            registerPanel.SetActive(false);
            loginPanel.SetActive(true);
            forgetPasswordPanel.SetActive(false);
        }
    }

    public void forgetPassword()
    {
        if (forgetPasswordPanel != null)
        {
            registerPanel.SetActive(false);
            loginPanel.SetActive(false);
            forgetPasswordPanel.SetActive(true);
        }
    }

}
