using System.Collections;
using System.Collections.Generic;
using UnityEngine.Animations.Rigging;
using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    //references
    private CharacterController characterController;
    private Animator animator;
    [SerializeField] private GameObject cameraM;
    [SerializeField] private GameObject groundRay;
    [SerializeField] private GameObject FloorChecker;
    [SerializeField] private GameObject leftScaleRay;
    [SerializeField] private GameObject rightScaleRay;

    [SerializeField] private GameObject LeftArmTarget;
    [SerializeField] private GameObject LeftArmRig;
    [SerializeField] private GameObject RightArmTarget;
    [SerializeField] private GameObject RightArmRig;
    [SerializeField] private GameObject CeilingRay;

    [SerializeField] private HandsManagerScript HandsManager;
    [SerializeField] private CeilingHandlerScript CeilingHandler;
    [SerializeField] private LayerMask playerLayer;

    //variables
    [SerializeField] private float walkingSpeed;
    [SerializeField] private float runningSpeed;
    [SerializeField] private float crouchingSpeed;
    [SerializeField] private float climbingSpeed;
    [SerializeField] private float gravity;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float wallJumpSpeed;
    [SerializeField] private float actualSpeed;
    [SerializeField] private bool isJumping;
    [SerializeField] private float maxTimeBtwnJumps;
    [SerializeField] private float maxInertia;
    [SerializeField] private float rollTime;
    private Vector3 leftPrevPosition;
    private Vector3 rightPrevPosition;
    [SerializeField] private bool isGrounded => inGround();
    private float timeBtwnJumps;
    [SerializeField] private bool isGrabbed;
    [SerializeField] private bool canGrab;
    [SerializeField] private bool isWallJumping;
    [SerializeField] private float timeBtwnGrabs;
    [SerializeField] private float timeBtwnRolls;
    [SerializeField] private float btwnRollsTimer;
    private float standingHeight;
    private float yCharacterCenter;
    private bool isWallRunning;
    private float momentum = 5f;
    private float horizontal;
    private float vertical;
    private Queue<KeyCode> inputBuffer;
    private Vector3 moveVector;
    private float turnSmoothVelocity = 1f;
    [SerializeField] private float yVelocity;
    private bool isRunning;
    private bool isFalling;
    private readonly float turnSmoothTime = 0.1f;
    private float maxHeight;
    private bool isCrouching;
    
    [SerializeField]private PlayerState playerState;
    public enum PlayerState 
    {   Walking,
        Running,
        Grabbed,
        WallJumping,
        Idle, 
        Jumping,
        GrabLeaved,
        Rolling,
        Crouching
    }
    public PlayerState GetPlayerState()
    {
        return playerState;
    }
    private void Awake()
    {
        
        inputBuffer = new Queue<KeyCode>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        moveVector = new Vector3();
        actualSpeed = walkingSpeed;
        timeBtwnJumps = maxTimeBtwnJumps;
        leftPrevPosition = new Vector3();
        rightPrevPosition = new Vector3();
        LeftArmRig.GetComponent<Rig>().weight = 0;
        RightArmRig.GetComponent<Rig>().weight = 0;
        playerState = PlayerState.Idle;
        canGrab = true;
        standingHeight = characterController.height;
        yCharacterCenter = characterController.center.y;
        timeBtwnGrabs = 0;
        btwnRollsTimer = timeBtwnRolls;
    }
    private void AumentarMomentum()
    {
        momentum *= 1.001f;
    }
    private void AumentarMomentumRun()
    {
        momentum *= 1.005f;
    }
    private void DisminuirMomentum()
    {
        momentum *= 0.995f;
    }
    private void Roll()
    {
        btwnRollsTimer = 0;
        playerState = PlayerState.Rolling;
        characterController.center /= 2;
        characterController.height /= 2;
        animator.SetBool("isRolling", true);
        
    }
    private Vector2 CalculateIntertia(Vector2 movement)
    {
        if (playerState.Equals(PlayerState.Rolling))
        {
            if (momentum < runningSpeed * 1.5f)
            {
                momentum = runningSpeed * 1.5f;
            }
            if (momentum > runningSpeed)
            {
                DisminuirMomentum();
            }
        }
        else if (playerState.Equals(PlayerState.Crouching))
        {
            if(momentum < crouchingSpeed)
            {
                AumentarMomentum();
            }
            if(momentum >= crouchingSpeed)
            {
                momentum = crouchingSpeed;
            }
        }
        else if (playerState.Equals(PlayerState.Jumping) && (moveVector.x != 0 || moveVector.z != 0))
        {
            if (momentum < maxInertia)
            {
                AumentarMomentum();
            }
            if(momentum >= maxInertia)
            {
                DisminuirMomentum();
            }
        }
        
        else if (playerState.Equals(PlayerState.Running))
        {
            if (momentum < runningSpeed)
            {
                AumentarMomentumRun();
            }
            if(momentum >= runningSpeed)
            {
                DisminuirMomentum();
            }
        }
        else if(!playerState.Equals(PlayerState.Running) && (moveVector.x != 0 || moveVector.z != 0) && !playerState.Equals(PlayerState.Grabbed))
        {
            if (momentum < walkingSpeed)
            {
                momentum = walkingSpeed;
            }
            if(momentum >= walkingSpeed)
            {
                DisminuirMomentum();
            }
        }
        else if (playerState.Equals(PlayerState.Grabbed))
        {
            if (momentum < climbingSpeed)
            {
                AumentarMomentum();
            }
            if (momentum >= climbingSpeed)
            {
                momentum = climbingSpeed;
            }
        }
        movement *= momentum;
        //if(isJumping && characterController.velocity.magnitude <= maxInertia)
        //{
        //    actualSpeed *= strafeMultiplier;
        //}
        //if(characterController.velocity.magnitude > maxInertia)
        //{
        //    actualSpeed *= (2 - strafeMultiplier);
        //}
        //if (inGround() && !isJumping)
        //{
        //    actualSpeed = walkingSpeed;
        //}

        return movement;
    }
    //public void SetState()
    //{
    //    if(playerInput.moveInput == Vector2.zero && !playerState.Equals(PlayerState.Jumping) && !playerState.Equals(PlayerState.Grabbed))
    //    {
    //        playerState = PlayerState.Idle;
    //    }
    //    if (playerInput.moveInput != Vector2.zero && !playerState.Equals(PlayerState.Jumping) && !playerState.Equals(PlayerState.Grabbed) && !playerInput.running)
    //    {
    //        playerState = PlayerState.Walking;
    //    }
    //    else if (playerInput.moveInput != Vector2.zero && !playerState.Equals(PlayerState.Jumping) && !playerState.Equals(PlayerState.Grabbed) && playerInput.running)
    //    {
    //        playerState = PlayerState.Running;
    //    }
    //    if (playerInput.crouching && actualSpeed > walkingSpeed && !playerState.Equals(PlayerState.Jumping) && rollTime <= 0.5f)
    //    {
    //        playerState = PlayerState.Rolling;
    //    }
    //}
    void Update()
    {
        Debug.Log(inGround());
        //inGround();
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        //SetState();
        //if (!isGrabbed)
        //{
        moveVector.x = horizontal;
            moveVector.z = vertical;
        //}
        CheckFloorHeight();

        if (!inGround() && !playerState.Equals(PlayerState.Grabbed) && !playerState.Equals(PlayerState.WallJumping) && !playerState.Equals(PlayerState.Rolling));
        if (inGround()) StrafeTimer();
        if(inGround() && !playerState.Equals(PlayerState.Rolling)) { btwnRollsTimer += Time.deltaTime; }

        if (playerState.Equals(PlayerState.Crouching) && !Input.GetKey(KeyCode.LeftControl) && btwnRollsTimer >1f)
        {
            if (!CheckCeiling())
            {
                playerState = PlayerState.Idle; animator.SetBool("isCrouching", false);
                characterController.height = standingHeight;
                characterController.center = new Vector3(characterController.center.x, yCharacterCenter, characterController.center.z);
            }
            else { playerState = PlayerState.Crouching; }
            
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            inputBuffer.Enqueue(KeyCode.Space);
            Invoke("CleanKey", 0.3f);
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            inputBuffer.Enqueue(KeyCode.LeftControl);
            Invoke("CleanKey", 0.3f);
        }

        if (!playerState.Equals(PlayerState.Grabbed))
        {
            timeBtwnGrabs += Time.deltaTime;
            
            if (inGround())
            {
                canGrab = true;
                if (inputBuffer.Count > 0)
                {
                    if(inputBuffer.Peek() == KeyCode.Space)
                    {
                        inputBuffer.Dequeue();
                        Jump();
                    }
                }

                if(playerState.Equals(PlayerState.Running))
                {
                    if (inputBuffer.Count > 0)
                    {
                        if (inputBuffer.Peek() == KeyCode.LeftControl && btwnRollsTimer >= timeBtwnRolls)
                        {
                            inputBuffer.Dequeue();
                            Roll();
                        }
                    }
                }

                
            }
            else
            {
                calculateGravity();
            }
            moveVector.y = yVelocity;
            Move(moveVector);
            HandleLedge();
            
            
        }
        


        //if (!isGrabbed)
        //{
        //    //WallrunChecker();
        //    if (inGround())
        //    {
        //        canGrab = true;
        //        if (inputBuffer.Count > 0)
        //        {
        //            if (inputBuffer.Peek() == KeyCode.Space)
        //            {
        //                inputBuffer.Dequeue();
        //                Jump();
        //                moveVector.y += jumpVelocity;
        //            }
        //        }

            //        //else if (isWallJumping)
            //        //{
            //        //    ScalableWallScript scalableWall = leftHit.transform.GetComponent<ScalableWallScript>();
            //        //    if (characterController.transform.position.y >= (jumpHeight+scalableWall.GetTopLimit()) && jumpVelocity > 0)
            //        //    {
            //        //        jumpVelocity = Mathf.Clamp(jumpVelocity - 20f, 0, Mathf.Infinity);
            //        //    }
            //        //}

            //    }
            //    //else if (isJumping && characterController.transform.position.y >= jumpHeight && !isWallJumping)
            //    //{
            //    //    jumpVelocity = 0;
            //    //    moveVector.y -= 2f;
            //    //}
            //    //else if (isWallRunning)
            //    //{
            //    //    WallrunGravity();
            //    //}

            //    HandleLedge();
            //    Move(moveVector);


            //}
        else if (playerState.Equals(PlayerState.Grabbed))
        {
            
            if (inputBuffer.Count > 0)
            {
                if (inputBuffer.Peek() == KeyCode.Space)
                {
                    
                    inputBuffer.Dequeue();
                    GrabWallJump();
                    moveVector.y += yVelocity;
                    Move(moveVector);
                    Invoke("EnableHandsCheckers", 1f);
                    //leftScaleRay.SetActive(true);
                    //rightScaleRay.SetActive(true);
                    canGrab = true;
                }
                
            }
            else
            {
                moveVector.z = 0;
                ClimbMove(moveVector);
            }
            HandleGrab();

        }
        if (playerState.Equals(PlayerState.Rolling))
        {
            RollTimer();
            if (rollTime > 0.5)
            {
                StopRoll();
            }
        }






    }
    private void StopRoll()
    {
        if (!CheckCeiling())
        {
            rollTime = 0;
            playerState = PlayerState.Walking;
            characterController.center *= 2;
            characterController.height *= 2;
            animator.SetBool("isRolling", false);
        }
        else
        {
            rollTime = 0;
            playerState = PlayerState.Crouching;
            animator.SetBool("isRolling", false);
            animator.SetBool("isCrouching", true);
        }
        
    }
    //private void WallrunChecker()
    //{
    //    Ray leftWallrunRay = new Ray(LeftWallrunRay.transform.position, LeftWallrunRay.transform.forward);
    //    Ray RightWallrunRay = new Ray(LeftWallrunRay.transform.position, LeftWallrunRay.transform.forward);

    //    if(Physics.Raycast(leftWallrunRay, out RaycastHit leftHit, 1f))
    //    {
    //        if (leftHit.collider.CompareTag("RunneableWall"))
    //        {
    //            isWallRunning = true;
    //            Vector3 partial = LeftWallrunRay.transform.position - characterController.transform.position;
    //            //Vector3 movement = leftHit.transform.TransformDirection(partial);
    //            characterController.Move(partial * 5f);
    //        }
    //    }
    //}
    public float GetYRotation()
    {
        return characterController.transform.rotation.y;
    }
    //private void CalculateInertia() 
    //{
    //    if (isJumping)
    //    {
    //        strafeMultiplier += 0.002f;
    //    }
    //    else
    //    {
    //        strafeMultiplier = initialMultiplier;
    //    }
    //}
    private void CleanKey()
    {
        if (inputBuffer.Count > 0)
        {
            inputBuffer.Dequeue();
        }
    }
    private void GrabWallJump()
    {
        LeaveWall();
        animator.Play("Running Jump");
        playerState = PlayerState.WallJumping;
        isWallJumping = true;
        yVelocity = wallJumpSpeed;
    }
    private void Jump()
    {


        animator.Play("Jump");
        playerState = PlayerState.Jumping;
        isJumping = true;
        yVelocity = 0;
        yVelocity = jumpSpeed;
        
    }
    void Move(Vector3 direction)
    {
        
        
        SetSpeed();

        Vector3 camAngle = cameraM.transform.eulerAngles;
        direction = Quaternion.Euler(0, camAngle.y, 0) * direction;

        Vector2 moveWithInertia = CalculateIntertia(new Vector2(direction.x, direction.z));

        characterController.Move(new Vector3(moveWithInertia.x * Time.deltaTime , yVelocity * Time.deltaTime, moveWithInertia.y * Time.deltaTime));

        if(direction.x != 0 || direction.z != 0)
        {
            RotateToFront(direction);
        }
            

        Animate(new Vector3(direction.x, 0, direction.z));
        actualSpeed = momentum;
    }
    void ClimbMove(Vector3 direction)
    {
        SetSpeed();
        ScalableWallScript scalableWall = HandsManager.GetWallScript();
        yVelocity = 0;
        //transform.rotation = Quaternion.Euler(0, scalableWall.transform.forward.y,0);
        direction = Quaternion.Euler(0,scalableWall.transform.eulerAngles.y,0) * direction;
        Vector2 moveWithInertia = CalculateIntertia(new Vector2(direction.x, direction.z));

        leftPrevPosition += new Vector3(moveWithInertia.x * Time.deltaTime, 0, 0);
        rightPrevPosition += new Vector3(moveWithInertia.x * Time.deltaTime, 0, 0);
        
        characterController.Move(new Vector3(moveWithInertia.x * Time.deltaTime, yVelocity * Time.deltaTime, moveWithInertia.y * Time.deltaTime));
        //RotateToFront(direction);
        Animate(new Vector3(direction.x, 0, direction.z));
        actualSpeed = momentum;
    }

    private void SetSpeed()
    {
        if (!playerState.Equals(PlayerState.Rolling))
        {
            if (Input.GetKey(KeyCode.LeftShift) && (moveVector.x != 0 || moveVector.z != 0) && !playerState.Equals(PlayerState.Grabbed) && !playerState.Equals(PlayerState.Jumping) && !playerState.Equals(PlayerState.Rolling) && !playerState.Equals(PlayerState.Crouching)) playerState = PlayerState.Running;
            else if (!Input.GetKey(KeyCode.LeftShift) && (moveVector.x != 0 || moveVector.z != 0) && !playerState.Equals(PlayerState.Grabbed) && !playerState.Equals(PlayerState.Jumping) && !playerState.Equals(PlayerState.Rolling) && !playerState.Equals(PlayerState.Crouching)) playerState = PlayerState.Walking;
            if ((Input.GetKey(KeyCode.LeftControl) && !playerState.Equals(PlayerState.Grabbed) && !playerState.Equals(PlayerState.Jumping) && !playerState.Equals(PlayerState.Running) && btwnRollsTimer >= timeBtwnRolls)) 
            {
                playerState = PlayerState.Crouching;
                characterController.height = standingHeight * 0.5f;
                characterController.center = new Vector3(characterController.center.x, yCharacterCenter * 0.5f, characterController.center.z);
            } 
        }


    }
    private void Crouch() 
    {
        playerState = PlayerState.Crouching;
        
    }
    private void UnCrouch()
    {
        if (!characterController.collisionFlags.Equals(CollisionFlags.CollidedAbove))
        {

            characterController.height *= (4 / 3);
            characterController.center *= (4 / 3);
        }
    }
    private void Animate(Vector3 direction)
    {
        if (direction == Vector3.zero)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
            animator.SetFloat("Forward", 0f, .1f, Time.deltaTime);
        }
        
        if (direction.magnitude >= 0.01f && !Input.GetKey(KeyCode.LeftShift))
        {
            animator.SetBool("isWalking", true);
            animator.SetBool("isRunning", false);
            animator.SetFloat("Forward", 0.5f, .1f, Time.deltaTime);
            animator.speed = actualSpeed / walkingSpeed;
        }
        if (direction.magnitude >= 0.01f && Input.GetKey(KeyCode.LeftShift) && actualSpeed >= 4f)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", true);
            animator.SetFloat("Forward", 1f, .1f, Time.deltaTime);
            animator.speed = actualSpeed / runningSpeed;
        }
        else if (direction.magnitude >= 0.01f && Input.GetKey(KeyCode.LeftShift) && actualSpeed < 4f)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", true);
            animator.SetFloat("Forward", 0.5f, .1f, Time.deltaTime);
            animator.speed = actualSpeed / walkingSpeed;
        }
        if (playerState.Equals(PlayerState.Crouching) && direction.magnitude >= 0.01f)
        {
            animator.SetBool("isCrouching", true);
            animator.SetBool("isRunning", false);
            animator.SetFloat("Forward", 1f, .1f, Time.deltaTime);
        }
        else if (playerState.Equals(PlayerState.Crouching) && direction == Vector3.zero)
        {
            animator.SetBool("isCrouching", true);
            animator.SetBool("isRunning", false);
            animator.SetFloat("Forward", 0f, .1f, Time.deltaTime);
        }
    }
    private void RotateToFront(Vector3 moveDirection)
   {
        float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }
    void calculateGravity()
    {
        if (playerState.Equals(PlayerState.Grabbed))
        {
            yVelocity = 0f;

        }
        if (!inGround())
        {
            yVelocity -= gravity * Time.deltaTime;
        }
        //else if (!isGrabbed) 
        //{
        //    moveVector.y -= gravity * Time.deltaTime;
        //}
    }
    private bool CheckCeiling()
    {
        return CeilingHandler.GetAreCeiling();
    }
    private void CheckFloorHeight()
    {
        maxHeight = FloorChecker.GetComponent<FloorCheckerScript>().GetFloorHeight();
        Debug.Log(maxHeight);
    }
    public bool inGround()
    {
        bool grounded;
        Ray ray = new Ray(groundRay.transform.position, groundRay.transform.forward);
        var layer = 1 << 6;
        if(Physics.Raycast(ray, .1f, layer))
        {
            grounded = true;
            leftScaleRay.SetActive(true);
            rightScaleRay.SetActive(true);
            animator.SetBool("isJumping", false);
            if (timeBtwnJumps <= 0 && !playerState.Equals(PlayerState.Rolling) && !playerState.Equals(PlayerState.Crouching))
            {
                isJumping = false;
                isWallJumping = false;
                isWallRunning = false;
                playerState = PlayerState.Idle;
                timeBtwnJumps = maxTimeBtwnJumps;
                animator.SetBool("inGround", true);
            }
        }
        else
        {
            grounded = false;
            animator.SetBool("inGround", false);
        }
        return grounded;

    }
    private void StrafeTimer()
    {
        timeBtwnJumps -= Time.deltaTime;
    }
    private void RollTimer()
    {
        rollTime += Time.deltaTime;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(leftScaleRay.transform.position,.1f);
        Gizmos.DrawSphere(rightScaleRay.transform.position, .1f);
        Gizmos.DrawSphere(groundRay.transform.position, 0.1f);

    }

    public void WallrunGravity()
    {
        moveVector.y /= (2 / 3);
    }
    private void HandleLedge()
    {

        
        if(HandsManager.GetLeftHand() && HandsManager.GetRightHand())
        {
            if(HandsManager.GetLeftHandPoint() != null && HandsManager.GetRightHandPoint() != null)
            {
                if(HandsManager.LeftHandChecker.transform.position.y >= HandsManager.GetWallScript().GetTopLimit()-0.03f && HandsManager.RightHandChecker.transform.position.y >= HandsManager.GetWallScript().GetTopLimit() - 0.03f)
                {
                    if(!playerState.Equals(PlayerState.Grabbed) && canGrab && timeBtwnGrabs >= .5f)
                    {
                        timeBtwnGrabs = 0;
                        GrabWall(HandsManager.GetLeftHandPoint(), HandsManager.GetRightHandPoint());
                    }
                }
            }
        }

        //if(Physics.SphereCast(leftRaycast , 0.1f ,out RaycastHit leftHitInfo, 1f) && Physics.SphereCast(rightRaycast, 0.1f ,out RaycastHit rightHitInfo, 1f))
        //{
        //    leftHit = leftHitInfo;
        //    rightHit = rightHitInfo;

           
        //    if(leftHit.collider.CompareTag("ScalableWall") && rightHit.collider.CompareTag("ScalableWall"))
        //    {

        //        ScalableWallScript scalableWall = leftHit.transform.GetComponent<ScalableWallScript>();
        //        if(leftHit.point.y >= scalableWall.GetTopLimit()-0.03f && rightHit.point.y >= scalableWall.GetTopLimit()-0.03f)
        //        {
        //            if (!isGrabbed && canGrab)
        //            {
        //                //transform.rotation = Quaternion.Euler(0, scalableWall.transform.forward.y, 0);
        //                GrabWall(leftHit.point, rightHit.point);    
        //            }
                    
        //        }
        //    }

        //}
    }
    private void EnableHandsCheckers()
    {
        HandsManager.LeftHandChecker.SetActive(true);
        HandsManager.RightHandChecker.SetActive(true);
    }
    private void GrabWall(Vector3 leftHandPos, Vector3 rightHandPos)
    {
        //transform.position = new Vector3(transform.position.x, transform.position.y-0.4f, transform.position.z);

        canGrab = false;
        LeftArmRig.GetComponent<Rig>().weight = 1;
        RightArmRig.GetComponent<Rig>().weight = 1;
        LeftArmTarget.transform.position = leftHandPos;
        RightArmTarget.transform.position = rightHandPos;
        //Invoke("DisableRays", 0.5f);
        playerState = PlayerState.Grabbed;
        isGrabbed = true;
        isWallJumping = false;
        isJumping = false;
    }

    private void LeaveWall()
    {
        LeftArmRig.GetComponent<Rig>().weight = 0;
        RightArmRig.GetComponent<Rig>().weight = 0;
        
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.4f, transform.position.z);
        //HandsManager.LeftHandChecker.SetActive(true);
        //HandsManager.RightHandChecker.SetActive(true);
        isGrabbed = false;
        //calculateGravity();

    }
    private void HandleGrab()
    {

        if (!HandsManager.GetLeftHand() && !HandsManager.GetRightHand() && playerState.Equals(PlayerState.Grabbed))
        {
            LeaveWall();
            playerState = PlayerState.GrabLeaved;
        }

    }
}
