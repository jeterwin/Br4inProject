using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings : MonoBehaviour
{
    public GameObject main_butt;
    public GameObject settings_butt;


    public void ChangeMenu()
    {
        main_butt.SetActive(false);
        settings_butt.SetActive(true);
    }

    public void ChangeBackMenu()
    {
        main_butt.SetActive(true);
        settings_butt.SetActive(false);
    }

}
