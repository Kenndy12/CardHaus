using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using TMPro;


public class AccountSettingManager : MonoBehaviour
{
    public TMP_Text usernameText;
    public TMP_Text emailText;

    void Awake()
    {
        usernameText.text = "Username: " + FirebaseAuth.DefaultInstance.CurrentUser.DisplayName;
        emailText.text = "Email: " + FirebaseAuth.DefaultInstance.CurrentUser.Email;
    }

    public void goBack()
    {
        SceneManager.LoadScene("LibraryPage");
    }

    public void logOut()
    {
        FirebaseAuth.DefaultInstance.SignOut();
        SceneManager.LoadScene("CardHausApp");
        AuthManager.instance.clearFields();
    }

}
