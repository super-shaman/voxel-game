using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject scroll;
    void Start()
    {
        
    }
    float s = 0;
    // Update is called once per frame
    void Update()
    {
        if (World.world.paused)
        {
            s -= Input.mouseScrollDelta.y;
            s = s < 0 ? 0 : s;
            scroll.transform.position = new Vector3(0, s*100, 0);
        }
    }
}
