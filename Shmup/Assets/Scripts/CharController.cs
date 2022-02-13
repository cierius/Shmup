using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharController : MonoBehaviour
{ 
    private Stats stats;

    private Vector2 lastMoveDir = Vector2.zero;
    private bool isRolling = false;
    private float rollTime;
    private float maxRollTime = .1f;
    private float rollAmount = 10f;
    private bool rollOnCooldown = false;
    private float rollCooldown = 3f; // Time in seconds
    private float rollCooldownTimer;

    // Shooting variables
    private bool isFiring = false;
    private Vector2 fireDir = Vector2.up;
    private Vector2 lastFireDir;
    private float fireRate = 0.2f; //time in sec
    private float timeSinceLastFire = 0f;

    // References to character components
    private Rigidbody2D rb;
    private Transform trans;
    private PlayerInputActions playerInputActions; // the script that contains the input information
    private PlayerInput playerInput;

    // Refs of laser
    [SerializeField] private GameObject laser;
    private LineRenderer laserLineRenderer;
    private Vector3[] laserPos = new Vector3[2];


    [SerializeField]
    private GameObject bulletPrefab;


    private void OnEnable()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
    }


    private void OnDisable()
    {
        playerInputActions.Player.Disable();
    }


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        trans = GetComponent<Transform>();
        playerInput = GetComponent<PlayerInput>();
        laserLineRenderer = laser.GetComponent<LineRenderer>();
        stats = new Stats();
        stats.healthTotal = stats.healthBase + stats.healthAdditive;
    }


    private void Update()
    {
        if(rollOnCooldown)
        {
            rollCooldownTimer += Time.deltaTime;
            if(rollCooldownTimer >= rollCooldown)
            {
                rollOnCooldown = false;
                rollCooldownTimer = 0f;
            }
        }

        if(Keyboard.current.f1Key.wasPressedThisFrame) // Current key to switch between keyboard and controller input
        {
            Singleton.Instance.SwitchControlScheme();
        }

        if(Singleton.Instance.isUsingController)
        {
            fireDir = playerInputActions.Player.FireDir.ReadValue<Vector2>(); // Read joystick on controller
        }
        else // When using mouse + keyboard
        {
            CheckMouseInput();

            // Gets normalized mouse position on screen relative to the position of the middle
            // bottom left = (-1, -1)
            // top right = (1, 1)
            fireDir = Input.mousePosition - new Vector3(Camera.main.scaledPixelWidth/2, Camera.main.scaledPixelHeight/2, 0); 
            fireDir.Normalize();
        }

        timeSinceLastFire += Time.deltaTime;

        if(isFiring)
        {
            if(fireDir == Vector2.zero)
            {
                Fire(lastFireDir);
            }
            else
            {
                Fire(fireDir);
            }
        }

        // Laser only working for controller. Really only needed for controller aiming
        if (Singleton.Instance.isUsingController)
        {
            Vector3 laserEnd = new Vector3((playerInputActions.Player.FireDir.ReadValue<Vector2>().x*20f) + trans.position.x, (playerInputActions.Player.FireDir.ReadValue<Vector2>().y*20f) + trans.position.y);
            laserLineRenderer.SetPositions(new Vector3[] { trans.position, laserEnd });
        }
        /*else
        {
            laserPos[0] = trans.position;
            laserPos[1] = Input.mousePosition - new Vector3(Camera.main.scaledPixelWidth / 2, Camera.main.scaledPixelHeight / 2, 0);
            float dist = Vector3.Distance(laserPos[0], laserPos[1]);

            laserLineRenderer.SetPositions(laserPos);
        }*/
    }


    private void FixedUpdate()
    {
        if(stats.isMoveable && !isRolling)
        {
            Movement();
        }
        else if(isRolling)
        {
            rollTime += Time.deltaTime;
            if(rollTime >= maxRollTime)
            {
                isRolling = false;
                rollTime = 0;
            }
        }
    }


    public void CheckMouseInput() // Checks for left mouse button press
    {
        if(Mouse.current.leftButton.wasPressedThisFrame)
        {
            isFiring = true;
        }
        else if(Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isFiring = false;
        }
    }


    private void Fire(Vector2 dir)
    {
        if(timeSinceLastFire >= fireRate && !isRolling)
        {
            var bulletInstance = Instantiate(bulletPrefab, trans);
            bulletInstance.GetComponent<Rigidbody2D>().AddRelativeForce(dir * 10f, ForceMode2D.Impulse);
            timeSinceLastFire = 0f;
        }  
    }


    public void Movement()
    { 
        Vector2 inputVector = playerInputActions.Player.Movement.ReadValue<Vector2>();
        lastMoveDir = inputVector;
        if(isFiring) // Move at half speed when shooting
        {
            rb.velocity = new Vector2(inputVector.x, inputVector.y) * (stats.speedBase + stats.speedAdditive) * 5f * Time.deltaTime;
        }
        else
            rb.velocity = new Vector2(inputVector.x, inputVector.y) * (stats.speedBase + stats.speedAdditive) * 10f * Time.deltaTime;
    }


    public void OnFire(InputAction.CallbackContext context) // Fire input for controller, mouse input not used by PlayerInput Component
    {
        if (context.started == true) // on button down
        {
            isFiring = true;
            print("Start Firing");
        }
        else if (context.canceled == true) // on button release
        {
            isFiring = false;

            if (playerInputActions.Player.FireDir.ReadValue<Vector2>() != Vector2.zero) // Save the last dir if not zero
            {
                lastFireDir = playerInputActions.Player.FireDir.ReadValue<Vector2>();
            }
            print("Stop Firing");
        }
    }


    public void Action(InputAction.CallbackContext context)
    {
        print(context);
    }


    public void Roll(InputAction.CallbackContext context)
    {
        if(context.started == true && !isRolling && !rollOnCooldown)
        {
            isRolling = true;
            rollOnCooldown = true;
            rb.AddForce(lastMoveDir * rollAmount, ForceMode2D.Impulse);
        }
    }

    public void ReceiveDamage(int damage)
    {
        stats.healthTotal -= damage;
        print(stats.healthTotal);
    }
}


// Class for containing all of the character stats
public class Stats
{
    public bool isMoveable = true;
    public float speedBase = 15f;
    public float speedAdditive = 0f; // Used for speed boosts / bonus speed upgrades

    public int healthBase = 100;
    public int healthAdditive = 0;
    public int healthTotal;

    public List<Weapon> weapons = new List<Weapon>();
    public Weapon weaponEquipped;
    public int[] ammo = new int[]{0, 0, 0}; // Currently 3 ammo types
}


// Need scriptable objects for this
public class Weapon 
{
    enum weaponType
    {
        Rifle,
        Launcher,

    }
}