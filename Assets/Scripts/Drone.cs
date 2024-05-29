using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Drone : MonoBehaviour
{
    private List<Vector3> m_NearDrones;
    private FlockManager m_BelongingFlock;
    private Rigidbody m_rigidbody;

    [FormerlySerializedAs("m_SteeringSpeed")] [Range(0, 90f)] [SerializeField]
    private float m_SteeringCoefficient;

    [SerializeField] private float m_MovementSpeed;
    [SerializeField] private float m_NearbyDronesRadius;
    [SerializeField] private float m_CohesionRuleTreshold;
    [SerializeField] private float m_CohesionRuleCoefficient;

    private float CohesionRuleWeight;
    private float AlignmentRuleWeight;
    private float SeperationRuleWeight;

    private Vector3 m_SeperationDirection;
    private Vector3 m_AverageHeadingDirection;
    private Vector3 m_CenterofMassDirection;

    private void Awake()
    {
        m_NearDrones = new List<Vector3>();
        m_rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        CalculateAlignmentRule();
        CalculateCohesionRule();
        CalculateSeperationRule();

        var direction = SeperationRuleWeight * m_SeperationDirection + CohesionRuleWeight * m_CenterofMassDirection +
                        AlignmentRuleWeight * m_AverageHeadingDirection;

        // direction = (direction - m_AverageHeadingDirection) / m_SteeringCoefficient;


        // transform.LookAt(direction);
        var lookQuaternion = Quaternion.LookRotation(direction.normalized);
        transform.rotation = lookQuaternion;
        m_rigidbody.velocity = transform.forward * m_MovementSpeed;
    }


    public void SetBelongingFlock(FlockManager flock)
    {
        m_BelongingFlock = flock;
    }

    // private void OnTriggerEnter(Collider other)
    // {
    //     m_NearDrones.Add((other.transform.position - transform.position).normalized);
    // }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, m_NearbyDronesRadius);
    }

    private void CalculateSeperationRule()
    {
        var seperationDirection = Vector3.zero;

        var nearbyDrones = Physics.OverlapSphere(transform.position, m_NearbyDronesRadius);
        Debug.Log(nearbyDrones.Length);

        foreach (var drone in nearbyDrones)
        {
            var dist = (drone.transform.position - transform.position).normalized;
            seperationDirection -= dist;
        }

        m_SeperationDirection = seperationDirection.normalized;
        m_NearDrones.Clear();
    }

    private void CalculateAlignmentRule()
    {
        m_AverageHeadingDirection = (transform.position - m_BelongingFlock.GetAverageHeading().normalized).normalized;
        // Debug.Log(m_BelongingFlock.GetAverageHeading());
    }

    private void CalculateCohesionRule()
    {
        var com = m_BelongingFlock.GetCenterofMass();
        m_CenterofMassDirection = (com - transform.position).normalized;

        if (Vector3.Distance(transform.position, com) <= m_CohesionRuleTreshold)
            m_CenterofMassDirection *= m_CohesionRuleCoefficient;
    }

    public void SetWeights(float seperation, float cohesion, float alignment)
    {
        CohesionRuleWeight = cohesion;
        SeperationRuleWeight = seperation;
        AlignmentRuleWeight = alignment;
    }
}