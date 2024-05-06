using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridLine : MonoBehaviour
{
    LineRenderer line;
    [SerializeField] private float gridSpace = 2f;
    [SerializeField] private int gridCount = 50;
    [SerializeField] private float gridOffset = -100f;

    private void Start()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = (gridCount + 1) * 4;

        for (int i = 0; i <= gridCount; i++)
        {
            Vector3[] gridVertex1 = new Vector3[]
            {
                new Vector3(gridOffset + gridSpace * i,0f,gridOffset),
                new Vector3(gridOffset + gridSpace * i, 0f, gridOffset + gridSpace * gridCount)
            };
            Vector3[] gridVertex2 = new Vector3[]
            {
                new Vector3(gridOffset,0f,gridOffset + gridSpace * i),
                new Vector3(gridOffset + gridSpace * gridCount,0f,gridOffset + gridSpace * i)
            };
            line.SetPositions(gridVertex1);
            line.SetPositions(gridVertex2);

        }
        /* Vector3[] gridVertex1 = new Vector3[]
             {
                 new Vector3(-10f,0f,0f),
                 new Vector3(10f, 0f,0f)
             };
         line.SetPositions(gridVertex1);*/
    }
}
