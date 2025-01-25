using System;
using UnityEngine;

public class PlayerBubble : MonoBehaviour
{
    [SerializeField] 
    private int pointCount = 32;
    [SerializeField] 
    private float radius = 1f;
    [SerializeField]
    private float radialForce = 1f;
    [SerializeField]
    private float neighborForce = 1f;
    [SerializeField]
    private float dampening = 1f;
    [SerializeField]
    private Rigidbody pointPrefab;
    
    [Header("Blower")]
    [SerializeField] 
    private Transform blower;
    [SerializeField]
    private float blowerForce = 1f;

    [Header("Camera")] 
    [SerializeField] 
    private Transform camera;
    [SerializeField] 
    private float cameraSmoothing = 1f;

    [Header("Breakage")] 
    [SerializeField] 
    private float breakingPoint = 100f;
    
    private LineRenderer line;
    
    private Rigidbody[] points;
    
    private float targetPointDistance;
    private float defaultAngle;

    private float strain;
    
    void Awake() {
        line = GetComponent<LineRenderer>();
        line.positionCount = pointCount + 1;
        
        points = new Rigidbody[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            points[i] = Instantiate(pointPrefab);
            points[i].position =new Vector3(Mathf.Sin(i * Mathf.PI * 2 / pointCount) * radius, 0f,
                Mathf.Cos(i * Mathf.PI * 2 / pointCount) * radius);
        }

        targetPointDistance = Vector3.Magnitude((Vector3.forward * radius) - (Quaternion.AngleAxis(360f / pointCount, Vector3.up) * Vector3.forward * radius));
    }
    
    
    void FixedUpdate()
    {
        Vector3 centerPoint = Vector3.zero;
        strain = 0;
        for (int i = 0; i < pointCount; i++)
        {
            line.SetPosition(i, points[i].position);
            centerPoint += points[i].position;
        }
        line.SetPosition(pointCount, points[0].position);
        centerPoint /= pointCount;
        for (int i = 0; i < pointCount; i++)
        {
            Vector3 centerDirection = points[i].position - centerPoint;
            float centerDistance = Vector3.Magnitude(centerDirection);
            points[i].AddForce(-centerDirection.normalized * Mathf.Min((centerDistance - radius) * radialForce, 0f));
            Vector3 previousDirection = points[i].position - points[(i + pointCount - 1) % pointCount].position;
            points[i].AddForce(-previousDirection.normalized * ((previousDirection.magnitude - targetPointDistance) * neighborForce));
            Vector3 nextDirection = points[i].position - points[(i + 1) % pointCount].position;
            points[i].AddForce(-nextDirection.normalized * ((nextDirection.magnitude - targetPointDistance) * neighborForce));
            
            Vector3 dampenVelocity = (points[(i + pointCount - 1) % pointCount].linearVelocity + points[(i + 1) % pointCount].linearVelocity) / 2;
            points[i].AddForce((dampenVelocity - points[i].linearVelocity) * dampening);
        }
        
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity,
                LayerMask.GetMask("CameraRaycast"))) 
        {
            blower.position = hit.point;
            if (Input.GetMouseButton(0))
            {
                Vector3 blowerDirection = (centerPoint - hit.point).normalized;
                for (int i = 0; i < pointCount; i++)
                {
                    Vector3 offset = blower.position - points[i].position;
                    float blowerMagnitude = offset.magnitude;
                    blowerMagnitude = Mathf.Min(1 / (blowerMagnitude * blowerMagnitude), 1);
                    points[i].AddForce(blowerDirection * (blowerForce * blowerMagnitude));
                }
            }
        }
        
        camera.position = Vector3.Lerp(camera.position, centerPoint + Vector3.up * 20, Time.deltaTime);
        transform.position = centerPoint;
    }

    private void OnTriggerEnter(Collider other)
    {
        dampening = 55;
        Debug.Log(other.name);
    }
}
