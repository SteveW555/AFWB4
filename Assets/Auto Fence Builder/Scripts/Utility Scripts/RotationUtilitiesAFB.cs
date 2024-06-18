using UnityEngine;

public class RotationUtilitiesAFB : MonoBehaviour
{
    public static Vector3 RotateVectorAroundPoint(Vector3 vec, Vector3 pivotPoint, Vector3 angles)
    {
        //=============================================================
        //===       Note: All 3 Methods give the same results       ===
        //=============================================================

        Vector3 rotatedVector = Vector3.zero;

        //---  Method 1 ---
        Vector3 dir = vec - pivotPoint; // get point direction relative to pos
        dir = Quaternion.Euler(angles) * dir; // rotate it
        rotatedVector = dir + pivotPoint; // calculate rotated point

        //---  Method 2 ---
        //rotatedVector = Quaternion.Euler(angles) * vec;

        //---  Method 3 ---
        //rotatedVector = Quaternion.AngleAxis(angles.z, new Vector3(0, 0, 1)) * vec;
        //rotatedVector = Quaternion.AngleAxis(angles.x, new Vector3(1, 0, 0)) * rotatedVector;
        //rotatedVector = Quaternion.AngleAxis(angles.y, new Vector3(0,1,0)) * rotatedVector;

        return rotatedVector;
    }
}