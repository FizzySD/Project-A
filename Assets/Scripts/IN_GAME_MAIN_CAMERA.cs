using System.Collections;
using UnityEngine;

internal class IN_GAME_MAIN_CAMERA : MonoBehaviour
{
    private float cameraDistance = 1.3f;
    private float distance = 10f;
    private Transform _transform;
    public GameObject main_object;
    public float distanceMulti;
    private float distanceOffsetMulti;
    public float MouseSensibility = 30;
    public Transform headPos;
    public static IN_GAME_MAIN_CAMERA instance;
    public enum RotationAxes
    {
        MouseXAndY = 0,
        MouseX = 1,
        MouseY = 2
    }
    public RotationAxes axes;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked; // Blocca il cursore all'interno della finestra di gioco.
        Cursor.visible = false;
        Cache();
        instance = this;
        base.name = "MainCamera";
    }

    private void Cache()
    {
        _transform = base.transform;
    }

    private void cameraMovement()
    {
        Camera camera = GetComponent<Camera>();
        distanceOffsetMulti = 1.3f;
        Vector3 horizontalDirection = _transform.forward;
        horizontalDirection.y = 0f; // Imposta la componente verticale a zero
        horizontalDirection.Normalize(); // Normalizza la direzione orizzontale

        _transform.position = ((headPos == null) ? main_object.transform.position : headPos.position);
        _transform.position += Vector3.up * 1;
        _transform.position -= Vector3.up * (0.6f - cameraDistance) * 2f;
        float num = MouseSensibility; // mouse speed

        float angle3 = Input.GetAxis("Mouse X") * 10f * num * Time.deltaTime;
        float num4 = (0f - Input.GetAxis("Mouse Y")) * 10f * num * (float)1 * Time.deltaTime;
        _transform.RotateAround(_transform.position, Vector3.up, angle3);
        float num5 = _transform.rotation.eulerAngles.x % 360f;
        float num6 = num5 + num4;
        if ((num4 <= 0f || ((num5 >= 260f || num6 <= 260f) && (num5 >= 80f || num6 <= 80f))) && (num4 >= 0f || ((num5 <= 280f || num6 >= 280f) && (num5 <= 100f || num6 >= 100f))))
        {
            _transform.RotateAround(_transform.position, _transform.right, num4);
        }
        _transform.position -= _transform.forward * distance * distanceMulti * distanceOffsetMulti;
        if (cameraDistance < 0.65f)
        {
            _transform.position += _transform.right * Mathf.Max((0.6f - cameraDistance) * 2f, 0.65f);
        }

    }

    private void Update()
    {
        cameraMovement();
    }


}
