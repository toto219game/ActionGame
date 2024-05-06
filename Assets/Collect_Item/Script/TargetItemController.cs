using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TargetItemController : MonoBehaviour
{
    /*public AblityID Ability { get; private set; }*/
    [SerializeField] private TargetItem tiInfo;

    public bool IsGet { get; private set; } = false;

    public delegate void TellTaken(AbilityID ticon);
    TellTaken tell;

    //‰Šú‰»ˆ—‚ğ“ü‚ê‚é
    public void Init(TellTaken func)
    {
        tell = func;
    }

    //”\—ÍID‚ğ“n‚·
    public AbilityID GetAbilityID()
    {
        return tiInfo.ability;
    }

    private void OnTriggerEnter(Collider other)
    {
        IsGet = true;
        tell(tiInfo.ability);
        gameObject.SetActive(false);
    }

    private void Start()
    {
        Instantiate(tiInfo.model, transform.position, Quaternion.identity, transform);
    }
}
