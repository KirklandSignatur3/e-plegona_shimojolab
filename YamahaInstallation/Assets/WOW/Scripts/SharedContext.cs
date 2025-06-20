using System.Diagnostics.Contracts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Unity.VisualScripting.FullSerializer;

[System.Serializable]
public class PlayerData
{
    //MM: replace with spread sheet type layout
    [SerializeField] public Player player;
    [SerializeField] public int make = 0;
    [SerializeField] public int makeTotal = 0;
    [SerializeField] public List<int> makeBuffer = new List<int>();
    [SerializeField] public int[] hitCount = new int[3] { 0, 0, 0 };
    [SerializeField] public int combo = 0;
    [SerializeField] public double lastNoteTap = 0;
}

[System.Serializable]
public class Context
{
    [SerializeField] public double BPM = 120d;
    [SerializeField] public int playerId = 0;
    [SerializeField] public DateTime startTime;
    [SerializeField] public int score = 0;
    [SerializeField] public float balance = 0f;
    [SerializeField] public Level level = Level.Level3;
    [SerializeField] public float[] levelBuffer = new float[4] { 0f, 0f, 0f, 0f };

    [SerializeField]
    public PlayerData[] playerDatas = new PlayerData[2]
    {
        new PlayerData{ player = Player.One },
        new PlayerData{ player = Player.Two }
    };
    [SerializeField] public int[,] playerColor = new int[2, 3] { { 0, 0, 0 }, { 0, 0, 0 } };
    [SerializeField] public int ballColor = 0;
    [SerializeField] public int musicChange = 0;
    [SerializeField] public List<int> scoreBuffer = new List<int>();
    [SerializeField] public List<float> balanceBuffer = new List<float>();

    public void ResetGameData()
    {
        score = 0;
        scoreBuffer = new List<int>();
        balance = 1f;
        balanceBuffer = new List<float>();
        level = Level.Level1;
        levelBuffer = new float[4] { 0f, 0f, 0f, 0f };
        ballColor = 0;
        musicChange = 0;
        playerDatas = new PlayerData[2]
        {
            new PlayerData{ player = Player.One },
            new PlayerData{ player = Player.Two }
        };
    }
}

public class SharedContext : SingletonMonoBehaviour<SharedContext>
{
    [SerializeField] public float bufferDuration = 1f;
    [SerializeField] public int makeBufferCount = 30;
    [SerializeField] public Context context = new Context();
    [SerializeField] TimerView timerView; // mm


    public double dur4Bars;
    public double durBars;
    public double durQuater;
    public double durEighth;

    bool isStart = false;

    private void Start()
    {
        var config = SharedConfig.Instance.config;

        durQuater = 60d / context.BPM;
        dur4Bars = durQuater * 16;
        durBars = durQuater * 4;
        durEighth = durQuater * 0.25;

        timerView.TimeLimit = config.gameDuration;
        timerView.Init();

        Debug.Log("duration 4Bars : " + dur4Bars);
        Debug.Log("duration Quater : " + durQuater);
        Debug.Log("duration Eighth : " + durEighth);
    }

    public void StartRecord()
    {
        timerView.StartTimer();
        InvokeRepeating("SetBuffer", 0, bufferDuration); //APPENDS BUFFER TO CSV EACH SECOND

        isStart = true;
    }

    public void StopRecord()
    {
        if (!isStart) return;
        isStart = false;
        CancelInvoke("SetBuffer");
        timerView.StopTimer();
    }

    private void Update()
    {
        if (isStart)
        {
            //levelの時間を記録.
            context.levelBuffer[(int)context.level] += Time.deltaTime;
        }
    }

