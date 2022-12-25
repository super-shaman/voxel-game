using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject scroll;
    public GameObject controlScroll;
    public GameObject controlMenu;
    bool controlsOpen = false;
    void Start()
    {
        
    }

    float s = 0;

    void Update()
    {
        if (World.world != null && World.world.paused)
        {
            if (!controlsOpen)
            {
                s -= Input.mouseScrollDelta.y;
                s = s < 0 ? 0 : s;
                scroll.transform.position = new Vector3(0, s * 100, 0);
            }else
            {
                s -= Input.mouseScrollDelta.y;
                s = s < 0 ? 0 : s;
                controlScroll.transform.position = new Vector3(0, s * 100, 0);
            }
        }else if (controlsOpen)
        {
            CloseControls();
        }
    }

    public void OpenControls()
    {
        s = 0;
        controlsOpen = true;
        controlMenu.SetActive(true);
        scroll.SetActive(false);
    }
    public void CloseControls()
    {
        s = 0;
        controlsOpen = false;
        controlMenu.SetActive(false);
        scroll.SetActive(true);
    }

}
