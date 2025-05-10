using UnityEngine;
using System;



public class PlayerMovement : MonoBehaviour {

    [Header("References")]
    public Transform playerCam;    // Reference to the player's camera
    public Transform orientation;   // Reference to the object that determines the player's direction
    private Rigidbody _rb;         // Player's rigidbody component
    private CapsuleCollider _cc;   // Player's capsule collider'
    private WallRunning _wallRunning;
    
 

    [Header("Movement Settings")]
    public float moveSpeed = 4500; // Base movement speed (high value because of force-based movement)
    public float maxSpeed = 20;  // Maximum velocity the player can reach
    public float counterMovement = 0.175f;  // How quickly the player stops when no input is given
    public float maxSlopeAngle = 35f;      // Maximum angle the player can walk on

    [Header("Wallrun settings")]
    public float wallrunSpeed = 1000; // Base movement speed (high value because of force-based movement)
    
    [Header("Jump Settings")]
    private bool _readyToJump = true;        // Whether the player can currently jump
    private float _jumpCooldown = 0.25f;     // Time between jumps
    public float jumpForce = 550f;         // Force applied when jumping

    [Header("Crouch & Slide Settings")]
    //unused but saved incase I switch back to old method
    //private Vector3 _crouchScale = new Vector3(1, 0.5f, 1); // Scale when crouching 
    private Vector3 _playerScale; // Player's normal scale
   
    public float slideForce = 400; // Force applied when sliding
    public float slideCounterMovement = 0.2f; // Counter movement when sliding
    private Vector3 _normalVector = Vector3.up; // The normal of the floor for jumping
    private Vector3 _wallNormalVector; // Normal vector when colliding with walls
    private float _originalHeight;
    public float crouchHeight = 1.0f;

