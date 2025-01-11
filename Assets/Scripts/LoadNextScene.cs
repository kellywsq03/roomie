using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadNextScene : MonoBehaviour
{
    public void LoadGardenScene() {
        SceneManager.LoadScene(2);
    }

    public void LoadRoomScene() {
        SceneManager.LoadScene(1);
    }
}
