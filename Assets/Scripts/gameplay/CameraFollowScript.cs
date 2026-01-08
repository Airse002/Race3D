using UnityEngine;

public class AircraftChaseCamera : MonoBehaviour
{
    public Transform target;

    [Header("Offset in target local space")]
    public Vector3 localOffset = new Vector3(0f, 3.5f, -12f);

    [Header("Smoothing")]
    public float positionSmoothTime = 0.12f;
    public float rotationLerp = 10f;

    [Header("Look")]
    public float lookAhead = 10f;

    [Range(0f, 1f)]
    public float rollFollow = 0.6f; // 0 = kamera rovně, 1 = plně se naklání s lodí

    Vector3 posVel;

    void LateUpdate()
    {
        if (!target) return;

        // pozice: za lodí v jejím lokálním prostoru
        Vector3 desiredPos = target.TransformPoint(localOffset);
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref posVel, positionSmoothTime);

        // kamera kouká dopředu
        Vector3 lookPoint = target.position + target.forward * lookAhead;
        Vector3 forwardToLook = (lookPoint - transform.position).normalized;

        // up v kameře: blend mezi světovým up a up lodě (kvůli roll follow)
        Vector3 upDir = Vector3.Slerp(Vector3.up, target.up, rollFollow);

        Quaternion desiredRot = Quaternion.LookRotation(forwardToLook, upDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotationLerp * Time.deltaTime);
    }
}
