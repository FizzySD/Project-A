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
    private Rigidbody rb;
    public float currentSpeed;
    private Animator anim;
    public float speed = 9.5f;
    private Camera currentCamera;
    public float maxVelocityChange = 10f;
    private readonly float gravity = 20f;
    private bool grounded;

    // Start is called before the first frame update
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        currentCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (grounded) {
            if (Input.GetKeyDown(KeyCode.LeftShift)) 
            {
                this.anim.SetTrigger("jump");
            }
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
        InputHandler();
    }

    public void InputHandler() 
    {
        float num2 = 0;
        if(Input.GetKey(KeyCode.W))
        {
            num2 = 1f;
        }
        else if (Input.GetKey(KeyCode.S)) 
        {
            num2 = -1f;
        }
        float num = 0;
        if (Input.GetKey(KeyCode.A)) 
        {
            num = -1f;
        }
        else if(Input.GetKey(KeyCode.D)) 
        {
            num = 1f;
        }
        if (grounded) {

            Vector3 vector5 = Vector3.zero;
            Vector3 vector6 = new Vector3(num, 0f, num2);
            float num3 = getGlobalFacingDirection(num, num2);
            vector5 = getGlobaleFacingVector3(num3);
            float num4 = ((!(vector6.magnitude <= 0.95f)) ? 1f : ((vector6.magnitude >= 0.25f) ? vector6.magnitude : 0f));
            vector5 *= num4;
            vector5 *= speed;
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

            rb.AddForce(force,ForceMode.VelocityChange);
            rb.rotation = Quaternion.Lerp(base.gameObject.transform.rotation, Quaternion.Euler(0f, num3, 0f), Time.deltaTime * 10f);

            if(vector6.magnitude > 0.1f)
            {
                anim.SetBool("isRunning", true);
            }
            else 
            {
                anim.SetBool("isRunning", false); //ok no idea now lol
            }

            currentSpeed = rb.velocity.magnitude;
            Debug.Log(vector5);
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
}

