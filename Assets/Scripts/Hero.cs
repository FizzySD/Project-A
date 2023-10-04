using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;


public class Hero : MonoBehaviour
{
    #region Comments
    //so num check if W is pressed or S is pressed, num2 check for A and D
    #endregion
    private bool QHold;
    private bool EHold;
    public Bullet lastHook;
    private AudioSource rope;
    private float launchElapsedTimeR;
    private float launchElapsedTimeL;
    public bool isLaunchLeft;
    public bool isLaunchRight;
    private bool isLeftHandHooked;
    private bool isRightHandHooked;
    private Vector3 launchPointLeft;
    private Vector3 launchPointRight;
    private Vector3 launchForce;
    public GameObject hookRefL1;
    public GameObject hookRefL2;
    public GameObject hookRefR1;
    public GameObject hookRefR2;



    public Transform GasPoint;
    public GameObject gas; 
    public static Hero instance;
    public Rigidbody rb;
    private HERO_STATE _state;
    public float currentSpeed;
    private Animation anim;
    public float speed = 9.5f;
    private Camera currentCamera;
    public float maxVelocityChange = 10f;
    private readonly float gravity = 20f;
    private bool grounded;
    public bool canMove = true;
    private float dashTime;
    private float facingDirection;
    private Vector3 dashV;
    private float originVM;
    private Quaternion targetRotation;
    private bool isMounted = false;
    public float currentGas = 100f;
    private Bullet bulletLeft;
    private Bullet bulletRight;
    public float totalGas = 100f;
    private float useGasSpeed = 0.2f;
    private bool _cancelGasDisable = false;
    private ParticleSystem smoke3Dmg;
    private DateTime _lastBurstTime = DateTime.Now;
    private float uTapTime = -1f;
    private float dTapTime = -1f;
    private float lTapTime = -1f;
    private float rTapTime = -1f;
    private float invincible = 3f;
    public float bulletTimer = 0f;
    private string currentAnimation;
    private bool gunner = false;
    private bool justGrounded;
    private string attackAnimation;
    private bool buttonAttackRelease;
    private float reelAxis = 0f;
    private bool almostSingleHook;
    private bool bodyLean = false;
    private float myScale = 1f;
    private bool attackMove;
    private bool attackReleased;
    private int attackLoop;
    private bool animationStopped = false;
    private bool needLean;
    private bool leanLeft;

    // Start is called before the first frame update
    private void Awake()
    {
        instance = this;
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animation>();
        currentCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        this.rb.mass = 0.5f - ((150 - 100) * 0.001f);
        
    }
    void Start()
    {
        // GameManager.gameManager.AddHero(this);
        smoke3Dmg = transform.Find("3dmg_smoke").GetComponent<ParticleSystem>();
        transform.localScale = new Vector3(myScale, myScale, myScale);
        facingDirection = transform.rotation.eulerAngles.y;
        targetRotation = Quaternion.Euler(0f, facingDirection, 0f);
        
    }

    private void CustomAnimationSpeed()
    {
        // anim["attack5"].speed = 1.85f;
        // anim["changeBlade"].speed = 1.2f;
        anim["air_release"].speed = 0.6f;
        // anim["changeBlade_air"].speed = 0.8f;
        // anim["AHSS_gun_reload_both"].speed = 0.38f;
        // anim["AHSS_gun_reload_both_air"].speed = 0.5f;
        // anim["AHSS_gun_reload_l"].speed = 0.4f;
        // anim["AHSS_gun_reload_l_air"].speed = 0.5f;
        // anim["AHSS_gun_reload_r"].speed = 0.4f;
        // anim["AHSS_gun_reload_r_air"].speed = 0.5f;
    }

