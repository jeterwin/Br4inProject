using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneScript : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;

    public void LoadTargetScene()
    {
        SceneManager.LoadScene(sceneToLoad);
        GameManager.IsPaused = false;
    }
}
