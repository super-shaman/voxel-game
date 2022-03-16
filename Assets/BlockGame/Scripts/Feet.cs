using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feet : MonoBehaviour
{

    public bool OnGround;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        OnGround = true;
    }

    private void OnTriggerStay(Collider other)
    {
        OnGround = true;
    }

    private void OnTriggerExit(Collider other)
    {
        OnGround = false;
    }

}
