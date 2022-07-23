using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using TMPro;


public class AccountSettingManager : MonoBehaviour
{
    Firebase.Auth.FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;

    public TMP_Text usernameText;
    public TMP_Text emailText;

    public TMP_InputField password;
    public TMP_InputField confirmPassword;

    public TMP_Text warningText;
    void Awake()
    {
        usernameText.text = "Username: " + FirebaseAuth.DefaultInstance.CurrentUser.DisplayName;
        emailText.text = "Email: " + FirebaseAuth.DefaultInstance.CurrentUser.Email;
    }

    public void changePassword()
    {
        bool error = false;
        if(password.text == confirmPassword.text)
        {
            Firebase.Auth.FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
            string newPassword = password.text;
            if (user != null)
            {
                Debug.Log("hey");
                user.UpdatePasswordAsync(newPassword).ContinueWith(task => {
                    if (task.IsCanceled)
                    {
                        error = true;
                        warningText.text = "Encountered an error please try again";
                        Debug.LogError("UpdatePasswordAsync was canceled.");
                    }
                    if (task.IsFaulted)
                    {
                        error = true;
                        warningText.text = "Encountered an error please try again";
                        Debug.LogError("UpdatePasswordAsync encountered an error: " + task.Exception);
                    }
                    Debug.Log("Password updated successfully.");                  
                });
                if (!error)
                {
                    warningText.text = "Password successfully changed";
                }
            }
        }
        else
        {
            warningText.text = "Passwords does not match";
        }
    }

    public void goBack()
    {
        SceneManager.LoadScene("LibraryPage");
        warningText.text = "";
    }

    public void logOut()
    {
        warningText.text = "";
        FirebaseAuth.DefaultInstance.SignOut();
        SceneManager.LoadScene("CardHausApp");
        AuthManager.instance.clearFields();
    }

}