    public string CurrentPlayingClipName()
    {
        foreach (var obj in anim)
        {
            var animationState = (AnimationState)obj;
            if (anim.IsPlaying(animationState.name))
            {
                return animationState.name;
            }
        }

        return string.Empty;
    }
    public void ContinueAnimation()
    {
        if (!animationStopped)
        {
            return;
        }
        animationStopped = false;
        foreach (var obj in anim)
        {
            var animationState = (AnimationState)obj;
            if (animationState.speed == 1f)
            {
                return;
            }

            animationState.speed = 1f;
        }

        CustomAnimationSpeed();
        PlayAnimation(CurrentPlayingClipName());
        // if (IN_GAME_MAIN_CAMERA.GameType != GameType.Single && BasePV.IsMine)
        // {
        //     BasePV.RPC("netContinueAnimation", PhotonTargets.Others);
        // }
    }
    public void FalseAttack()
    {
        attackMove = false;
        if (gunner)
        {
            if (!attackReleased)
            {
                ContinueAnimation();
                attackReleased = true;
            }
        }
        else
        {
            // if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single || BasePV.IsMine)
            // {
            //     wLeft.Active = false;
            //     wRight.Active = false;
            //     wLeft.clearHits();
            //     wRight.clearHits();
            //     leftbladetrail.StopSmoothly(0.2f);
            //     rightbladetrail.StopSmoothly(0.2f);
            //     leftbladetrail2.StopSmoothly(0.2f);
            //     rightbladetrail2.StopSmoothly(0.2f);
            // }

            attackLoop = 0;
            if (!attackReleased)
            {
                ContinueAnimation();
                attackReleased = true;
            }
        }
    }
    // Update is called once per frame
    public void Update()
    {   
    
    var dt = Time.deltaTime;
    if (invincible > 0f)
    {
        invincible -= dt;
    }
    
    if (bulletTimer > 0)
    {
        bulletTimer -= dt;
    }

    if (!grounded && State != HERO_STATE.AirDodge)
    {
        bool rebindedTrigger = Input.GetKey(KeyCode.LeftControl);
        if (rebindedTrigger)
        {
            if (Input.GetKey(KeyCode.W))
            {
                Dash(0f, 1f);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                Dash(0f, -1f);
            }
            else if (Input.GetKey(KeyCode.A))
            {
                Dash(-1f, 0f);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                Dash(1f, 0f);
            }
        }
        else
        {
            
            CheckDashDoubleTap();
            
        }
    }
    drawRayCast();
    if (grounded && (State == HERO_STATE.Idle || State == HERO_STATE.Slide)) 
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !anim.IsPlaying("oldJump")) 
        {
            Idle();
            // this.anim.SetTrigger("jump");
            CrossFade("oldJump", 0.1f);
        }
        if (Input.GetKeyDown(KeyCode.LeftControl) && !anim.IsPlaying("oldJump")) 
        {
            Dodge();
            return;
        }
    }
    Debug.Log(State);
    switch (State) {
        case HERO_STATE.GroundDodge:
            if (anim.IsPlaying("oldDodge")) // but for now create a new branch because i did some changes to the animator okayy
            {
                if (!grounded && anim["oldDodge"].normalizedTime > 0.6f) 
                {
                    Idle();
                    
                }
                if (anim["oldDodge"].normalizedTime >= 1f) 
                { // not dodge animation oh like the "0" animÖ wait nvm
                    Idle();
                
                }
            }
            break;
        case HERO_STATE.Land:
            if (anim.IsPlaying("dash_land") && anim["dash_land"].normalizedTime >= 1f)
            {
                Idle();
            }

            break;
        case HERO_STATE.AirDodge:
            if (dashTime > 0f)
            {
                dashTime -= dt;
                if (currentSpeed > originVM)
                {
                    rb.AddForce(-rb.velocity * dt * 1.7f, ForceMode.VelocityChange);
                }
            }
            else
            {
                dashTime = 0f;
                Idle();
            }

            break;
        case HERO_STATE.Slide:
            if (!grounded)
            {
                Idle();
            }

            break;
        }
        if ((!anim.IsPlaying("attack3_1") && !anim.IsPlaying("attack5") && !anim.IsPlaying("special_petra") && State != HERO_STATE.Grab) || State == HERO_STATE.Idle) {

            if (Input.GetKey(KeyCode.Q))
            {
                if (bulletLeft)
                {
                    QHold = true;
                }
                else
                {
                    Ray ray4 = currentCamera.ScreenPointToRay(Input.mousePosition);
                    LayerMask layerMask7 = 1 << LayerMask.NameToLayer("Ground");
                    LayerMask layerMask8 = 1 << LayerMask.NameToLayer("EnemyBox");
                    if (Physics.Raycast(ray4, out RaycastHit hitInfo4, 10000f, ((LayerMask)((int)layerMask8 | (int)layerMask7)).value))
                    {
                        LaunchLeftRope(hitInfo4, true);
                        //rope.Play();
                    }
                }
            }
            else
            {
                QHold = false;
            }
            if (Input.GetKey(KeyCode.E))
            {
                if (bulletRight)
                {
                    EHold = true;
                }
                else
                {
                    Ray ray5 = currentCamera.ScreenPointToRay(Input.mousePosition);
                    LayerMask layerMask9 = 1 << LayerMask.NameToLayer("Ground");
                    LayerMask layerMask10 = 1 << LayerMask.NameToLayer("EnemyBox");
                    if (Physics.Raycast(ray5, out RaycastHit hitInfo5, 10000f, ((LayerMask)((int)layerMask10 | (int)layerMask9)).value))
                    {
                        LaunchRightRope(hitInfo5, true);
                        //rope.Play();
                    }
                }
            }
            else
            {
                EHold = false;
            }
            if (Input.GetKey(KeyCode.Space))
            {
                QHold = true;
                EHold = true;
                if (bulletLeft == null && bulletRight == null)
                {
                    Ray ray6 = currentCamera.ScreenPointToRay(Input.mousePosition);
                    LayerMask layerMask11 = 1 << LayerMask.NameToLayer("Ground");
                    LayerMask layerMask12 = 1 << LayerMask.NameToLayer("EnemyBox");
                    if (Physics.Raycast(ray6, out RaycastHit hitInfo6, 1000000f, ((LayerMask)((int)layerMask12 | (int)layerMask11)).value))
                    {
                        LaunchLeftRope(hitInfo6, false);
                        LaunchRightRope(hitInfo6, false);
                        //rope.Play();
                    }
                }
            }
        }
    }

    public void Launch(Vector3 des, bool left = true, bool leviMode = false)
    {
        if (left)
        {
            isLaunchLeft = true;
            launchElapsedTimeL = 0f;
        }
        else
        {
            isLaunchRight = true;
            launchElapsedTimeR = 0f;
        }
        /*
        if (state == HERO_STATE.Grab)
        {
            return;
        }
        */
        if (isMounted)
        {
            //unmounted();
        }
        if (State != HERO_STATE.Attack)
        {
            Idle();
        }
        Vector3 vector = des - base.transform.position;
        if (left)
        {
            launchPointLeft = des;
        }
        else
        {
            launchPointRight = des;
        }
        vector.Normalize();
        vector *= 20f;
        if (bulletLeft != null && bulletRight != null && bulletLeft.IsHooked() && bulletRight.IsHooked())
        {
            vector *= 0.8f;
        }
        leviMode = anim.IsPlaying("attack5") || anim.IsPlaying("special_petra");
        if (!leviMode)
        {
            FalseAttack();
            Idle();
            if (bodyLean) {
                /*
                if (useGun)
                {
                    crossFade("AHSS_hook_forward_both", 0.1f);
                }
                */
                if (left && !isRightHandHooked)
                {
                    CrossFade("air_hook_l_just", 0.1f);
                }
                else if (!left && !isLeftHandHooked)
                {
                    CrossFade("air_hook_r_just", 0.1f);
                }
                else
                {
                    CrossFade("oldDash", 0.1f);
                    anim["oldDash"].time = 0f;
                }
            }
        }
        if (left)
        {
            isLaunchLeft = true;
        }

        if (!left)
        {
            isLaunchRight = true;
        }
        launchForce = vector;
        if (!leviMode)
        {
            if (vector.y < 30f)
            {
                launchForce += Vector3.up * (30f - vector.y);
            }
            if (des.y >= base.transform.position.y)
            {
                launchForce += Vector3.up * (des.y - base.transform.position.y) * 10f;
            }
            rb.AddForce(launchForce);
        }
        facingDirection = Mathf.Atan2(launchForce.x, launchForce.z) * 57.29578f;
        Quaternion rotation = Quaternion.Euler(0f, facingDirection, 0f);
        gameObject.transform.rotation = rotation;
        rb.rotation = rotation;
        targetRotation = rotation;
        if (left)
        {
            launchElapsedTimeL = 0f;
        }
        else
        {
            launchElapsedTimeR = 0f;
        }

        if (leviMode)
        {
            launchElapsedTimeR = -100f;
        }
        if (anim.IsPlaying("special_petra"))
        {
            launchElapsedTimeR = -100f;
            launchElapsedTimeL = -100f;
            if (bulletRight != null)
            {
                bulletRight.Disable();
                //releaseIfIHookSb();
            }
            if (bulletLeft != null)
            {
                bulletLeft.Disable();
                //releaseIfIHookSb();
            }
        }
        _cancelGasDisable = true;
        //sparks.enableEmission = false;
    }



    private void Dash(float horizontal, float vertical)
    {
        DateTime now = DateTime.Now;
        if (dashTime > 0f
            || currentGas <= 0f
            || isMounted
            && (now - _lastBurstTime) < TimeSpan.FromMilliseconds(300))
        {
            return;
        }

        UseGas(totalGas * 0.04f);
        facingDirection = GetGlobalFacingDirection(horizontal, vertical);
        dashV = GetGlobalFacingVector3(facingDirection);
        originVM = currentSpeed;
        var rotation = Quaternion.Euler(0f, facingDirection, 0f);
        rb.rotation = rotation;
        targetRotation = rotation;

        UnityEngine.Object.Instantiate(Resources.Load("FX/GasBurst"), base.transform.position, base.transform.rotation);
        // if (IN_GAME_MAIN_CAMERA.GameType == GameType.Single)
        // {
        //     Pool.Enable("FX/boost_smoke", baseT.position, baseT.rotation);
        // }
        // else
        // {
        //     Pool.NetworkEnable("FX/boost_smoke", baseT.position, baseT.rotation);
        // }

        dashTime = 0.5f;
        CrossFade("oldDash", 0.1f);
        anim["oldDash"].time = 0.1f;
        // anim.SetTrigger("Dashing");
        State = HERO_STATE.AirDodge;
        FalseAttack();
        rb.AddForce(dashV * 40f, ForceMode.VelocityChange);
        _lastBurstTime = now;
    }

    public void CrossFade(string aniName, float time)
    {
        currentAnimation = aniName;
        anim.CrossFade(aniName, time);
        // if (!PhotonNetwork.connected)
        // {
        //     return;
        // }

        // if (BasePV.IsMine)
        // {
        //     BasePV.RPC("netCrossFade", PhotonTargets.Others, aniName, time);
        // }
    }
    private void Idle()
    {
        if (State == HERO_STATE.Attack)
        {
            FalseAttack();
        }

        State = HERO_STATE.Idle;
        CrossFade("oldStand", 0.1f);
    }


    private void CheckDashDoubleTap()
    {
        if (uTapTime >= 0f)
        {
            uTapTime += Time.deltaTime;
            if (uTapTime > 0.2f)
            {
                uTapTime = -1f;
            }
        }

        if (dTapTime >= 0f)
        {
            dTapTime += Time.deltaTime;
            if (dTapTime > 0.2f)
            {
                dTapTime = -1f;
            }
        }

        if (lTapTime >= 0f)
        {
            lTapTime += Time.deltaTime;
            if (lTapTime > 0.2f)
            {
                lTapTime = -1f;
            }
        }

        if (rTapTime >= 0f)
        {
            rTapTime += Time.deltaTime;
            if (rTapTime > 0.2f)
            {
                rTapTime = -1f;
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (uTapTime == -1f)
            {
                uTapTime = 0f;
            }

            if (uTapTime != 0f)
            {
                Dash(0f, 1f);
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (dTapTime == -1f)
            {
                dTapTime = 0f;
            }

            if (dTapTime != 0f)
            {
                Dash(0f, -1f);
            }
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (lTapTime == -1f)
            {
                lTapTime = 0f;
            }

            if (lTapTime != 0f)
            {
                Dash(-1f, 0f);
            }
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            if (rTapTime == -1f)
            {
                rTapTime = 0f;
            }

            if (rTapTime != 0f)
            {
                Dash(1f, 0f);
            }
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

        if (!anim.IsPlaying("attack3_2") && !anim.IsPlaying("attack5") && !anim.IsPlaying("special_petra"))
        {
            rb.rotation = Quaternion.Lerp(base.gameObject.transform.rotation, targetRotation, Time.fixedDeltaTime * 6f);
        }
        Debug.Log(IsGrounded());
        if (IsGrounded())
        {   
            if (!grounded) {
                justGrounded = true;
            }
            grounded = true;
        }
        else 
        {
            grounded = false;
        }

        
        
        // anim.SetBool("isGrounded", grounded);
        InputHandler();
    }

    private void LaunchLeftRope(RaycastHit hit, bool single, int mode = 0)
    {
        if (currentGas == 0f)
        {
            return;
        }

        UseGas();
        bulletLeft = ((GameObject)Instantiate(Resources.Load("hook"))).GetComponent<Bullet>();

        GameObject gameObject = hookRefL1;
        string launcher_ref = "hookRefL1";
        bulletLeft.transform.position = gameObject.transform.position;
        float num = !single ? hit.distance <= 50f ? hit.distance * 0.05f : hit.distance * 0.3f : 0f;
        var component = bulletLeft;
        Vector3 vector = hit.point - base.transform.right * num - component.transform.position;
        vector.Normalize();
        Debug.Log("Sigle :" + single + " Mode: " + mode);
        if (mode == 1)
        {
            component.Launch(vector * 3f, rb.velocity, launcher_ref, true, this, true);
        }
        else
        {
            component.Launch(vector * 3f, rb.velocity, launcher_ref, true, this);
        }
        launchPointLeft = Vector3.zero;
        
    }

    private void LaunchRightRope(RaycastHit hit, bool single, int mode = 0)
    {
        if (currentGas == 0f)
        {
            return;
        }
        UseGas();

        bulletRight = ((GameObject)Instantiate(Resources.Load("hook"))).GetComponent<Bullet>();
        GameObject gameObject = hookRefR1;
        string launcher_ref = "hookRefR1";
        bulletRight.transform.position = gameObject.transform.position;
        var component = bulletRight;
        float num = !single ? hit.distance <= 50f ? hit.distance * 0.05f : hit.distance * 0.3f : 0f;
        Vector3 vector = hit.point + base.transform.right * num - component.transform.position;
        vector.Normalize();
        if (mode == 1)
        {
            component.Launch(vector * 5f, rb.velocity, launcher_ref, false, this, true);
        }
        else
        {
            component.Launch(vector * 3f, rb.velocity, launcher_ref, false, this);
        }
        launchPointRight = Vector3.zero;
        
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
        isLeftHandHooked = false;
        isRightHandHooked = false;
        if (isLaunchLeft)
        {
            if (bulletLeft != null && bulletLeft.IsHooked())
            {
                isLeftHandHooked = true;
                Vector3 vector3 = bulletLeft.transform.position - transform.position;
                vector3.Normalize();
                vector3 *= 10f;
                if (!isLaunchRight)
                {
                    vector3 *= 2f;
                }
                if (Vector3.Angle(rb.velocity, vector3) > 90f && Input.GetKey(KeyCode.LeftShift))
                {
                    flag2 = true;
                    flag = true;
                }
                if (!flag2)
                {
                    rb.AddForce(vector3);
                    if (Vector3.Angle(rb.velocity, vector3) > 90f)
                    {
                        rb.AddForce(-rb.velocity * 2f, ForceMode.Acceleration);
                    }
                }

                // if (!bodyLean)
                // {
                //     facingDirection = Mathf.Atan2(vector3.x, vector3.z) * 57.29578f;
                //     var rotation = Quaternion.Euler(0f, facingDirection, 0f);
                //     transform.rotation = rotation;
                //     rb.rotation = rotation;
                // }
            }
            launchElapsedTimeL += Time.deltaTime;
            if (QHold && currentGas > 0f)
            {
                UseGas(useGasSpeed * Time.deltaTime);
            }
            else if (launchElapsedTimeL > 0.3f)
            {
                isLaunchLeft = false;
                if (bulletLeft != null)
                {
                    var component = bulletLeft;
                    component.Disable();
                    bulletLeft = null;
                    flag2 = false;
                }
            }
        }
        if (isLaunchRight)
        {
            if (bulletRight != null && bulletRight.IsHooked())
            {
                isRightHandHooked = true;
                Vector3 vector4 = bulletRight.transform.position - transform.position;
                vector4.Normalize();
                vector4 *= 10f;
                if (!isLaunchLeft)
                {
                    vector4 *= 2f;
                }
                if (Vector3.Angle(rb.velocity, vector4) > 90f && Input.GetKey(KeyCode.LeftShift))
                {
                    flag3 = true;
                    flag = true;
                }
                if (!flag3)
                {
                    rb.AddForce(vector4);
                    if (Vector3.Angle(rb.velocity, vector4) > 90f)
                    {
                        rb.AddForce(-rb.velocity * 2f, ForceMode.Acceleration);
                    }
                }

                // if (!bodyLean)
                // {
                //     facingDirection = Mathf.Atan2(vector4.x, vector4.z) * 57.29578f;
                //     var rotation = Quaternion.Euler(0f, facingDirection, 0f);
                //     transform.rotation = rotation;
                //     rb.rotation = rotation;
                // }
            }
            launchElapsedTimeR += Time.deltaTime;
            if (EHold && currentGas > 0f)
            {
                UseGas(useGasSpeed * Time.deltaTime);
            }
            else if (launchElapsedTimeR > 0.3f)
            {
                isLaunchRight = false;
                if (bulletRight != null)
                {
                    var component2 = bulletRight;
                    component2.Disable();
                    bulletRight = null;
                    flag3 = false;
                }
            }
        }
        if (grounded) 
        {
            Vector3 movementVector = Vector3.zero;
            if (justGrounded)
            {
                if (State != HERO_STATE.Attack || attackAnimation != "attack3_1" && attackAnimation != "attack5" &&
                    attackAnimation != "special_petra")
                {
                    if (State != HERO_STATE.Attack && horizontalMovement == 0f && verticalMovement == 0f && !bulletLeft && !bulletRight &&
                        State != HERO_STATE.FillGas)
                    {
                        State = HERO_STATE.Land;
                        CrossFade("dash_land", 0.01f);
                    }
                    else
                    {
                        buttonAttackRelease = true;
                        if (State != HERO_STATE.Attack &&
                            rb.velocity.x * rb.velocity.x + rb.velocity.z * rb.velocity.z >
                            speed * speed * 1.5f && State != HERO_STATE.FillGas)
                        {
                            State = HERO_STATE.Slide;
                            CrossFade("slide", 0.05f);
                            var velocity1 = rb.velocity;
                            facingDirection = Mathf.Atan2(velocity1.x, velocity1.z) * 57.29578f;
                            targetRotation = Quaternion.Euler(0f, facingDirection, 0f);
                            // sparks.enableEmission = true;
                        }
                    }
                }

                justGrounded = false;
                movementVector = rb.velocity;
            }


            
            switch(this.State) {
                case HERO_STATE.GroundDodge:
                    {
                        if (anim["oldDodge"].normalizedTime >= 0.2f && anim["oldDodge"].normalizedTime < 0.8f)
                        {
                            movementVector = 2.4f * speed * -transform.forward;
                        }

                        if (anim["oldDodge"].normalizedTime > 0.8f)
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
                    float magnitudeModifer = inputVector.magnitude <= 0.95f ? inputVector.magnitude >= 0.25f ? inputVector.magnitude : 0f : 1f;
                    movementVector *= magnitudeModifer;
                    movementVector *= speed;
                    
                    if (horizontalMovement != 0f || verticalMovement != 0f) {
                        if (!anim.IsPlaying("oldRun") && !anim.IsPlaying("oldJump")) {
                            
                            CrossFade("oldRun", 0.1f);
                            // anim.SetBool("isRunning", true);
                        }
                    }
                    else 
                    {
                        if (!anim.IsPlaying("oldStand") && State != HERO_STATE.Land && !anim.IsPlaying("oldJump")) {
                            CrossFade("oldStand", 0.1f);
                            movementVector *= 0f;
                            // anim.SetBool("isRunning", false);
                        }
                        // anim.SetBool("isRunning", false);
                        globalDirection = -874f;
                    }

                    if (globalDirection != -874f)
                    {
                        facingDirection = globalDirection;
                        targetRotation = Quaternion.Euler(0f, facingDirection, 0f);
                    }

                    break;
                case HERO_STATE.Land:
                    movementVector = rb.velocity;
                    movementVector *= 0.96f;
                    break;

                case HERO_STATE.Slide:
                    {
                        movementVector = rb.velocity;
                        movementVector *= 0.99f;
                        if (currentSpeed < speed * 1.2f)
                        {
                            Idle();
                            // sparks.enableEmission = false;
                        }

                        break;
                    }
            }

            
            Vector3 velocity = rb.velocity;
            Vector3 force = movementVector - velocity;
            force.x = Mathf.Clamp(force.x, -maxVelocityChange, maxVelocityChange);
            force.z = Mathf.Clamp(force.z, -maxVelocityChange, maxVelocityChange);
            force.y = 0f;
            //smth like that we need to find how to see the current anim
            if (anim.IsPlaying("oldJump") && anim["oldJump"].normalizedTime > 0.18f) // there
            {
                force.y += 8f;
            }
            if (State != HERO_STATE.Attack || !gunner) {
                rb.AddForce(force, ForceMode.VelocityChange);
                rb.rotation = Quaternion.Lerp(base.gameObject.transform.rotation, Quaternion.Euler(0f, facingDirection, 0f), Time.deltaTime * 10f);
            }

            // currentSpeed = rb.velocity.magnitude;
        }
        else {

            if (State == HERO_STATE.Idle && !anim.IsPlaying("oldDash") && !anim.IsPlaying("wallrun") &&
                !anim.IsPlaying("toRoof") && !anim.IsPlaying("horse_geton") && !anim.IsPlaying("horse_getoff") &&
                !anim.IsPlaying("air_release") && !isMounted &&
                (!anim.IsPlaying("air_hook_l_just") || anim["air_hook_l_just"].normalizedTime >= 1f) &&
                (!anim.IsPlaying("air_hook_r_just") || anim["air_hook_r_just"].normalizedTime >= 1f) ||
                anim["oldDash"].normalizedTime >= 0.99f)
            {
                if (!isLeftHandHooked && !isRightHandHooked &&
                    (anim.IsPlaying("air_hook_l") || anim.IsPlaying("air_hook_r") || anim.IsPlaying("air_hook")) &&
                    rb.velocity.y > 20f)
                {
                    anim.CrossFade("air_release");
                }
                else
                {
                    var flag4 = Mathf.Abs(rb.velocity.x) + Mathf.Abs(rb.velocity.z) > 25f;
                    var flag5 = rb.velocity.y < 0f;
                    if (!flag4)
                    {
                        if (flag5)
                        {
                            if (!anim.IsPlaying("air_fall"))
                            {
                                CrossFade("air_fall", 0.2f);
                            }
                        }
                        else if (!anim.IsPlaying("air_rise"))
                        {
                            CrossFade("air_rise", 0.2f);
                        }
                    }
                    else if (!isLeftHandHooked && !isRightHandHooked)
                    {
                        var velocity = rb.velocity;
                        var cr = -Mathf.Atan2(velocity.z, velocity.x) * 57.29578f;
                        var num6 = -Mathf.DeltaAngle(cr, transform.rotation.eulerAngles.y - 90f);
                        if (Mathf.Abs(num6) < 45f)
                        {
                            if (!anim.IsPlaying("air2"))
                            {
                                CrossFade("air2", 0.2f);
                            }
                        }
                        else if (num6 < 135f && num6 > 0f)
                        {
                            if (!anim.IsPlaying("air2_right"))
                            {
                                CrossFade("air2_right", 0.2f);
                            }
                        }
                        else if (num6 > -135f && num6 < 0f)
                        {
                            if (!anim.IsPlaying("air2_left"))
                            {
                                CrossFade("air2_left", 0.2f);
                            }
                        }
                        else if (!anim.IsPlaying("air2_backward"))
                        {
                            CrossFade("air2_backward", 0.2f);
                        }
                    }
                    else if (gunner)
                    {
                        if (!isRightHandHooked)
                        {
                            if (!anim.IsPlaying("AHSS_hook_forward_l"))
                            {
                                CrossFade("AHSS_hook_forward_l", 0.1f);
                            }
                        }
                        else if (!isLeftHandHooked)
                        {
                            if (!anim.IsPlaying("AHSS_hook_forward_r"))
                            {
                                CrossFade("AHSS_hook_forward_r", 0.1f);
                            }
                        }
                        else if (!anim.IsPlaying("AHSS_hook_forward_both"))
                        {
                            CrossFade("AHSS_hook_forward_both", 0.1f);
                        }
                    }
                    else if (!isRightHandHooked)
                    {
                        if (!anim.IsPlaying("air_hook_l"))
                        {
                            CrossFade("air_hook_l", 0.1f);
                        }
                    }
                    else if (!isLeftHandHooked)
                    {
                        if (!anim.IsPlaying("air_hook_r"))
                        {
                            CrossFade("air_hook_r", 0.1f);
                        }
                    }
                    else if (!anim.IsPlaying("air_hook"))
                    {
                        CrossFade("air_hook", 0.1f);
                    }
                }
            }

            if (!anim.IsPlaying("air_rise"))
            {
                if (State == HERO_STATE.Idle && anim.IsPlaying("air_release") && anim["air_release"].normalizedTime >= 1f)
                {
                    CrossFade("air_rise", 0.2f);
                }

                if (anim.IsPlaying("horse_getoff") && anim["horse_getoff"].normalizedTime >= 1f)
                {
                    CrossFade("air_rise", 0.2f);
                }
            }


            if (!anim.IsPlaying("attack5") && !anim.IsPlaying("special_petra") && !anim.IsPlaying("oldDash") && !anim.IsPlaying("oldJump"))
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
        var current = Vector3.zero;
        if (flag2 && flag3)
        {
            current = (bulletRight.transform.position + bulletLeft.transform.position) * 0.5f - transform.position;
        }
        else if (flag2 && !flag3)
        {
            current = bulletLeft.transform.position - transform.position;
        }
        else if (flag3 && !flag2)
        {
            current = bulletRight.transform.position - transform.position;
        }

        if (flag2 || flag3)
        {
            
            rb.AddForce(Vector3.zero, ForceMode.VelocityChange);
            // rb.AddForce(Vector3.zero, ForceMode.Acceleration);
            if (Input.GetKey(KeyCode.Space))
            {
                reelAxis = -1f;
            }
            else if (Input.GetKey(KeyCode.LeftAlt))
            {
                reelAxis = 1f;
            }
            // else if(InputManager.DisableMouseReeling.Value == false)
            // {
            //     reelAxis = Input.GetAxis("Mouse ScrollWheel") * 5555f;
            // }

            var idk = 1.53938f * (1f + Mathf.Clamp(reelAxis, -0.8f, 0.8f));
            reelAxis = 0f;
            rb.velocity = Vector3.RotateTowards(current, rb.velocity, idk, idk).normalized * (currentSpeed + 0.1f);

            
        }

        bool flag7 = false;
        if ((this.bulletLeft != null) || (this.bulletRight != null))
        {
            if ((this.bulletLeft != null) && (this.bulletLeft.transform.position.y > base.gameObject.transform.position.y) && this.isLaunchLeft && this.bulletLeft.IsHooked())
            {
                flag7 = true;
            }
            if ((this.bulletRight != null) && (this.bulletRight.transform.position.y > base.gameObject.transform.position.y) && this.isLaunchRight && this.bulletRight.IsHooked())
            {
                flag7 = true;
            }
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
        if (!_cancelGasDisable)
        {
            if (flag)
            {
                UseGas(useGasSpeed * Time.deltaTime);
                // if (!smoke3Dmg.enableEmission && IN_GAME_MAIN_CAMERA.GameType != GameType.Single)
                // {
                //     BasePV.RPC("net3DMGSMOKE", PhotonTargets.Others, true);
                // }
                // Instantiate(gas,GasPoint.transform.position, GasPoint.transform.rotation);
                var emission = smoke3Dmg.emission;
                emission.enabled = true;
            }
            else
            {
            //     if (smoke3Dmg.enableEmission && IN_GAME_MAIN_CAMERA.GameType != GameType.Single)
            //     {
            //         BasePV.RPC("net3DMGSMOKE", PhotonTargets.Others, false);
            //     }

                var emission = smoke3Dmg.emission;
                emission.enabled = false;
            }
        }
        else
        {
            _cancelGasDisable = false;
        }
        SetHookedPplDirection();
        BodyLean();
    }

    private void BodyLean()
    {
        var z = 0f;
        needLean = false;
        if (!grounded && !gunner && State == HERO_STATE.Attack && attackAnimation != "attack3_1" &&
            attackAnimation != "attack3_2")
        {
            var velocity = rb.velocity;
            var y = velocity.y;
            var x = velocity.x;
            var z2 = velocity.z;
            var x2 = Mathf.Sqrt(x * x + z2 * z2);
            var num = Mathf.Atan2(y, x2) * 57.29578f;
            targetRotation = Quaternion.Euler(-num * (1f - Vector3.Angle(rb.velocity, transform.forward) / 90f),
                facingDirection, 0f);
            if (isLeftHandHooked && bulletLeft != null || isRightHandHooked && bulletRight != null)
            {
                transform.rotation = targetRotation;
            }

            return;
        }

        if (isLeftHandHooked && bulletLeft != null && isRightHandHooked && bulletRight != null)
        {
            if (almostSingleHook)
            {
                needLean = true;
                z = GetLeanAngle(bulletRight.transform.position, true);
            }
        }
        else if (isLeftHandHooked && bulletLeft != null)
        {
            needLean = true;
            z = GetLeanAngle(bulletLeft.transform.position, true);
        }
        else if (isRightHandHooked && bulletRight != null)
        {
            needLean = true;
            z = GetLeanAngle(bulletRight.transform.position, false);
        }

        if (needLean)
        {
            var num2 = 0f;
            if (!gunner && State != HERO_STATE.Attack)
            {
                num2 = currentSpeed * 0.1f;
                num2 = Mathf.Min(num2, 20f);
            }

            targetRotation = Quaternion.Euler(-num2, facingDirection, z);
        }
        else if (State != HERO_STATE.Attack)
        {
            targetRotation = Quaternion.Euler(0f, facingDirection, 0f);
        }
    }


    private float GetLeanAngle(Vector3 p, bool left)
    {
        if (!gunner && State == HERO_STATE.Attack)
        {
            return 0f;
        }

        var position = transform.position;
        var num = p.y - position.y;
        var num2 = Vector3.Distance(p, position);
        var num3 = Mathf.Acos(num / num2) * 57.29578f;
        num3 *= 0.1f;
        num3 *= 1f + Mathf.Pow(rb.velocity.magnitude, 0.2f);
        var vector = p - position;
        var current = Mathf.Atan2(vector.x, vector.z) * 57.29578f;
        var velocity = rb.velocity;
        var target = Mathf.Atan2(velocity.x, velocity.z) * 57.29578f;
        var num4 = Mathf.DeltaAngle(current, target);
        num3 += Mathf.Abs(num4 * 0.5f);
        if (State != HERO_STATE.Attack)
        {
            num3 = Mathf.Min(num3, 80f);
        }

        if (num4 > 0f)
        {
            leanLeft = true;
        }
        else
        {
            leanLeft = false;
        }

        if (gunner)
        {
            return num3 * (num4 >= 0f ? 1 : -1);
        }

        float num5;
        if (left && num4 < 0f || !left && num4 > 0f)
        {
            num5 = 0.1f;
        }
        else
        {
            num5 = 0.5f;
        }

        return num3 * (num4 >= 0f ? num5 : -num5);
    }
    private void SetHookedPplDirection()
    {
        almostSingleHook = false;
        if (isRightHandHooked && isLeftHandHooked)
        {
            if (bulletLeft != null && bulletRight != null)
            {
                var vector = bulletLeft.transform.position - bulletRight.transform.position;
                if (vector.sqrMagnitude < 4f)
                {
                    var vector2 = (bulletLeft.transform.position + bulletRight.transform.position) * 0.5f - transform.position;
                    facingDirection = Mathf.Atan2(vector2.x, vector2.z) * 57.29578f;
                    if (gunner && State != HERO_STATE.Attack)
                    {
                        var velocity = rb.velocity;
                        var current = -Mathf.Atan2(velocity.z, velocity.x) * 57.29578f;
                        var target = -Mathf.Atan2(vector2.z, vector2.x) * 57.29578f;
                        var num = -Mathf.DeltaAngle(current, target);
                        facingDirection += num;
                    }

                    almostSingleHook = true;
                }
                else
                {
                    var position = transform.position;
                    var position1 = bulletLeft.transform.position;
                    var to = position - position1;
                    var position2 = bulletRight.transform.position;
                    var to2 = position - position2;
                    var vector3 = (position1 + position2) * 0.5f;
                    var from = position - vector3;
                    if (Vector3.Angle(from, to) < 30f && Vector3.Angle(from, to2) < 30f)
                    {
                        almostSingleHook = true;
                        var vector4 = vector3 - transform.position;
                        facingDirection = Mathf.Atan2(vector4.x, vector4.z) * 57.29578f;
                    }
                    else
                    {
                        almostSingleHook = false;
                        var forward = transform.forward;
                        Vector3.OrthoNormalize(ref vector, ref forward);
                        facingDirection = Mathf.Atan2(forward.x, forward.z) * 57.29578f;
                        var current2 = Mathf.Atan2(to.x, to.z) * 57.29578f;
                        var num2 = Mathf.DeltaAngle(current2, facingDirection);
                        if (num2 > 0f)
                        {
                            facingDirection += 180f;
                        }
                    }
                }
            }
        }
        else
        {
            almostSingleHook = true;
            Vector3 vector5;
            if (isRightHandHooked && bulletRight != null)
            {
                vector5 = bulletRight.transform.position - transform.position;
            }
            else
            {
                if (!isLeftHandHooked || !(bulletLeft != null))
                {
                    return;
                }

                vector5 = bulletLeft.transform.position - transform.position;
            }

            facingDirection = Mathf.Atan2(vector5.x, vector5.z) * 57.29578f;
            if (State != HERO_STATE.Attack)
            {
                var velocity = rb.velocity;
                var current3 = -Mathf.Atan2(velocity.z, velocity.x) * 57.29578f;
                var target2 = -Mathf.Atan2(vector5.z, vector5.x) * 57.29578f;
                var num3 = -Mathf.DeltaAngle(current3, target2);
                if (gunner)
                {
                    facingDirection += num3;
                }
                else
                {
                    float num4;
                    if (isLeftHandHooked && num3 < 0f || isRightHandHooked && num3 > 0f)
                    {
                        num4 = -0.1f;
                    }
                    else
                    {
                        num4 = 0.1f;
                    }

                    facingDirection += num3 * num4;
                }
            }
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
            // this.anim.SetTrigger("Dodging");
            CrossFade("oldDodge", 0.1f);
        }
        else
        {
            // this.anim.SetTrigger("Dodging");
            PlayAnimation("oldDodge");
            PlayAnimationAt("oldDodge", 0.2f);
        }
            
        
    }

    private void PlayAnimationAt(string aniName, float normalizedTime)
    {
        currentAnimation = aniName;
        anim.Play(aniName);
        anim[aniName].normalizedTime = normalizedTime;
        // if (!PhotonNetwork.connected)
        // {
        //     return;
        // }

        // if (BasePV.IsMine)
        // {
        //     BasePV.RPC("netPlayAnimationAt", PhotonTargets.Others, aniName, normalizedTime);
        // }
    }
    
    public void PlayAnimation(string aniName)
    {
        currentAnimation = aniName;
        anim.Play(aniName);
        // if (!PhotonNetwork.connected)
        // {
        //     return;
        // }

        // if (BasePV.IsMine)
        // {
        //     BasePV.RPC("netPlayAnimation", PhotonTargets.Others, aniName);
        // }
    }

    public void FillGas()
    {
        this.currentGas = this.totalGas;
    }
     private void UseGas(float amount = 0)
    {
        
        if (amount == 0f)
        {
            amount = this.useGasSpeed;
        }
        if (this.currentGas > 0f)
        {
            this.currentGas -= amount;
            if (this.currentGas < 0f)
            {
                this.currentGas = 0f;
            }
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

