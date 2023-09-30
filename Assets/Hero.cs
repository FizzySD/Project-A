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
        Debug.Log(state);
        if (grounded && (state == HERO_STATE.Idle || state == HERO_STATE.Slide)) {
            if (Input.GetKeyDown(KeyCode.LeftShift)) 
            {
                state = HERO_STATE.Idle;
                this.anim.SetTrigger("jump");
            }
            if (Input.GetKeyDown(KeyCode.LeftControl) && !this.anim.GetCurrentAnimatorStateInfo(0).IsName("dodge")) 
            {
                dodge();
                return;
            }
        }

        switch (state) {
            case HERO_STATE.GroundDodge:
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("dodge")) // but for now create a new branch because i did some changes to the animator okayy
                {
                    if (!grounded && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.6f) {
                        state = HERO_STATE.Idle;
                        
                    }
                    if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.95f) { // not dodge animation oh like the "0" animÖ wait nvm
                        state = HERO_STATE.Idle;
                    
                    }
                }
            break;
        }
    }

    public bool IsGrounded()
    {
        LayerMask mask = ((int) 1) << LayerMask.NameToLayer("Ground");
        LayerMask mask2 = ((int) 1) << LayerMask.NameToLayer("EnemyBox");
        LayerMask mask3 = mask2 | mask;
        return Physics.Raycast(base.gameObject.transform.position + Vector3.up * 0.1f, -Vector3.up, 1f,  mask3.value);
    } //iöm lost xd xd

    private void FixedUpdate()
    {   
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
        float num2 = 0;
        float num = 0;
        if(Input.GetKey(KeyCode.W))
        {
            num2 = 1f;
        }
        else if (Input.GetKey(KeyCode.S)) 
        {
            num2 = -1f;
        }
        if (Input.GetKey(KeyCode.A)) 
        {
            num = -1f;
        }
        else if(Input.GetKey(KeyCode.D)) 
        {
            num = 1f;
        }

        var flag = false;
        var flag2 = false;
        var flag3 = false;
        if (grounded && canMove) 
        {
            Vector3 vector5 = Vector3.zero;
            switch(this.state) {
                case HERO_STATE.GroundDodge:
                    {
                        if (anim.GetCurrentAnimatorStateInfo(0).IsName("dodge") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.2f && anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.8f)
                        {
                            vector5 = -transform.forward * 2.4f * speed;
                        }

                        if (anim.GetCurrentAnimatorStateInfo(0).IsName("dodge") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.8f)
                        {
                            vector5 =  rb.velocity;
                            vector5 *= 0.9f;
                        }

                        break;
                    }

                case HERO_STATE.Idle:
                    Vector3 vector6 = new Vector3(num, 0f, num2);
                    float num3 = getGlobalFacingDirection(num, num2);
                    vector5 = getGlobaleFacingVector3(num3);
                    float num4 = (!(vector6.magnitude <= 0.95f)) ? 1f : ((vector6.magnitude >= 0.25f) ? vector6.magnitude : 0f);
                    vector5 *= num4;
                    vector5 *= speed;
                    
                    if (num != 0f || num2 != 0f) {
                        anim.SetBool("isRunning", true);
                    }
                    else 
                    {
                        anim.SetBool("isRunning", false);
                        num3 = -874f;
                    }

                    if (num3 != -874f)
                    {
                        facingDirection = num3;
                        targetRotation = Quaternion.Euler(0f, facingDirection, 0f);
                    }

                    break;
                
            }

            
            Vector3 velocity = rb.velocity;
            Vector3 force = vector5 - velocity;
            force.x = Mathf.Clamp(force.x, 0f - maxVelocityChange, maxVelocityChange);
            force.z = Mathf.Clamp(force.z, 0f - maxVelocityChange, maxVelocityChange);
            force.y = 0f;
            //smth like that we need to find how to see the current anim
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("jump") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.18f) // there
            {
                force.y += 8f;
            }
            if (state != HERO_STATE.Attack) {
                rb.AddForce(force, ForceMode.VelocityChange);
                rb.rotation = Quaternion.Lerp(base.gameObject.transform.rotation, Quaternion.Euler(0f, facingDirection, 0f), Time.deltaTime * 10f);
            }

            // if(vector6.magnitude > 0.1f)
            // {
            //     anim.SetBool("isRunning", true);
            // }
            // else 
            // {
            //     anim.SetBool("isRunning", false); //ok no idea now lol
            // }

            currentSpeed = rb.velocity.magnitude;
            // Debug.Log(vector5);
        }
        else {
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("attack5") && !anim.GetCurrentAnimatorStateInfo(0).IsName("special_petra") && !anim.GetCurrentAnimatorStateInfo(0).IsName("dash") && !anim.GetCurrentAnimatorStateInfo(0).IsName("jump"))
            {
                var vector7 = new Vector3(num, 0f, num2);
                var num7 = getGlobalFacingDirection(num, num2);
                var vector8 = getGlobaleFacingVector3(num7);
                var d3 = vector7.magnitude <= 0.95f ? vector7.magnitude >= 0.25f ? vector7.magnitude : 0f : 1f;
                vector8 *= d3;
                vector8 *= 150 / 10f * 2f;
                if (num == 0f && num2 == 0f)
                {
                    if (state == HERO_STATE.Attack)
                    {
                        vector8 *= 0f;
                    }

                    num7 = -874f;
                }

                if (num7 != -874f)
                {
                    facingDirection = num7;
                    targetRotation = Quaternion.Euler(0f, facingDirection, 0f);
                }

                if (!flag2 && !flag3 && !isMounted && Input.GetKey(KeyCode.LeftShift) && currentGas > 0f)
                {
                    if (num != 0f || num2 != 0f)
                    {
                        
                        rb.AddForce(vector8, ForceMode.Acceleration);
                    }
                    else
                    {
                    
                        rb.AddForce(transform.forward * vector8.magnitude, ForceMode.Acceleration);
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


    private Vector3 getGlobaleFacingVector3(float resultAngle)
    {
        float num = 0f - resultAngle + 90f;
        float x = Mathf.Cos(num * 0.01745329f);
        return new Vector3(x, 0f, Mathf.Sin(num * 0.01745329f));
    }

    private Vector3 getGlobaleFacingVector3(float horizontal, float vertical)
    {
        float num = 0f - getGlobalFacingDirection(horizontal, vertical) + 90f;
        float x = Mathf.Cos(num * 0.01745329f);
        return new Vector3(x, 0f, Mathf.Sin(num * 0.01745329f));
    }


    private float getGlobalFacingDirection(float horizontal, float vertical)
    {
        if (vertical == 0f && horizontal == 0f)
        {
            return base.transform.rotation.eulerAngles.y;
        }
        float y = currentCamera.transform.rotation.eulerAngles.y;
        float num = Mathf.Atan2(vertical, horizontal) * 57.29578f;
        num = 0f - num + 90f;
        return y + num;
    }
    

    private void dodge(bool offTheWall = false)
    {
        
        this.state = HERO_STATE.GroundDodge;
        if (!offTheWall)
        {
            float num2 = 0;
            float num = 0;
            if(Input.GetKey(KeyCode.W))
            {
                num = 1f;
            }
            else if (Input.GetKey(KeyCode.S)) 
            {
                num = -1f;
            }
            if (Input.GetKey(KeyCode.A)) 
            {
                num2 = -1f;
            }
            else if(Input.GetKey(KeyCode.D)) 
            {
                num2 = 1f;
            }
            float num3 = this.getGlobalFacingDirection(num2, num);
            if ((num2 != 0f) || (num != 0f))
            {
                this.facingDirection = num3 + 180f;
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
    private HERO_STATE state
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

