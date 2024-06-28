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
    [SerializeField] private GoalObject goal;

    private bool isPause;

    public bool GameClearFlag { get; private set; } = false;

    //残りのTargetItem個数GameManager用
    private int counter;

    //関数群=================================================================
    

    private void Pause()
    {
        //timeScaleを0にして操作不能にする
        Time.timeScale = 0;
        isPause = true;
        uiManager.DisplayPause();
        Cursor.lockState = CursorLockMode.None;
    }
    private void EndPause()
    {
        //timeScaleを1にして操作可能にする
        Time.timeScale = 1;
        isPause = false;
        uiManager.HidePause();
        Cursor.lockState = CursorLockMode.Locked;
    }

    //ゲームをクリア状態にする
    private void GameClear()
    {
        GameClearFlag = true;
        uiManager.DisplayClear();

        Cursor.lockState = CursorLockMode.None;
    }

    //クリア状態にするかどうかを検知する関数
    private void DetectGameClear()
    {
        if (counter <= 0 && goal.ReachGoal == true)
        {
            GameClear();
        }
    }

    //ゴールできる状態にする関数、できれば個数が一回だけの実行にしたい
    private void UnlockGoal()
    {
        if(counter <= 0)
        {
            goal.Enable();
        }
    }

    //能力をPlayerに流す
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
        tiManager.Init(FlowPlayerAbility);
        uiManager.UIManagerInit();
        uiManager.CounterUpdate(tiManager.TotalNum);
    }

    private void Update()
    {

        counter = tiManager.TotalNum;

        if (GameClearFlag) return;

        
        if (isPause)
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                EndPause();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                Pause();
            }
            uiManager.CounterUpdate(counter);
            uiManager.TimerUpdate();
        }
        
    }

    private void LateUpdate()
    {
        UnlockGoal();
        DetectGameClear();
    }
}
