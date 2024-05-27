using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLocomotion : MonoBehaviour
{
    // Components
    private CharacterController controller;
    private Camera playerCamera;

    // Locomotion
    [SerializeField] private float walkSpeed = 10f;
    [SerializeField] private float jumpHeight = 7f;
    [SerializeField] private float doubleJumpHeight = 4f;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashTime = 0.1f;
    private float gravity = -9.81f;
    private Vector2 movementInput;
    private Vector3 currentVelocity;
    private Vector3 dashVelocity;
    [SerializeField] private bool isOnGround;
    [SerializeField] private bool canDoubleJump;
    [SerializeField] private bool canDash = true;
    private bool isDashing;
    [SerializeField] private bool isCrouching;
    private Coroutine crouchCoroutine;

    // Camera
    [SerializeField] private float lookSensitivity = 0.2f;
    [SerializeField] private float verticalLookLimit = 75;
    private Vector2 lookInput;
    private float xRotation;
    private float yRotation;

    void Start()
    {
        // Make the cursor locked and invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Get components from the player
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        // Reset vertical velocity if grounded
        if (isOnGround && currentVelocity.y < 0)
            currentVelocity.y = -2;

        // Check if the player is grounded
        isOnGround = controller.isGrounded;
        if (isOnGround)
            canDoubleJump = true;

        // Move character based on the controller input
        if (!isDashing)
            controller.Move((transform.forward * movementInput.y + transform.right * movementInput.x) * walkSpeed * Time.deltaTime);

        // Apply gravity to current velocity
        currentVelocity.y += gravity * Time.deltaTime;
        // Apply gravity to player character
        controller.Move((currentVelocity + dashVelocity) * Time.deltaTime);
    }

    public void OnLook(InputValue input)
    {
        // Get mouse delta scaled by vector
        lookInput = input.Get<Vector2>() * lookSensitivity;

        // Use Y input for camera X rotation and clamp camera X angle
        xRotation += lookInput.y;
        xRotation = Mathf.Clamp(xRotation, -verticalLookLimit, verticalLookLimit);

        // Use X input for player Y rotation
        yRotation += lookInput.x;

        // Rotate camera on X axis and rotate player on Y axis
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    public void OnMove(InputValue input)
    {
        movementInput = input.Get<Vector2>();
    }

    public void OnJump()
    {
        if (!isDashing)
        {
            if (isOnGround)
                currentVelocity.y += jumpHeight;
            else if (!isOnGround && canDoubleJump)
            {
                currentVelocity.y += doubleJumpHeight;
                canDoubleJump = false;
            }
        }
    }

    public void OnDash()
    {
        if (canDash)
            StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine()
    {
        canDash = false; // Prevent further dashes during the dash duration
        isDashing = true;

        // Find the forward direction and set the dash velocity
        Vector3 dashDirection = playerCamera.transform.forward * dashSpeed / dashTime; // Calculate dash speed to cover dashDistance in 0.2 seconds
        dashVelocity = dashDirection;

        yield return new WaitForSeconds(dashTime); // Dash duration

        // Reset the dash velocity after the dash duration
        dashVelocity = Vector3.zero;

        canDash = true; // Allow dashing again
        isDashing = false;
    }

    public void OnCrouch()
    {
        if (isOnGround)
        {
            isCrouching = !isCrouching;

            if (crouchCoroutine != null)
                StopCoroutine(crouchCoroutine);

            crouchCoroutine = StartCoroutine(CrouchCoroutine(isCrouching ? 1f : 2f));
        }
    }

    private IEnumerator CrouchCoroutine(float targetHeight)
    {
        float currentHeight = controller.height;
        float timeElapsed = 0f;
        float duration = 0.2f;

        while (timeElapsed < duration)
        {
            controller.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        controller.height = targetHeight;
    }
}