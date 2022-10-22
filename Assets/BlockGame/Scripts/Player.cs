
using UnityEngine;

public class Player : MonoBehaviour
{

    public Camera cam;
    public Rigidbody rb;
    public float jumpPower;
    bool flying = true;
    float speed = 1;
    public WorldPosition wp;
    Vector3 camLocalPosition;
    void Start()
    {
        camLocalPosition = cam.transform.localPosition;
        wp = new WorldPosition(new Vector3Int(), new Vector3());
        cam.opaqueSortMode = UnityEngine.Rendering.OpaqueSortMode.FrontToBack;
        cam.transparencySortMode = TransparencySortMode.Perspective;
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
    }
    Vector3 previousLocalPosition = new Vector3();
    public void SetWorldPos(Vector3 v)
    {
        previousLocalPosition = v;
        wp.Add(v);
    }

    public void UpdateWorldPos()
    {
        transform.position = transform.position - previousLocalPosition;
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
    float zoom = 0;

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(rb.velocity.magnitude);
        if (Input.GetMouseButtonDown(0))
        {
            if (!World.world.paused)
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
        if (!jumping && Input.GetKeyDown(KeyCode.Space))
        {
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
        zoom += Input.mouseScrollDelta.y;
        zoom = zoom < 0 ? 0 : zoom;
        cam.transform.localPosition = camLocalPosition - cam.transform.forward * zoom;
        UpdateSpeed();
    }

    float speedAdjustTimer = 0;
    bool adjustingSpeed = false;
    float speedDelta = 0;
    void UpdateSpeed()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            speed += 1;
            speedDelta = 1;
            speedAdjustTimer = 0;
            adjustingSpeed = true;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            speed -= 1;
            speedDelta = -1;
            speed = speed < 1 ? 1 : speed;
            speedAdjustTimer = 0;
            adjustingSpeed = true;
        }
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            speedAdjustTimer = 0;
            adjustingSpeed = false;
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            speedAdjustTimer = 0;
            adjustingSpeed = false;
        }
        if (adjustingSpeed)
        {
            speedAdjustTimer += Time.deltaTime;
            if (speedAdjustTimer >= 0.2)
            {
                speed += speedDelta;
                speed = speed < 1 ? 1 : speed;
            }
        }
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
        move = q*move.normalized;
        move *= 1+(speed * speed*0.5f);
        rb.velocity = Vector3.Lerp(rb.velocity,move,0.25f);
        rb.drag = 3;
    }

    void Walk()
    {
        rb.drag = 0.001f;
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
        move *= 1 + (speed * speed * 0.25f);
        if (!jumping && OnGround)
        {
            Vector3 vel = rb.velocity;
            Vector3 newVel = Vector3.Lerp(vel, q * move, 0.5f);
            newVel.y = 0;
            float d = Vector3.Dot(new Vector3(newVel.x, newVel.z).normalized, new Vector3(currentImpulse.x, currentImpulse.z).normalized);
            d = (1.0f + d) / 2.0f * 0.5f + 0.5f;
            // d -= 0.5f;
            // d = d < 0 ? 0 : d * 2;
            newVel.x *= d;
            newVel.z *= d;
            rb.velocity = newVel;
            if (up)
            {
                jumping = true;
                jumpTimer = 0;
                jumpStrength = 0;
            }
        }else
        {
            Vector3 vel = rb.velocity;
            Vector3 newVel = Vector3.Lerp(vel, q * move, 0.5f);
            newVel.y = vel.y;
            float d = Vector3.Dot(new Vector3(newVel.x,newVel.z).normalized, new Vector3(currentImpulse.x,currentImpulse.z).normalized);
            d = (1.0f + d) / 2.0f*0.5f+0.5f;
            // d -= 0.5f;
            // d = d < 0 ? 0 : d * 2;
            newVel.x *= d;
            newVel.z *= d;
            rb.velocity = newVel;
        }
        if (jumping)
        {
            jumpStrength = Mathf.Lerp(jumpStrength, jumpPower, 0.75f);
            Vector3 newVel = rb.velocity+new Vector3(0, jumpStrength, 0);
            rb.velocity = newVel;
            jumpTime++;
            if (jumpTime == jumpSteps)
            {
                jumpTime = 0;
                jumping = false;
            }
        }
    }
    float jumpStrength;
    int jumpTime = 0;
    int jumpSteps = 16;
    private void FixedUpdate()
    {
        if (flying)
        {
            Fly();
        }else
        {
            Walk();
        }
        OnGround = false;
        currentImpulse = new Vector3();
        collisionCount = 0;
    }

    bool OnGround;
    Vector3 currentImpulse = new Vector3();
    int collisionCount = 0;

    private void OnCollisionEnter(Collision collision)
    {
        bool counts = true;
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint c = collision.contacts[i];
            if (Vector3.Dot(c.normal, new Vector3(0, 1, 0)) > 0.975)
            {
                OnGround = true;
                counts = false;
            }
        }
        if (counts)
        {
            currentImpulse += collision.impulse;
            collisionCount++;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        bool counts = true;
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint c = collision.contacts[i];
            if (Vector3.Dot(c.normal, new Vector3(0, 1, 0)) > 0.975)
            {
                OnGround = true;
                counts = false;
            }
        }
        if (counts)
        {
            currentImpulse += collision.impulse;
            collisionCount++;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
    }

}
