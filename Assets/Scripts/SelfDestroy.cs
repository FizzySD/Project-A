using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    public float CountDown = 5f;

    private void Start()
    {
    }

    private void Update()
    {
        CountDown -= Time.deltaTime;
        if (!(CountDown <= 0f))
        {
            return;
        }
        Object.Destroy(base.gameObject);
    }
}
