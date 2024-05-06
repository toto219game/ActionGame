using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //シングルトン化
    #region
    public static GameManager Instance;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    [SerializeField] private PlayerController player;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private TargetItemManager tiManager;

    public bool GameClearFlag { get; private set; } = false;

    //残りのTargetItem個数GameManager用
    private int counter;

    private void GameClear()
    {
        GameClearFlag = true;
        uiManager.DisplayClear();

        Cursor.lockState = CursorLockMode.None;
    }

    private void FlowPlayerAbility()
    {
        foreach(AbilityID id in tiManager.enableAblityIDs)
        {
            player.UnlockPlayerAbility(id);
        }
    }


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        uiManager.UIManagerInit();
        uiManager.CounterUpdate(tiManager.TotalNum);
        tiManager.Init(FlowPlayerAbility);
    }

    private void Update()
    { 

        counter = tiManager.TotalNum;
        if (!GameClearFlag)
        {
            uiManager.CounterUpdate(counter);
            uiManager.TimerUpdate();
        }
    }

    private void LateUpdate()
    {
        if (counter <= 0)
        {
            GameClear();
        }
    }
}
