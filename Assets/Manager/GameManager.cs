using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //�V���O���g����
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

    private bool isPause;

    public bool GameClearFlag { get; private set; } = false;

    //�c���TargetItem��GameManager�p
    private int counter;

    private void Pause()
    {
        //timeScale��0�ɂ��đ���s�\�ɂ���
        Time.timeScale = 0;
        isPause = true;
        uiManager.DisplayPause();
        Cursor.lockState = CursorLockMode.None;
    }
    private void EndPause()
    {
        //timeScale��1�ɂ��đ���\�ɂ���
        Time.timeScale = 1;
        isPause = false;
        uiManager.HidePause();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void GameClear()
    {
        GameClearFlag = true;
        uiManager.DisplayClear();

        Cursor.lockState = CursorLockMode.None;
    }

    //�\�͂�Player�ɗ���
    private void FlowPlayerAbility()
    {
        foreach(AbilityID id in tiManager.enableAblityIDs)
        {
            player.UnlockPlayerAbility(id);
        }
    }

    List<int> from = new List<int>();
    List<int> to = new List<int>() { 99,98,97,100 };

    private void Start()
    {
        to = from;
        foreach(int n in from)
        {
            Debug.Log(n);
        }
        to.Add(5);
        foreach (int n in from)
        {
            Debug.Log(n);
        }
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
        if (counter <= 0)
        {
            GameClear();
        }
    }
}
