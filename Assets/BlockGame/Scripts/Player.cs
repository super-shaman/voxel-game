using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public Camera cam;
    public Rigidbody rb;
    public Feet feet;
    bool flying = true;
    float speed = 1;

    void Start()
    {
        cam.opaqueSortMode = UnityEngine.Rendering.OpaqueSortMode.FrontToBack;
    }

    bool forward;
    bool right;
    bool left;
    bool backward;
    bool up;
    bool down;
    Vector3 rotation;
    bool paused = true;
    int jumpTimer = 0;
    bool jumping;

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(rb.velocity.magnitude);
        if (Input.GetMouseButtonDown(0))
        {
            if (paused)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                paused = false;
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!paused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                paused = true;
            }
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            flying = flying ? false : true;
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            forward = true;
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            forward = false;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            left = true;
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            left = false;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            backward = true;
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            backward = false;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            right = true;
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            right = false;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumping = false;
            up = true;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            up = false;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            down = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            down = false;
        }
        rotation.x += Input.GetAxis("Mouse X");
        rotation.y += Input.GetAxis("Mouse Y");
        Quaternion q = Quaternion.Euler(-rotation.y, rotation.x, 0);
        cam.transform.rotation = q;
        speed += Input.mouseScrollDelta.y;
        speed = speed < 1 ? 1 : speed;
    }

    void Fly()
    {
        Quaternion q = Quaternion.Euler(-rotation.y, rotation.x, 0);
        Vector3 move = new Vector3();
        if (forward)
        {
            move += new Vector3(0, 0, 1);
        }
        if (backward)
        {
            move += new Vector3(0, 0, -1);
        }
        if (left)
        {
            move += new Vector3(-1, 0, 0);
        }
        if (right)
        {
            move += new Vector3(1, 0, 0);
        }
        move = move.normalized;
        move *= (speed * speed);
        rb.AddForce(q * move - Physics.gravity);
        rb.drag = 3;
    }

    void Walk()
    {
        Quaternion q = Quaternion.Euler(0, rotation.x, 0);
        Vector3 move = new Vector3();
        if (forward)
        {
            move += new Vector3(0, 0, 1);
        }
        if (backward)
        {
            move += new Vector3(0, 0, -1);
        }
        if (left)
        {
            move += new Vector3(-1, 0, 0);
        }
        if (right)
        {
            move += new Vector3(1, 0, 0);
        }
        move = move.normalized;
        move *= (speed * speed);
        if (OnGround)
        {
            rb.drag = 0.001f;
            Vector3 newVel = Vector3.Lerp(rb.velocity, q * move, 0.5f);
            newVel = newVel - rb.velocity;
            newVel.y = 0;
            rb.AddForce(newVel, ForceMode.VelocityChange);
            Vector3 vel = rb.velocity;
            if (!jumping && up)
            {
                jumping = true;
                jumpTimer = 0;
                rb.drag = 0.001f;
                rb.velocity = new Vector3(vel.x, 10, vel.z);
                OnGround = false;
            }
        }else
        {
            rb.drag = 0.001f;
            Vector3 newVel = Vector3.Lerp(rb.velocity, q * move, 0.5f);
            newVel = newVel - rb.velocity;
            newVel.y = 0;
            rb.AddForce(newVel, ForceMode.VelocityChange);
        }
    }

    private void FixedUpdate()
    {
        if (jumping)
        {
            jumpTimer++;
            OnGround = false;
            if (jumpTimer >= 25)
            {
                jumpTimer = 0;
                jumping = false;
            }
        }
        if (flying)
        {
            Fly();
        }else
        {
            Walk();
        }
        Quaternion q = Quaternion.Euler(0, rotation.x, 0);
        transform.rotation = q;
        rb.angularVelocity = new Vector3();
    }

    bool OnGround;

    private void OnCollisionEnter(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint c = collision.contacts[i];
            if (Vector3.Dot(c.normal, new Vector3(0, 1, 0)) > 0.9)
            {
                OnGround = true;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint c = collision.contacts[i];
            if (Vector3.Dot(c.normal, new Vector3(0, 1, 0)) > 0.9)
            {
                OnGround = true;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        OnGround = false;
    }

}
