using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamerManager : MonoBehaviour
{
    [SerializeField] private FlockManager m_TargetFlock;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var centerofMass = m_TargetFlock.GetCenterofMass();
        
        transform.LookAt(centerofMass);
    }
}
