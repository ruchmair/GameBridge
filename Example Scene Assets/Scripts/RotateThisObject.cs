using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Rigidbody))]
public class RotateThisObject : MonoBehaviour
{

    public float x_Rotation;
    public float y_Rotation;
    public float z_Rotation;


    void Update()
    {
        transform.Rotate(new Vector3(
            (x_Rotation > 0) ? x_Rotation * Time.deltaTime : 0,
            (y_Rotation > 0) ? y_Rotation * Time.deltaTime : 0,
             (z_Rotation > 0) ? z_Rotation * Time.deltaTime : 0));
    }
}
