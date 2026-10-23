using UnityEngine;

[CreateAssetMenu(menuName = "Jordan World/Sandbox/Car Config", fileName = "CarConfig")]
public class CarConfig : ScriptableObject
{
    [Header("Driving")]
    public float motorForce = 1500f;
    public float steerTorque = 400f;
    public float maxSpeed = 25f;

    [Header("Physics")]
    public float groundLinearDamping = 0.5f;
    public float groundAngularDamping = 2f;
    public Vector3 centerOfMass = new Vector3(0f, -0.5f, 0f);
}
