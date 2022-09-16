using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ARCameraManager : MonoBehaviour
{
   public void exitCamera()
    {
        SceneManager.LoadScene("LibraryPage");
    }
}
