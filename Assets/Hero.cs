using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


public class Hero : MonoBehaviour
{
    #region Comments
    //so num check if W is pressed or S is pressed, num2 check for A and D
    #endregion
    public Transform GasPoint;
    public GameObject gas; 
    public static Hero instance;
    private Rigidbody rb;
    private HERO_STATE _state;
    public float currentSpeed;
    private Animator anim;
    public float speed = 9.5f;
    private Camera currentCamera;
    public float maxVelocityChange = 10f;
    private readonly float gravity = 20f;
    private bool grounded;
    public bool canMove = true;
    private float dashTime;
    private float facingDirection;
    private Quaternion targetRotation;
    private bool isMounted = false;
    private float currentGas = 100;
    private object bulletLeft;
    private object bulletRight;

    // Start is called before the first frame update
    private void Awake()
    {
        instance = this;
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        currentCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        this.rb.mass = 0.5f - ((150 - 100) * 0.001f);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        drawRayCast();
        Debug.Log(State);
        if (grounded && (State == HERO_STATE.Idle || State == HERO_STATE.Slide)) {
            if (Input.GetKeyDown(KeyCode.LeftShift)) 
            {
                State = HERO_STATE.Idle;
                this.anim.SetTrigger("jump");
            }
            if (Input.GetKeyDown(KeyCode.LeftControl) && !this.anim.GetCurrentAnimatorStateInfo(0).IsName("dodge")) 
            {
                Dodge();
                return;
            }
        }

        switch (State) {
            case HERO_STATE.GroundDodge:
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("dodge")) // but for now create a new branch because i did some changes to the animator okayy
                {
                    if (!grounded && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.6f) {
                        State = HERO_STATE.Idle;
                        
                    }
                    if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.95f) { // not dodge animation oh like the "0" animÖ wait nvm
                        State = HERO_STATE.Idle;
                    
                    }
                }
            break;
        }
    }
    
    public void drawRayCast() {
        LayerMask mask = ((int) 1) << LayerMask.NameToLayer("Ground");
        LayerMask mask2 = ((int) 1) << LayerMask.NameToLayer("EnemyBox");
        LayerMask mask3 = mask2 | mask;
        Debug.DrawRay(base.gameObject.transform.position + Vector3.up * 0.1f, -Vector3.up * 1.1f, Color.red);
    }
    public bool IsGrounded()
    {
        LayerMask mask = ((int) 1) << LayerMask.NameToLayer("Ground");
        LayerMask mask2 = ((int) 1) << LayerMask.NameToLayer("EnemyBox");
        LayerMask mask3 = mask2 | mask;
        return Physics.Raycast(base.gameObject.transform.position + Vector3.up * 0.1f, -Vector3.up, 1.1f,  mask3.value);
    } 
    private void FixedUpdate()
    {   

        currentSpeed = rb.velocity.magnitude;

        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("attack3_2") && !anim.GetCurrentAnimatorStateInfo(0).IsName("attack5") && !anim.GetCurrentAnimatorStateInfo(0).IsName("special_petra") && !anim.GetCurrentAnimatorStateInfo(0).IsName("FallImpact"))
        {
            rb.rotation = Quaternion.Lerp(base.gameObject.transform.rotation, targetRotation, Time.fixedDeltaTime * 6f);
        }
        Debug.Log(IsGrounded());
        if (IsGrounded())
        {
            grounded = true;
        }
        else 
        {
            grounded = false;
        }
        anim.SetBool("isGrounded", grounded);
        InputHandler();
    }

    public void InputHandler() 
    {
        float verticalMovement = 0;
        float horizontalMovement = 0;
        if(Input.GetKey(KeyCode.W))
        {
            verticalMovement = 1f;
        }
        else if (Input.GetKey(KeyCode.S)) 
        {
            verticalMovement = -1f;
        }
        if (Input.GetKey(KeyCode.A)) 
        {
            horizontalMovement = -1f;
        }
        else if(Input.GetKey(KeyCode.D)) 
        {
            horizontalMovement = 1f;
        }

        var flag = false;
        var flag2 = false;
        var flag3 = false;
        if (grounded && canMove && !anim.GetCurrentAnimatorStateInfo(0).IsName("FallImpact")) 
        {
            Vector3 movementVector = Vector3.zero;
            switch(this.State) {
                case HERO_STATE.GroundDodge:
                    {
                        if (anim.GetCurrentAnimatorStateInfo(0).IsName("dodge") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.2f && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.8f)
                        {
                            movementVector = -transform.forward * 2.4f * speed;
                        }

                        if (anim.GetCurrentAnimatorStateInfo(0).IsName("dodge") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.8f)
                        {
                            movementVector =  rb.velocity;
                            movementVector *= 0.9f;
                        }

                        break;
                    }

                case HERO_STATE.Idle:
                    Vector3 inputVector = new Vector3(horizontalMovement, 0f, verticalMovement);
                    float globalDirection = GetGlobalFacingDirection(horizontalMovement, verticalMovement);
                    movementVector = GetGlobalFacingVector3(globalDirection);
                    float magnitudeModifer = inputVector.magnitude > 0.95f ? 1f : (inputVector.magnitude >= 0.25f ? inputVector.magnitude : 0f);
                    movementVector *= magnitudeModifer;
                    movementVector *= speed;
                    
                    if (horizontalMovement != 0f || verticalMovement != 0f) {
                        anim.SetBool("isRunning", true);
                    }
                    else 
                    {
                        anim.SetBool("isRunning", false);
                        globalDirection = -874f;
                    }

                    if (globalDirection != -874f)
                    {
                        facingDirection = globalDirection;
                        targetRotation = Quaternion.Euler(0f, facingDirection, 0f);
                    }

                    break;
                
            }

            
            Vector3 velocity = rb.velocity;
            Vector3 force = movementVector - velocity;
            force.x = Mathf.Clamp(force.x, 0f - maxVelocityChange, maxVelocityChange);
            force.z = Mathf.Clamp(force.z, 0f - maxVelocityChange, maxVelocityChange);
            force.y = 0f;
            //smth like that we need to find how to see the current anim
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("jump") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.18f) // there
            {
                force.y += 8f;
            }
            if (State != HERO_STATE.Attack) {
                rb.AddForce(force, ForceMode.VelocityChange);
                rb.rotation = Quaternion.Lerp(base.gameObject.transform.rotation, Quaternion.Euler(0f, facingDirection, 0f), Time.deltaTime * 10f);
            }

            currentSpeed = rb.velocity.magnitude;
        }
        else {
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("attack5") && !anim.GetCurrentAnimatorStateInfo(0).IsName("special_petra") && !anim.GetCurrentAnimatorStateInfo(0).IsName("dash") && !anim.GetCurrentAnimatorStateInfo(0).IsName("jump"))
            {
                var inputVector = new Vector3(horizontalMovement, 0f, verticalMovement);
                var globalDirection = GetGlobalFacingDirection(horizontalMovement, verticalMovement);
                var movementVector = GetGlobalFacingVector3(globalDirection);
                var magnitudeModifer = inputVector.magnitude <= 0.95f ? inputVector.magnitude >= 0.25f ? inputVector.magnitude : 0f : 1f;
                movementVector *= magnitudeModifer;
                movementVector *= 150 / 10f * 2f;
                if (horizontalMovement == 0f && verticalMovement == 0f)
                {
                    if (State == HERO_STATE.Attack)
                    {
                        movementVector *= 0f;
                    }

                    globalDirection = -874f;
                }

                if (globalDirection != -874f)
                {
                    facingDirection = globalDirection;
                    targetRotation = Quaternion.Euler(0f, facingDirection, 0f);
                }

                if (!flag2 && !flag3 && !isMounted && Input.GetKey(KeyCode.LeftShift) && currentGas > 0f)
                {
                    Instantiate(gas,GasPoint.transform.position, GasPoint.transform.rotation);
                    if (horizontalMovement != 0f || verticalMovement != 0f)
                    {
                        
                        rb.AddForce(movementVector, ForceMode.Acceleration);
                    }
                    else
                    {
                    
                        rb.AddForce(transform.forward * movementVector.magnitude, ForceMode.Acceleration);
                    }
                    
                    flag = true;
                }
            }
        }
        bool flag7 = false;
        if ((this.bulletLeft != null) || (this.bulletRight != null))
        {
            // if (((this.bulletLeft != null) && (this.bulletLeft.transform.position.y > base.gameObject.transform.position.y)) && (this.isLaunchLeft && this.bulletLeft.GetComponent<Bullet>().isHooked()))
            // {
            //     flag7 = true;
            // }
            // if (((this.bulletRight != null) && (this.bulletRight.transform.position.y > base.gameObject.transform.position.y)) && (this.isLaunchRight && this.bulletRight.GetComponent<Bullet>().isHooked()))
            // {
            //     flag7 = true;
            // }
        }
        if (flag7)
        {
            this.rb.AddForce(new Vector3(0f, -10f * this.rb.mass, 0f));
        }
        else
        {
            this.rb.AddForce(new Vector3(0f, -this.gravity * this.rb.mass, 0f));
        }
        if (this.currentSpeed > 10f)
        {
            this.currentCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(this.currentCamera.GetComponent<Camera>().fieldOfView, Mathf.Min((float) 100f, (float) (this.currentSpeed + 40f)), 0.1f);
        }
        else
        {
            this.currentCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(this.currentCamera.GetComponent<Camera>().fieldOfView, 50f, 0.1f);
        }
    }


    private Vector3 GetGlobalFacingVector3(float angle)
    {
        float angleInDegrees = 0f - angle + 90f;
        float x = Mathf.Cos(angleInDegrees * 0.01745329f);
        return new Vector3(x, 0f, Mathf.Sin(angleInDegrees * 0.01745329f));
    }
    private Vector3 GetGlobalFacingVector3(float horizontalMovement, float verticalMovement)
    {
        float angle = 0f - GetGlobalFacingDirection(horizontalMovement, verticalMovement) + 90f;
        float x = Mathf.Cos(angle * 0.01745329f);
        return new Vector3(x, 0f, Mathf.Sin(angle * 0.01745329f));
    }

    private float GetGlobalFacingDirection(float horizontalMovement, float verticalMovement)
    {
        if (verticalMovement == 0f && horizontalMovement == 0f)
        {
            return transform.rotation.eulerAngles.y;
        }
        float cameraRotationY = currentCamera.transform.rotation.eulerAngles.y;
        float angle = Mathf.Atan2(verticalMovement, horizontalMovement) * 57.29578f;
        angle = 0f - angle + 90f;
        return cameraRotationY + angle;
    }
    

    private void Dodge(bool offTheWall = false)
    {
        
        this.State = HERO_STATE.GroundDodge;
        if (!offTheWall)
        {
            float verticalMovement = 0;
            float horizontalMovement = 0;
            if(Input.GetKey(KeyCode.W))
            {
                verticalMovement = 1f;
            }
            else if (Input.GetKey(KeyCode.S)) 
            {
                verticalMovement = -1f;
            }
            if (Input.GetKey(KeyCode.A)) 
            {
                horizontalMovement = -1f;
            }
            else if(Input.GetKey(KeyCode.D)) 
            {
                horizontalMovement = 1f;
            }
            float globalDirection = this.GetGlobalFacingDirection(horizontalMovement, verticalMovement);
            if ((verticalMovement != 0f) || (horizontalMovement != 0f))
            {
                this.facingDirection = globalDirection + 180f;
                this.targetRotation = Quaternion.Euler(0f, this.facingDirection, 0f);
            }
            this.anim.SetTrigger("Dodging");
            // this.crossFade("dodge", 0.1f);
        }
        else
        {
            this.anim.SetTrigger("Dodging");
            // this.playAnimation("dodge");
            // this.playAnimationAt("dodge", 0.2f);
        }
            
        
    }
    private HERO_STATE State
    {
        get
        {
            return this._state;
        }
        set
        {
            if ((this._state == HERO_STATE.AirDodge) || (this._state == HERO_STATE.GroundDodge))
            {
                this.dashTime = 0f;
            }
            this._state = value;
        }
    }
}

