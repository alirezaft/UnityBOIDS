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

    [SerializeField] private float m_MovementSpeed;
    [SerializeField] private float m_NearbyDronesRadius;
    [SerializeField] private float m_CohesionRuleTreshold;
    [SerializeField] private float m_CohesionRuleCoefficient;
    [Range(0f, 1f)] [SerializeField] private float m_DirectionLowPassFilterCutoff;

    private float m_CohesionRuleWeight;
    private float m_AlignmentRuleWeight;
    private float m_SeperationRuleWeight;
    private float m_BorderAvoidanceruleWeight;

    private Vector3 m_SeperationDirection;
    private Vector3 m_AverageHeadingDirection;
    private Vector3 m_CenterofMassDirection;
    private Vector3 m_BorderAvoidanceDirection;

    private float m_FlockRadius;
    private Vector3 m_FlockOrigin;
    private Vector3 m_PreviousDirection;

    private void Awake()
    {
        m_NearDrones = new List<Vector3>();
        m_rigidbody = GetComponent<Rigidbody>();
        m_PreviousDirection = Vector3.zero;
    }

    private void Update()
    {
        CalculateAlignmentRule();
        CalculateCohesionRule();
        CalculateSeperationRule();
        CalculateborderAvoidanceRule();
        
        
        var direction = m_SeperationRuleWeight * m_SeperationDirection +
                        m_CohesionRuleWeight * m_CenterofMassDirection +
                        m_AlignmentRuleWeight * m_AverageHeadingDirection +
                        m_BorderAvoidanceruleWeight * m_BorderAvoidanceDirection
                        ;
        direction = Vector3.Lerp(direction, m_PreviousDirection, m_DirectionLowPassFilterCutoff);
        m_PreviousDirection = direction;
        
        var lookQuaternion = Quaternion.LookRotation(direction.normalized);
        transform.rotation = lookQuaternion;
        m_rigidbody.velocity = transform.forward * m_MovementSpeed;
    }


    public void SetBelongingFlock(FlockManager flock)
    {
        m_BelongingFlock = flock;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + m_SeperationDirection);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + m_AverageHeadingDirection);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + m_CenterofMassDirection);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + m_SeperationRuleWeight * m_SeperationDirection +
                                            m_CohesionRuleWeight * m_CenterofMassDirection +
                                            m_AlignmentRuleWeight * m_AverageHeadingDirection +
                                            m_BorderAvoidanceruleWeight * m_BorderAvoidanceDirection);
    }

    private void CalculateSeperationRule()
    {
        var seperationDirection = Vector3.zero;

        var nearbyDrones = Physics.OverlapSphere(transform.position, m_NearbyDronesRadius);

        foreach (var col in nearbyDrones)
        {
            if (transform.position.Equals(col.transform.position))
                continue;

            var dist = Vector3.zero;
            
            if(col.tag.Equals("Untagged"))
            {
                dist = (col.transform.position - transform.position).normalized;
                dist /= Vector3.Distance(col.transform.position, transform.position);
            }
            else if (col.tag.Equals("Obstacle"))
            {
                dist = (col.ClosestPoint(transform.position) - transform.position).normalized;
                dist /= Vector3.Distance(col.ClosestPoint(transform.position), transform.position);

            }
            
            // dist /= Vector3.Distance(col.transform.position, transform.position);
            seperationDirection -= dist;
        }

        m_SeperationDirection = seperationDirection.normalized;
        m_NearDrones.Clear();
    }

    private void CalculateAlignmentRule()
    {
        m_AverageHeadingDirection = (transform.position - m_BelongingFlock.GetAverageHeading().normalized).normalized;
    }

    private void CalculateCohesionRule()
    {
        var com = m_BelongingFlock.GetCenterofMass();
        m_CenterofMassDirection = (com - transform.position).normalized;

        if (Vector3.Distance(transform.position, com) <= m_CohesionRuleTreshold)
            m_CenterofMassDirection *= m_CohesionRuleCoefficient;
    }

    private void CalculateborderAvoidanceRule()
    {
        if (Vector3.Distance(transform.position, m_FlockOrigin) >= m_FlockRadius)
            m_BorderAvoidanceDirection = m_FlockOrigin - transform.position;

        m_BorderAvoidanceDirection = m_BorderAvoidanceDirection.normalized;
    }

    public void SetWeights(float seperation, float cohesion, float alignment, float borderAvoidance)
    {
        m_CohesionRuleWeight = cohesion;
        m_SeperationRuleWeight = seperation;
        m_AlignmentRuleWeight = alignment;
        m_BorderAvoidanceruleWeight = borderAvoidance;
    }

    public void SetFlockOrigin(Vector3 origin)
    {
        m_FlockOrigin = origin;
    }

    public void SetFlockRadius(float radius)
    {
        m_FlockRadius = radius;
    }
}