using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TargetItemManager : MonoBehaviour
{
    private List<TargetItemController> target = new List<TargetItemController>();
    public List<AbilityID> enableAblityIDs;       //�擾�����\�͂̓��X�g�ŊǗ�����
    public int TotalNum { get; private set; }

    public delegate void ManagerCallBack();
    public ManagerCallBack unlock;


    //�֐��Q====================================================================================
    //TIController�̏������i�R�[���o�b�N�֐���n���j
    public void Init(ManagerCallBack callBack)
    {
        foreach(Transform child in transform)
        {
            target.Add(child.GetComponent<TargetItemController>());
        }
        unlock = callBack;
        TotalNum = target.Count;
        TargetInit();
    }

    public bool IsZero()
    {
        if(TotalNum <= 0)
        {
            return true;
        }
        return false;
    }


    private void ControllerHandler(AbilityID id)
    {
        TotalNum = 0;
        enableAblityIDs.Clear();
        for(int i = 0;i < target.Count; i++)
        {
            if (!target[i].IsGet)
            {
                TotalNum++;
            }
            else
            {
                enableAblityIDs.Add(target[i].GetAbilityID());
            }
        }

        unlock();
    }

    private void TargetInit()
    {
        for (int i = 0; i < target.Count; i++)
        {
            target[i].Init(ControllerHandler);
        }

    }

    /*private void Update()
    {
        CountTargetNum();
    }*/
}
