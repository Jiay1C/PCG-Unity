using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FlockConfig", menuName = "FlockConfig")]
public class FlockConfig : ScriptableObject
{
    public string type;
    public uint count;
    public float maxForce;
    public float minSpeed;
    public float maxSpeed;
    public float maxAngularSpeed;
    public float angularDrag;
    public uint nearbyObjectCount;
    public bool enableWanderForce;
    public float coefWanderForce;
    public float maxWanderForce;
    public bool enableFlockCenterForce;
    public float coefFlockCenterForce;
    public float maxFlockCenterForce;
    public bool enableCollisionAvoidForce;
    public float coefCollisionAvoidForce;
    public float maxCollisionAvoidForce;
    public float maxCollisionAvoidRadius;
    public bool enableVelocityMatchForce;
    public float coefVelocityMatchForce;
    public float maxVelocityMatchForce;
    public bool enableEnvironmentForce;
    public float coefEnvironmentForce;
    public float maxEnvironmentDistance;
    public bool enableTrail;
    public Material trailMaterial;
    public float trailTime;
    public float trailWidth;
}