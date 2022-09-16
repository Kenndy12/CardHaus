using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using TMPro;

public class AppManager : MonoBehaviour
{
    public FirebaseUser user;
    public GameObject warningPanel;

    public void openLibrary()
    {
        SceneManager.LoadScene("LibraryPage");
    }

    public void openARCamera()
    {
        SceneManager.LoadScene("CardHausAR");
    }

    public void openLoginScreen()
    {
        SceneManager.LoadScene("CardHausApp");
        warningPanel.SetActive(false);
    }

    public void openMyPacks()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser.DisplayName == "")
        {
            warningPanel.SetActive(true);
        }
        else
        {
            SceneManager.LoadScene("MyPacksPage");
        }

    }

    public void openAccountSetting()
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser.DisplayName == "")
        {
            warningPanel.SetActive(true);
        }
        else
        {
            SceneManager.LoadScene("AccountSettingPage");
        }
    }

    public void acknowlegdeError()
    {
        warningPanel.SetActive(false);
    }

    public void openAboutUs()
    {
        SceneManager.LoadScene("AboutUsPage");
    }
    public void learnMoreClicked()
    {
        Application.OpenURL("https://cardhaus530463295.wordpress.com/");
    }
}