    public void SetBuffer()
    {
        context.scoreBuffer.Add(context.score);
        context.balanceBuffer.Add(context.balance);

        context.playerDatas[(int)Player.One].makeBuffer.Add(context.playerDatas[(int)Player.One].make);
        while (context.playerDatas[(int)Player.One].makeBuffer.Count > makeBufferCount) context.playerDatas[(int)Player.One].makeBuffer.RemoveAt(0);
        context.playerDatas[(int)Player.One].make = 0;

        context.playerDatas[(int)Player.Two].makeBuffer.Add(context.playerDatas[(int)Player.Two].make);
        while (context.playerDatas[(int)Player.Two].makeBuffer.Count > makeBufferCount) context.playerDatas[(int)Player.Two].makeBuffer.RemoveAt(0);
        context.playerDatas[(int)Player.Two].make = 0;

        context.balance = ScoreCalculator.CalcBalance(context.playerDatas[(int)Player.One].makeBuffer, context.playerDatas[(int)Player.Two].makeBuffer);
		context.level = ScoreCalculator.CalcLevel(context.balance);
    }

    public void Save()
    {
        var filePath = Application.streamingAssetsPath + "/Context.json";
        JsonSerializeIO.Save<Context>(context, filePath);
    }

    public void Load()
    {
        var filePath = Application.streamingAssetsPath + "/Context.json";
        context = JsonSerializeIO.Load<Context>(filePath);
    }

    [ContextMenu("Export CSV")]
    public void ExportCSV()
    {
        List<string[]> table = new List<string[]>();

        //1/ セッション通し番号
        table.Add(new string[5] { "SessionID", String.Format("{0:000}", context.playerId), "", "", "" });

        //2/ タイムスタンプ（月日時分）
        table.Add(new string[5] { "DateTime", context.startTime.ToString("yyyy/MM/dd HH:mm:ss"), "", "", "" });

        //3/ 終了時の得点
        table.Add(new string[5] { "TotalScore", context.score.ToString(), "", "", "" });
        table.Add(new string[5] { "", "", "", "", "" });

        //4/ プレー中の各レベルの合計時間
        //例：計４分のプレーのうち、Level 1が計60秒、Level 2が計20秒、Level 3が計15秒…といった感じ
        table.Add(new string[5] { "Level(Sec)", "1", "2", "3", "4" });
        table.Add(new string[5] { "", context.levelBuffer[0].ToString(),
            context.levelBuffer[1].ToString(),
            context.levelBuffer[2].ToString(),
            context.levelBuffer[3].ToString() });
        table.Add(new string[5] { "", "", "", "", "" });

        //各プレイヤーのパフォーマンスにおけるPerfect/Great/Niceの割合
        table.Add(new string[5] { "Hit", "Good", "Nice", "Perfect", "Total" });
        var playData1 = context.playerDatas[(int)Player.One];
        table.Add(new string[5] { "Player1", playData1.hitCount[0].ToString(),
            playData1.hitCount[1].ToString(),
            playData1.hitCount[2].ToString(),
            playData1.makeTotal.ToString() });
        var playData2 = context.playerDatas[(int)Player.Two];
        table.Add(new string[5] { "Player2", playData2.hitCount[0].ToString(),
            playData2.hitCount[1].ToString(),
            playData2.hitCount[2].ToString(),
            playData2.makeTotal.ToString() });
        table.Add(new string[5] { "", "", "", "", "" });

        //5/ 確変タイムウィンドウ内の確変指標の変遷
        table.Add(new string[5] { "Balances", "", "", "", "" });
        for (int i = 0; i < context.balanceBuffer.Count; ++i)
            table.Add(new string[5] { "", context.balanceBuffer[i].ToString(), "", "", "" });
        table.Add(new string[5] { "", "", "", "", "" });

        //6/ 合計得点の変遷
        table.Add(new string[5] { "Scores", "", "", "", "" });
        for (int i = 0; i < context.scoreBuffer.Count; ++i)
            table.Add(new string[5] { "", context.scoreBuffer[i].ToString(), "", "", "" });

        string filePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) +
            "\\Log\\" + String.Format("{0:000}", context.playerId) + ".csv";
        Debug.Log(filePath);
        CSVWriter.SaveCSV(ConvertListToArray(table), filePath);
    }

    string[,] ConvertListToArray(List<string[]> list)
    {
        int numRows = list.Count;
        int numCols = list[0].Length;
        string[,] array = new string[numRows, numCols];

        for (int i = 0; i < numRows; i++)
        {
            for (int j = 0; j < numCols; j++)
            {
                array[i, j] = list[i][j];
            }
        }
        return array;
    }
}