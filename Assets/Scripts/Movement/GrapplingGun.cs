using System;
using UnityEngine;

public class GrapplingGun : MonoBehaviour {
    public LayerMask whatIsGrappable;    // LayerMask to specify the grappable objects
    public Transform gunTip, player;  // References to gun tip, and player transform
    public new Transform camera;         // Reference to the camera transform
    public float maxDistance = 100f;     // Maximum distance the grapple can travel (adjustable)
        
    private LineRenderer _lr;              // Reference to the LineRenderer for drawing the rope
    private SpringJoint _joint;            // SpringJoint used for connecting the player to the grapple point
    private Vector3 _grapplePoint;         // The point where the grapple hook attaches
    

    void Awake()
    {
        // Initialize the LineRenderer component
        _lr = GetComponent<LineRenderer>();
    }

    void Update()
    {
        // Detect when the middle mouse button is pressed (mouse button 2)
        if (Input.GetKeyDown(KeyCode.Q)) // Middle mouse button pressed
        {
            StartGrapple();
        }
        // Detect when the middle mouse button is released
        else if (Input.GetKeyUp(KeyCode.Q)) // Middle mouse button released
        {
            StopGrapple();
        }
    }

    /// <summary>
    /// This is called after the Update method to render the rope.
    /// </summary>
    void LateUpdate()
    {
        DrawRope();
    }

    /// <summary>
    /// Initiates the grapple action.
    /// Called when the player presses the middle mouse button.
    /// </summary>
    void StartGrapple()
    {
        RaycastHit hit; // To store information about the hit object
        // Perform a raycast from the camera to check if we hit a grappable object
        if (Physics.Raycast(camera.position, camera.forward, out hit, maxDistance, whatIsGrappable))
        {
            // Store the point where the raycast hit
            _grapplePoint = hit.point;

            // Add a SpringJoint component to the player to attach them to the grapple point
            _joint = player.gameObject.AddComponent<SpringJoint>();
            _joint.autoConfigureConnectedAnchor = false;   // We will manually set the connected anchor
            _joint.connectedAnchor = _grapplePoint;        // The point where the joint connects (grapple point)

            // Calculate the distance between the player and the grapple point
            float distanceFromPoint = Vector3.Distance(player.position, _grapplePoint);
            
            // Set the maximum and minimum distance that the SpringJoint should maintain
            _joint.maxDistance = distanceFromPoint * 0.8f;  // The distance the grapple will try to maintain
            _joint.minDistance = distanceFromPoint * 0.25f; // The minimum distance the grapple can go

            // Set the SpringJoint properties (spring, damper, and mass scale) to control the grapple's behavior
            _joint.spring = 4.5f; // How stiff the spring is
            _joint.damper = 7f;   // How much damping the spring has (slows down the motion)
            _joint.massScale = 4.5f; // How much the player’s mass influences the spring’s behavior

            // Enable the LineRenderer by setting the number of points it will draw (from the gun tip to the grapple point)
            _lr.positionCount = 2;
        }
    }

    /// <summary>
    /// Stops the grapple action.
    /// Called when the player releases the middle mouse button.
    /// </summary>
    public void StopGrapple()
    {
        // Disable the LineRenderer by setting position count to 0
        _lr.positionCount = 0;
        // Destroy the SpringJoint to detach the player from the grapple point
        if (_joint != null)
        {
            Destroy(_joint);
            _joint = null;
        }
    }

    /// <summary>
    /// Draws the rope between the player and the grapple point using LineRenderer.
    /// This method is called every frame to update the rope's positions.
    /// </summary>
    void DrawRope()
    {
        // If no joint is active (i.e., not grappling), do not draw the rope
        if (!_joint) return;
        
        // Set the positions for the LineRenderer (from the gun tip to the grapple point)
        _lr.SetPosition(0, gunTip.position);      // Set the starting point of the rope (gun tip position)
        _lr.SetPosition(1, _grapplePoint);         // Set the end point of the rope (grapple point)
    }

    /// <summary>
    /// Returns whether the player is currently grappling.
    /// </summary>
    public bool IsGrappling()
    {
        // If the SpringJoint is not null, the player is grappling
        return _joint != null;
    }

    /// <summary>
    /// Returns the current grapple point where the player is attached.
    /// </summary>
    public Vector3 GetGrapplePoint()
    {
        return _grapplePoint;
    }

    private void OnDisable()
    {
        StopGrapple();
    }
}

