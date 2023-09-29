using System.Collections;
using System.Collections.Generic;
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
    // Start is called before the first frame update
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        currentCamera = GetComponentInChildren<Camera>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
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

        rb.AddForce(force,ForceMode.VelocityChange);
        rb.rotation = Quaternion.Lerp(base.gameObject.transform.rotation, Quaternion.Euler(0f, num3, 0f), Time.deltaTime * 10f);



        currentSpeed = rb.velocity.magnitude;
        anim.SetFloat("Velocity X", currentSpeed);
        anim.SetFloat("Velocity Z", num);
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
