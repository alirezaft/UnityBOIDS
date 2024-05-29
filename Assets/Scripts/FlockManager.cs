using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class FlockManager : MonoBehaviour
{
    [SerializeField] private int m_DroneNumber;
    [SerializeField] private GameObject m_DronePrefab;
    [SerializeField] private float m_FlockRadius;
    
    [SerializeField] [Range(0f, 1f)] private float CohesionRuleWeight;
    [SerializeField] [Range(0f, 1f)] private float AlignmentRuleWeight;
    [SerializeField] [Range(0f, 1f)] private float SeperationRuleWeight;
    
    private Drone[] m_Drones;

    private Vector3 m_AverageHeading;
    private Vector3 m_CenterOfMass;

    private void Awake()
    {
        m_Drones = new Drone[m_DroneNumber];
    }

    void Start()
    {
        for (int i = 0; i < m_DroneNumber; i++)
        {
            var dposition = Random.insideUnitSphere * m_FlockRadius;
            var drotation = Random.insideUnitSphere * 20;
            var drone = Instantiate(m_DronePrefab, dposition, Quaternion.Euler(drotation), transform).GetComponent<Drone>();
            drone.SetBelongingFlock(this);
            drone.SetWeights(SeperationRuleWeight, CohesionRuleWeight, AlignmentRuleWeight);
            m_Drones[i] = drone;
        }    
    }

    // Update is called once per frame
    void Update()
    {
        var sumDronePosition = Vector3.zero;
        var sumHeading = Vector3.zero;
        
        foreach (var drone in m_Drones)
        {
            var dtransform = drone.transform;
            sumDronePosition += dtransform.position;
            sumHeading += dtransform.forward.normalized;
        }

        m_CenterOfMass = sumDronePosition / m_DroneNumber;
        m_AverageHeading = sumHeading / m_DroneNumber;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(m_CenterOfMass, 1);
    }

    public Vector3 GetAverageHeading()
    {
        return m_AverageHeading;
    }

    public Vector3 GetCenterofMass()
    {
        return m_CenterOfMass;
    }
}
