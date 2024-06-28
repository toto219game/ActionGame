using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalObject : MonoBehaviour
{
    public bool ReachGoal { get; private set; } = false;
    private bool goalEnable = false;


    public void Enable()
    {
        goalEnable = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (goalEnable)
        {
            ReachGoal = true;
        }
    }
}
