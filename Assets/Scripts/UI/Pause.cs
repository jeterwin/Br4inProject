using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pause : MonoBehaviour
{
    public GameObject menu;
    public static bool GamePaused = false;


    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (menu.activeSelf)
            {
                Resumed();
            }
            else
            {
                Paused();
            }
        }
    }

    public void Resumed()
    {
        menu.SetActive(false);
        Time.timeScale = 1f;
        GameManager.IsPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Paused()
    {
        menu.SetActive(true);
        Time.timeScale = 0f;
        GameManager.IsPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


}


