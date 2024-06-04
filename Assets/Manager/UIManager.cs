using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer
{
    private float deltaTime = 0f;
    private int msec = 0;
    private int sec = 0;
    private int min = 0;

    public void Init()
    {
        deltaTime = 0f;
    }

    public string UpdateTime()
    {
        deltaTime += Time.deltaTime;

        min = Mathf.FloorToInt(deltaTime / 60f);
        sec = Mathf.FloorToInt(deltaTime % 60f);
        msec = Mathf.FloorToInt((deltaTime - 60f * min - sec) * 100);

        return string.Format("{0:00}:{1:00}.{2:00}", min, sec, msec);
    }
}

public class UIManager : MonoBehaviour
{
    //シングルトン化
    #region
    public static UIManager Instance;
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

    [SerializeField] private TextMeshProUGUI targetItemCounter;
    [SerializeField] private Canvas clearCanvas;
    [SerializeField] private Canvas pauseCanvas;

    private Timer timer;
    [SerializeField] private TextMeshProUGUI timeCounter;


    //Mainシーンでの要素
    //タイマーの更新
    public void TimerUpdate()
    {
        timeCounter.text = timer.UpdateTime();
    }

    //残り個数を代入
    public void CounterUpdate(int num)
    {
        targetItemCounter.text = num.ToString();
    }

    //pause画面表示
    public void DisplayPause()
    {
        pauseCanvas.gameObject.SetActive(true);
    }

    //pause画面非表示
    public void HidePause()
    {
        pauseCanvas.gameObject.SetActive(false);
    }

    //クリア表示
    public void DisplayClear()
    {
        clearCanvas.gameObject.SetActive(true);
    }

    public void UIManagerInit()
    {
        timer = new Timer();
        timer.Init();
    }
}
