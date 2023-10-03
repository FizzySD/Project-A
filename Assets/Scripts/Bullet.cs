using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class Bullet : MonoBehaviour
{
    private GameObject myRef;
    private float killTime2;
    private bool left = true;
    private Hero master;
    private int phase;
    private float killTime;
    private LineRenderer lineRenderer;
    private GameObject rope;
    private List<Vector3> nodes = new List<Vector3>();
    private Vector3 velocity = Vector3.zero;
    private Vector3 velocity2 = Vector3.zero;
    private bool isdestroying;

    private void Start()
    {
        rope = (GameObject)Object.Instantiate(Resources.Load("rope"));
        lineRenderer = rope.GetComponent<LineRenderer>();
        // GameManager.gameManager.AddHook(this);
    }

    public void RemoveMe()
    {
        this.isdestroying = true;
        Object.Destroy(rope);
        Object.Destroy(base.gameObject);
    }

    public LineRenderer GetHook()
    {
        return lineRenderer;
    }

    private void OnDestroy() 
    {
        // if (GameManager.gameManager != null)
        // {
        //     GameManager.gameManager.RemoveHook(this);
        // }
        Destroy(this.rope);
    }

    public bool IsHooked()
    {
        return phase == 1;
    }
    // private void KillObject()
    // {
    //     Object.Destroy(rope);
    //     Object.Destroy(base.gameObject);
    // }

    public void Disable()
    {
        if (transform.parent)
        {
            if (master)
            {
                master.bulletTimer = 0.5f;
            }
        }
        phase = 2;
        killTime = 0f;
        /*
        if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER)
        {
            object[] parameters = new object[1] { 2 };
            base.photonView.RPC("setPhase", PhotonTargets.Others, parameters);
        }
        */
    }
    public void Update()
    {
        if (this.isdestroying)
        {
            return;
        }
        if (this.master == null)
        {
            
            this.RemoveMe();
            return;
        }
        if (phase == 0)
        {
            SetLinePhase();
        }
        else if (phase == 1)
        {
            Vector3 vector = base.transform.position - myRef.transform.position;
            Vector3 vector2 = base.transform.position + myRef.transform.position;
            Vector3 vector3 = master.rb.velocity;
            float magnitude = vector3.magnitude;
            float magnitude2 = vector.magnitude;
            int value = (int)((magnitude2 + magnitude) / 5f);
            value = Mathf.Clamp(value, 2, 6);
            lineRenderer.positionCount = value;
            lineRenderer.SetPosition(0, myRef.transform.position);
            int i = 1;
            float num = Mathf.Pow(magnitude2, 0.3f);
            for (; i < value; i++)
            {
                int num2 = value / 2;
                float num3 = Mathf.Abs(i - num2);
                float f = ((float)num2 - num3) / (float)num2;
                f = Mathf.Pow(f, 0.5f);
                float num4 = (num + magnitude) * 0.0015f * f;
                lineRenderer.SetPosition(i, new Vector3(Random.Range(0f - num4, num4), Random.Range(0f - num4, num4), Random.Range(0f - num4, num4)) + myRef.transform.position + vector * ((float)i / (float)value) - Vector3.up * num * 0.05f * f - vector3 * 0.001f * f * num);
            }
            lineRenderer.SetPosition(value - 1, base.transform.position);
        }
        else if (phase == 2)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, base.transform.position);
            lineRenderer.SetPosition(1, myRef.transform.position);
            killTime += Time.deltaTime * 0.2f;
            lineRenderer.startWidth = 0.1f - killTime;
            lineRenderer.endWidth = 0.1f - killTime;
            if (killTime > 0.1f)
            {
                RemoveMe();
                return;
            }
        }
        else if (phase == 4)
        {
            base.gameObject.transform.position += velocity + velocity2 * Time.deltaTime;
            nodes.Add(new Vector3(base.gameObject.transform.position.x, base.gameObject.transform.position.y, base.gameObject.transform.position.z));
            Vector3 vector6 = myRef.transform.position - nodes[0];
            for (int k = 0; k <= nodes.Count - 1; k++)
            {
                lineRenderer.positionCount = nodes.Count;
                lineRenderer.SetPosition(k, nodes[k] + vector6 * Mathf.Pow(0.5f, k));
            }
            killTime2 += Time.deltaTime;
            if (killTime2 > 0.8f)
            {
                killTime += Time.deltaTime * 0.2f;
                lineRenderer.startWidth = 0.1f - killTime;
                lineRenderer.endWidth = 0.1f - killTime;
                if (killTime > 0.1f)
                {
                    RemoveMe();
                    return;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        /*
        if ((phase == 2 || phase == 1) && leviMode)
        {
            spiralcount++;
            if (spiralcount >= 60)
            {
                isdestroying = true;
                removeMe();
                return;
            }
        }
        */
        /*
        if (IN_GAME_MAIN_CAMERA.gametype != 0 && !base.photonView.isMine)
        {
            if (phase == 0)
            {
                base.gameObject.transform.position += velocity * Time.deltaTime * 50f + velocity2 * Time.deltaTime;
                nodes.Add(new Vector3(base.gameObject.transform.position.x, base.gameObject.transform.position.y, base.gameObject.transform.position.z));
            }
        }
        */
        if (phase != 0)
        {
            return;
        }
        base.gameObject.transform.position += (this.velocity * 50f + this.velocity2) * Time.fixedDeltaTime;
        LayerMask layerMask = 1 << LayerMask.NameToLayer("EnemyBox");
        LayerMask layerMask2 = 1 << LayerMask.NameToLayer("Ground");
        LayerMask layerMask3 = 1 << LayerMask.NameToLayer("NetworkObject");
        LayerMask layerMask4 = (int)layerMask | (int)layerMask2 | (int)layerMask3;
        bool flag = false;
        if ((nodes.Count <= 1) ? Physics.Linecast(nodes[^1], base.gameObject.transform.position, out RaycastHit hitInfo, layerMask4.value) : Physics.Linecast(nodes[^2], base.gameObject.transform.position, out hitInfo, layerMask4.value))
        {
            bool flag3 = true;
            Transform tf = hitInfo.collider.transform;
            if (tf.gameObject.layer == LayerMask.NameToLayer("EnemyBox"))
            {
                /*
                    if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER)
                    {
                        object[] parameters = new object[1] { hitInfo.collider.transform.root.gameObject.GetPhotonView().viewID };
                        base.photonView.RPC("tieMeToOBJ", PhotonTargets.Others, parameters);
                    }
                */
                master.lastHook = this;
                base.transform.parent = tf;
            }
            else if (tf.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                master.lastHook = null;
            }
            /*
            else if (hitInfo.collider.transform.gameObject.layer == LayerMask.NameToLayer("NetworkObject") && hitInfo.collider.transform.gameObject.tag == "Player" && !leviMode)
            {               
                if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER)
                {
                    object[] parameters2 = new object[1] { hitInfo.collider.transform.root.gameObject.GetPhotonView().viewID };
                    base.photonView.RPC("tieMeToOBJ", PhotonTargets.Others, parameters2);
                }
                master.GetComponent<HERO>().hookToHuman(hitInfo.collider.transform.root.gameObject, base.transform.position);
                base.transform.parent = hitInfo.collider.transform;
                master.GetComponent<HERO>().lastHook = null;
            }
            */
            else
            {
                flag3 = false;
            }

            if (flag3)
            {
                master.Launch(hitInfo.point, left, false);
                base.transform.position = hitInfo.point;
                if (phase != 2)
                {
                    phase = 1;
                    /*
                    if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER)
                    {
                        object[] parameters3 = new object[1] { 1 };
                        base.photonView.RPC("setPhase", PhotonTargets.Others, parameters3);
                        object[] parameters4 = new object[1] { base.transform.position };
                        base.photonView.RPC("tieMeTo", PhotonTargets.Others, parameters4);
                    }
                    if (leviMode)
                    {
                        getSpiral(master.transform.position, master.transform.rotation.eulerAngles);
                    }
                    */
                    flag = true;
                }
            }
        }
        nodes.Add(new Vector3(base.gameObject.transform.position.x, base.gameObject.transform.position.y, base.gameObject.transform.position.z));
        if (flag)
        {
            return;
        }
        killTime2 += Time.fixedDeltaTime;
        if (killTime2 > 0.8f)
        {
            phase = 4;
            /*
            if (IN_GAME_MAIN_CAMERA.gametype == GAMETYPE.MULTIPLAYER)
            {
                object[] parameters5 = new object[1] { 4 };
                base.photonView.RPC("setPhase", PhotonTargets.Others, parameters5);
            }
            */
            return;
        }
    }


    private void SetLinePhase()
    {
        if (master == null)
        {
            Object.Destroy(rope);
            Object.Destroy(base.gameObject);
            return;
        }
        if (this.nodes.Count <= 0)
        {
            return;
        }

        Vector3 vector = myRef.transform.position - nodes[0];
        lineRenderer.positionCount = nodes.Count;
        for (int i = 0; i <= nodes.Count - 1; i++)
        {
            lineRenderer.SetPosition(i, nodes[i] + vector * Mathf.Pow(0.75f, i));
        }
        if (nodes.Count > 1)
        {
            lineRenderer.SetPosition(1, myRef.transform.position);
        }
        
    }

    public void Launch(Vector3 v, Vector3 v2, string launcher_ref, bool isLeft, Hero hero, bool leviMode = false)
    {
        if (this.phase == 2)
        {
            return;
        }
        
        master = hero;
        velocity = v;
        float f = Mathf.Acos(Vector3.Dot(v.normalized, v2.normalized)) * 57.29578f;
        if (Mathf.Abs(f) > 90f)
        {
            velocity2 = Vector3.zero;
        }
        else
        {
            velocity2 = Vector3.Project(v2, v);
        }
        switch (launcher_ref)
        {
            case "hookRefL1":
                this.myRef = hero.hookRefL1;
                break;

            case "hookRefL2":
                this.myRef = hero.hookRefL2;
                break;

            case "hookRefR1":
                this.myRef = hero.hookRefR1;
                break;

            case "hookRefR2":
                this.myRef = hero.hookRefR2;
                break;
        }
        nodes = new List<Vector3>
        {
            myRef.transform.position
        };
        phase = 0;
        //this.leviMode = leviMode;
        left = isLeft;
        /*
        if (IN_GAME_MAIN_CAMERA.gametype != 0 && base.photonView.isMine)
        {
            object[] parameters = new object[2]
            {
                hero.GetComponent<HERO>().photonView.viewID,
                launcher_ref
            };
            base.photonView.RPC("myMasterIs", PhotonTargets.Others, parameters);
            object[] parameters2 = new object[3] { v, velocity2, left };
            base.photonView.RPC("setVelocityAndLeft", PhotonTargets.Others, parameters2);
        }
        */
        base.transform.SetPositionAndRotation(myRef.transform.position, Quaternion.LookRotation(v.normalized));        //SetSkin();
    }
}



