using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TargetItemManager : MonoBehaviour
{
    private List<TargetItemController> target = new List<TargetItemController>();
    public List<AbilityID> enableAblityIDs;       //取得した能力はリストで管理する
    public int TotalNum { get; private set; }

    public delegate void AbilityCallBack();
    private AbilityCallBack unlockAbility;


    //関数群====================================================================================
    //TIControllerの初期化（コールバック関数を渡す）
    public void Init(AbilityCallBack callBack)
    {
        foreach(Transform child in transform)
        {
            target.Add(child.GetComponent<TargetItemController>());
        }
        unlockAbility = callBack;
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

        unlockAbility();
    }

    private void TargetInit()
    {
        for (int i = 0; i < target.Count; i++)
        {
            target[i].Init(ControllerHandler);
        }

    }
}
