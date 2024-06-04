
using UnityEngine;

public enum AbilityID
{
    blink,
    sliding,
    grapHook,
    wallJump,
    doubleJump
}

[CreateAssetMenu(fileName ="TargetItem_instance",menuName ="ScriptableObject/TargetItem")]
public class TargetItem : ScriptableObject
{
    [SerializeField] public GameObject model;
    [SerializeField] public AbilityID ability;
    [SerializeField] public Transform spawnPoint;
}