    [Header("Ground Check")]
    public bool grounded; // If the player is grounded
    public LayerMask whatIsGround; // Layer mask to identify the ground
    private float _threshold = 0.01f; // Threshold for movement calculations
   
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        jumping,
        sliding,
        air,
        wallrunning
    
    }
    public MovementState state;
    
    public bool wallrunning;
    
        
    // Input state variables
    float _x, _y; // Movement input on the X and Y axes
    bool _jumping, _sprinting, _crouching; // State variables for jump, sprint, crouch actions

    /// <summary>
    /// Initialize components and setup
    /// </summary>
    void Awake() {
        _rb = GetComponent<Rigidbody>(); // Get the player's rigidbody component
        _wallRunning = GetComponent<WallRunning>();
    }

    /// <summary>
    /// Initialize player settings
    /// </summary>
    void Start() {
        _playerScale =  transform.localScale; // Store the player's normal scale
        Cursor.lockState = CursorLockMode.Locked; // Lock the mouse cursor
        Cursor.visible = false; // Make the cursor invisible
        _cc = GetComponent<CapsuleCollider>();
        _originalHeight = _cc.height;
    }

    /// <summary>
    /// Perform movement actions during fixed update
    /// </summary>
    private void FixedUpdate() {
        Movement(); // Perform movement-related actions
    }

    /// <summary>
    /// Handle player input and camera rotation
    /// </summary>
    private void Update() {
        MyInput(); // Get player input for movement and actions
       
    }

    /// <summary>
    /// Get player input for movement and actions like jumping and crouching
    /// </summary>
    private void MyInput() {
        _x = Input.GetAxisRaw("Horizontal"); // Get horizontal movement input (A/D or Left/Right Arrow)
        _y = Input.GetAxisRaw("Vertical"); // Get vertical movement input (W/S or Up/Down Arrow)
        _jumping = Input.GetButton("Jump"); // Check if the player pressed jump
        _crouching = Input.GetKey(KeyCode.LeftControl); // Check if the player is crouching

        // Crouching actions
        if (Input.GetKeyDown(KeyCode.LeftControl))
            StartCrouch(); // Start crouching
        if (Input.GetKeyUp(KeyCode.LeftControl))
            StopCrouch(); // Stop crouching
    }

    /// <summary>
    /// Start the crouch action and adjust player's scale and position
    /// </summary>
    private void StartCrouch() {
        _cc.height = crouchHeight; // Change player's scale to crouch
        playerCam.localPosition = new Vector3(playerCam.localPosition.x, playerCam.localPosition.y - 0.5f, playerCam.localPosition.z); // Lower player's position
        if (_rb.linearVelocity.magnitude > 0.5f) {
            if (grounded) {
                _rb.AddForce(orientation.transform.forward * slideForce); // Apply force when sliding
            }
        }
    }

    /// <summary>
    /// Stop the crouch action and reset player's scale and position
    /// </summary>
    private void StopCrouch() {
        _cc.height = _originalHeight; // Reset player's scale to normal
        playerCam.localPosition = new Vector3(playerCam.localPosition.x, playerCam.localPosition.y + 0.5f, playerCam.localPosition.z);
    }

    /// <summary>
    /// Handle the player's movement
    /// </summary>
    private void Movement() {
        // Extra gravity for a more grounded feel
        _rb.AddForce(Vector3.down * (Time.deltaTime * 10));
        
        // Find actual velocity relative to where the player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        // Counteract sliding and sloppy movement
        CounterMovement(_x, _y, mag);

        // Jump if ready
        if (_readyToJump && _jumping) Jump();

        // Set maximum speed
        float maxSpeed = this.maxSpeed;

        // Apply forces when sliding down a ramp while crouching
        if (_crouching && grounded && _readyToJump) {
            _rb.AddForce(Vector3.down * (Time.deltaTime * 3000));
            return;
        }

        // Prevent movement from exceeding maximum speed
        if (_x > 0 && xMag > maxSpeed) _x = 0;
        if (_x < 0 && xMag < -maxSpeed) _x = 0;
        if (_y > 0 && yMag > maxSpeed) _y = 0;
        if (_y < 0 && yMag < -maxSpeed) _y = 0;

        // Movement multipliers
        float multiplier = 1f, multiplierV = 1f;

        // Movement in air has a reduced multiplier
        if (!grounded) {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }

        // Movement while sliding is disabled
        if (grounded && _crouching) multiplierV = 0f;

        // Apply forces to move the player in the desired direction
        _rb.AddForce(orientation.transform.forward * (_y * moveSpeed * Time.deltaTime * multiplier * multiplierV));
        _rb.AddForce(orientation.transform.right * (_x * moveSpeed * Time.deltaTime * multiplier));
    }

    /// <summary>
    /// Handle jumping behavior
    /// </summary>
    private void Jump() {
        if (grounded && _readyToJump) {
            _readyToJump = false;

            // Add jump forces
            _rb.AddForce(Vector2.up * (jumpForce * 1.5f));
            _rb.AddForce(_normalVector * (jumpForce * 0.5f));

            // Adjust Y velocity to prevent excessive falling during a jump
            Vector3 vel = _rb.linearVelocity;
            if (_rb.linearVelocity.y < 0.5f)
                _rb.linearVelocity = new Vector3(vel.x, 0, vel.z);
            else if (_rb.linearVelocity.y > 0) 
                _rb.linearVelocity = new Vector3(vel.x, vel.y / 2, vel.z);

            // Reset jump readiness after cooldown
            Invoke(nameof(ResetJump), _jumpCooldown);
        }
    }
    
    /// <summary>
    /// Reset jump ability after the cooldown period
    /// </summary>
    private void ResetJump() {
        _readyToJump = true;
    }

    private float _desiredX;



    /// <summary>
    /// Counteract player movement to prevent sliding and unwanted behavior
    /// </summary>
    private void CounterMovement(float x, float y, Vector2 mag) {
        if (!grounded || _jumping) return;

        // Slow down sliding when crouching
        if (_crouching) {
            _rb.AddForce(-_rb.linearVelocity.normalized * (moveSpeed * Time.deltaTime * slideCounterMovement));
            return;
        }

        // Counter-movement to stop sliding
        if (Math.Abs(mag.x) > _threshold && Math.Abs(x) < 0.05f || (mag.x < -_threshold && x > 0) || (mag.x > _threshold && x < 0)) {
            _rb.AddForce(orientation.transform.right * (moveSpeed * Time.deltaTime * -mag.x * counterMovement));
        }
        if (Math.Abs(mag.y) > _threshold && Math.Abs(y) < 0.05f || (mag.y < -_threshold && y > 0) || (mag.y > _threshold && y < 0)) {
            _rb.AddForce(orientation.transform.forward * (moveSpeed * Time.deltaTime * -mag.y * counterMovement));
        }

        // Limit diagonal movement
        if (Mathf.Sqrt((Mathf.Pow(_rb.linearVelocity.x, 2) + Mathf.Pow(_rb.linearVelocity.z, 2))) > maxSpeed) {
            float fallspeed = _rb.linearVelocity.y;
            Vector3 n = _rb.linearVelocity.normalized * maxSpeed;
            _rb.linearVelocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    /// <summary>
    /// Calculate the velocity relative to where the player is looking
    /// </summary>
    /// <returns>Vector2 with X and Y magnitudes</returns>
    public Vector2 FindVelRelativeToLook() {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(_rb.linearVelocity.x, _rb.linearVelocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = _rb.linearVelocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    /// <summary>
    /// Determine if the given surface is walkable based on slope angle
    /// </summary>
    /// <returns>True if the angle is walkable, false otherwise</returns>
    private bool IsFloor(Vector3 v) {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool _cancellingGrounded;

    /// <summary>
    /// Detect if the player is grounded based on collision normals
    /// </summary>
    private void OnCollisionStay(Collision other) {
        // Check for walkable layers
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        // Iterate through each contact point
        for (int i = 0; i < other.contactCount; i++) {
            Vector3 normal = other.contacts[i].normal;
            // Check if the surface is walkable
            if (IsFloor(normal)) {
                grounded = true;
                _cancellingGrounded = false;
                _normalVector = normal;
                CancelInvoke(nameof(StopGrounded)); // Cancel stop grounded invocation
            }
        }

        // Schedule grounded state stop if no floor is detected
        float delay = 3f;
        if (!_cancellingGrounded) {
            _cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    /// <summary>
    /// Stop grounded state after a delay
    /// </summary>
    private void StopGrounded() {
        grounded = false;
    }
    
    private void StateHandler() {
        // Wallrunning overrides all other states
        if (wallrunning) {
            state = MovementState.wallrunning;
            moveSpeed = wallrunSpeed;
        }
       
    }
    
    
}