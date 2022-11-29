
using UnityEngine;

public class Player : MonoBehaviour
{

    public Camera cam;
    public float jumpPower;
    public float GravityPower;
    public float MoveSpeed;
    public Vector3 velocity;
    bool flying = true;
    float speed = 1;
    public WorldPosition wp;
    public WorldPosition updateWp;
    Vector3 camLocalPosition;
    void Start()
    {
        camLocalPosition = cam.transform.localPosition;
        wp = new WorldPosition(new Vector3Int(), new Vector3());
        updateWp = new WorldPosition(wp);
        cam.opaqueSortMode = UnityEngine.Rendering.OpaqueSortMode.FrontToBack;
        cam.transparencySortMode = TransparencySortMode.Perspective;
    }
    Vector3 previousLocalPosition = new Vector3();
    public void SetWorldPos(Vector3 v)
    {
        previousLocalPosition = v;
        updateWp.Add(v);
    }


    public void UpdateWorldPos()
    {
        wp.Set(updateWp);
        transform.position = transform.position - previousLocalPosition;
    }
    bool forward;
    bool right;
    bool left;
    bool backward;
    bool up;
    bool down;
    public bool run = false;
    float runTimer = -1;
    Vector3 rotation;
    bool paused = true;
    float jumpTimer = 0;
    bool jumping;
    float zoom = 0;
    bool Zoom = false;
    // Update is called once per frame
    void Update()
    {
        //Debug.Log(rb.velocity.magnitude);
        if (Input.GetMouseButtonDown(0))
        {
            if (!World.world.paused && paused)
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
        if (runTimer >= 0)
        {
            runTimer += Time.deltaTime;
            if (runTimer >= 0.125f)
            {
                runTimer = -1;
            }
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            forward = true;
            if (runTimer >= 0 && runTimer < 0.125f)
            {
                runTimer = -1;
                run = true;
            }
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            forward = false;
            run = false;
            if (runTimer < 0)
            {
                runTimer = 0;
            }
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            run = true;
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Zoom = true;
        }
        if (Input.GetKeyUp(KeyCode.C))
        {
            Zoom = false;
        }
        if (Zoom)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 25+(zoom < 0 ? zoom : 0), 0.25f * Time.deltaTime > 1 ? 1 : 1 * Time.deltaTime);
        }else if (cam.fieldOfView != 90)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 90, 1 * Time.deltaTime > 1 ? 1 : 1 * Time.deltaTime);
            if (90-cam.fieldOfView < 0.001f)
            {
                cam.fieldOfView = 90;
            }
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

        //Physics simulation for player now done on the render thread

        if (!paused)
        {
            zoom += Input.mouseScrollDelta.y;
            zoom = zoom < -20 ? -20 : zoom;
            cam.transform.localPosition = camLocalPosition - cam.transform.forward * (zoom < 0 ? 0 : zoom);
            UpdateSpeed();
            a = Mathf.CeilToInt(Time.deltaTime / (1.0f / 60.0f));
            a *= Mathf.FloorToInt((velocity.magnitude > 64 ? 64 : velocity.magnitude) + 1) * 8;
            for (int i = 0; i < a; i++)
            {
                if (flying)
                {
                    Fly();
                }
                else
                {
                    Walk();
                }
                if (!OnGround)
                {
                    velocity += new Vector3(0, -GravityPower * Time.deltaTime / a, 0);
                }
                transform.position += velocity * Time.deltaTime / a;
                OnGround = false;
                currentImpulse = new Vector3();
                collisionCount = 0;
                chunk.SimulatePlayer(this);
            }
        }
    }
    int a;

    public bool OnGround;
    Vector3 currentImpulse = new Vector3();
    int collisionCount = 0;
    float speedAdjustTimer = 0;
    bool adjustingSpeed = false;
    float speedDelta = 0;
    float jumpStrength;

    public WorldChunk chunk;

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
        if (down)
        {
            move += new Vector3(0, -1, 0);
        }
        move = q*move.normalized;
        move *= 1+(speed * speed*0.5f);
        move *= MoveSpeed * (run ? 2.5f : 1);
        float timer = Time.deltaTime*10 / a;
        timer = timer > 1 ? 1 : timer;
        velocity += (move - velocity) * timer;
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
        move *= 1 + (speed * speed * 0.25f);
        float timer = Time.deltaTime * 30 / a;
        timer = timer > 1 ? 1 : timer;
        if (OnGround)
        {
            Vector3 vel = velocity;
            Vector3 newVel = q * move*MoveSpeed*(run?2.5f:1);
            newVel.y = 0;
            velocity.x += (newVel.x - velocity.x) * timer;
            velocity.z += (newVel.z - velocity.z) * timer;
            if (jumping)
            {
                jumpTimer += Time.deltaTime / a;
                if (jumpTimer > 0.1)
                {
                    jumping = false;
                    jumpTimer = 0;
                }
            }
            if (up && !jumping)
            {
                velocity.y = jumpPower;
                OnGround = false;
                jumping = true;
            }
        }else
        {
            Vector3 vel = velocity;
            Vector3 newVel = q * move * MoveSpeed * (run ? 2.5f : 1);
            newVel.y = vel.y;
            velocity.x += (newVel.x - velocity.x) * timer/8;
            velocity.z += (newVel.z - velocity.z) * timer/8;
        }
    }

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

    private void FixedUpdate()
    {
    }


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
