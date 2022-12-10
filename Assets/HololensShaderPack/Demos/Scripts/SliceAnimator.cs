using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Continuously animates the sliceplane shader variables of the specified Material 
/// </summary>
public class SliceAnimator : MonoBehaviour
{
    [Tooltip("The Material with sliceplane to animate")]
    public Material SurfaceMaterial;

    [Tooltip("Animation speed in meters per second")]
    public float Speed = 1.0f;

    [Tooltip("Animation speed in meters per second")]
    public float Offset = -1.0f;

    [Tooltip("The maximum distance from the gaze hitpoint in meters.")]
    public float Range = 1.0f;

    void Update()
    {
        if (SurfaceMaterial)
        {
            float planeOffset = Mathf.PingPong(Speed * Time.time, Range) + Offset;
            float angle = Mathf.PingPong(10 * Time.time, 60) - 60;

            var slicePlaneNormal = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.up;

            SurfaceMaterial.SetVector("_SlicePlane", new Vector4(slicePlaneNormal.x, slicePlaneNormal.y, slicePlaneNormal.z, planeOffset));
        }
    }
}
