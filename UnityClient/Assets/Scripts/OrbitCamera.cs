using UnityEngine;
using System.Collections;
 
[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class OrbitCamera: MonoBehaviour {
 
    Camera camera;
    public Transform target;
    public Transform origTarget;

    public float distance = 30.0f;
    public float xSpeed = 90.0f;
    public float ySpeed = 60.1f;
    public float scrollSpeed = 50.0f;
 
    public float yMinLimit = 15f;
    public float yMaxLimit = 80f;
 
    public float distanceMin = 10f;
    public float distanceMax = 65f ;
 
    private Rigidbody rigidbody;

    public bool click = false;
    public bool demo  = false;
 
    float x = 0.0f;
    float y = 20.0f;

 
    // Use this for initialization
    void Start () 
    {
        origTarget = target;
        this.camera = Camera.main;
        Vector3 angles = transform.eulerAngles;

        if (demo){
           x = angles.y;
           y = 35.264f;
           distance = 25f;
        } else {
           x = angles.y;
           y = angles.x;
        }

        rigidbody = GetComponent<Rigidbody>();
 
        // Make the rigid body not change rotation
        if (rigidbody != null)
        {
            rigidbody.freezeRotation = true;
        }
    }

    void Update() {
        if (!target.transform) {
            target.transform.SetParent(origTarget.transform);
            target.transform.localPosition = new Vector3(0,0,0);
        }
    }
 
    void LateUpdate () 
    {
        if (Input.GetMouseButtonDown(2)) {
            click = true;
        }

        if (Input.GetMouseButtonUp(2)) {
            click = false;
        }


        if (!target) 
        {
            return;
        }

        if (click) {
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
        }

        y = ClampAngle(y, yMinLimit, yMaxLimit);

        if (demo){
           x += 0.025f;
        }
 
        Quaternion rotation = Quaternion.Euler(y, x, 0);

        distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel")*scrollSpeed, distanceMin, distanceMax);
        this.camera.nearClipPlane = distance / 25;
        //this.camera.nearClipPlane = 25;
        
        this.scrollSpeed = distance;

        RaycastHit hit;
        //Why was this here? Causes render glitches at min distance
        //if (Physics.Linecast (target.position, transform.position, out hit)) 
        //{
        //    distance -=  hit.distance;
        //}
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + target.position;

        transform.rotation = rotation;
        transform.position = position;
    }
 
    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
