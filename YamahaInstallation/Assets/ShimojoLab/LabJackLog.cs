using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LabJack.LabJackUD;
using System.IO;
using System;
//using static UnityEditor.Timeline.TimelinePlaybackControls;


public class LabJackLog : MonoBehaviour
{

    //LabJackU3
    private U3 u3;
    public float signalDelay = .005f;

    public string filepath = "";
    public string filename = "";
    public string file = "";

    public int P1_CREATE_NOTE = 1;
    public int P2_CREATE_NOTE = 2; 
    public int P1_MISS_NOTE = 3;
    public int P2_MISS_NOTE = 4;
    public int P1_GOOD = 5;
    public int P1_NICE = 6;
    public int P1_PERFECT = 7;
    public int P2_GOOD = 8;
    public int P2_NICE = 9;
    public int P2_PERFECT = 10; 
    public int START_GAME = 11;

    public List<string> events = new List<string> { "time", "P1_CREATE_NOTE", "P2_CREATE_NOTE", "P1_MISS_NOTE", 
        "P2_MISS_NOTE", "P1_GOOD", "P1_NICE", "P1_PERFECT", "P2_GOOD", "P2_NICE", "P2_PERFECT", "START_GAME", }; 

    private TextWriter tw;

    /*    public int BG_CHANGE_BLACK = 12;
        public int AEP = 13;*/

    void Awake()
    {
        /// make filename
        filename = "ePlegona_data";
        //filepath = "Desktop";
        //filepath = "C:\\Users\\Yamaha\\Desktop\\Shimojo_Log";
        filepath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\Log";

        file = filepath + "\\" + filename + "_" + DateTime.Now.ToString("MMM-dd-yyyy,hh.mm.ss") + ".csv";
        Debug.Log("file name: " + file);

        TextWriter tw = new StreamWriter(file, false); //false bc we are wiping the file?

        //tw.WriteLine("time, player Y, p1 jump, p2 jump, p1 off time press, p2 off time press, hit obstacle, pass obstacle, " +
        //            "start game, end game, coin Y, bg_change_white, bg_change_black, AEP");    //writing the headings
        tw.WriteLine("time, P1_CREATE_NOTE, P2_CREATE_NOTE, P1_MISS_NOTE," +
            " P2_MISS_NOTE, P1_GOOD, P1_NICE, P1_PERFECT, P2_GOOD, P2_NICE, P2_PERFECT, START_GAME");    ///writing the headings



        tw.Close();
        Debug.Log("csv created");
        /*        TextWriter tw = new StreamWriter(filename, true);
        */
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SendLabJackSignal(float delay, int pin)
    {
        u3 = new U3(LJUD.CONNECTION.USB, "0", true);
        LJUD.ePut(u3.ljhandle, LJUD.IO.PIN_CONFIGURATION_RESET, 0, 0, 0);
        LJUD.eDO(u3.ljhandle, pin, 1);

        StartCoroutine(SendLabJackSignalHelper(delay, pin));

    }

    private IEnumerator SendLabJackSignalHelper(float delay, int pin)
    {
        yield return new WaitForSeconds(delay);
        LJUD.eDO(u3.ljhandle, pin, 0);



    }

    public void LogEvent(float time, int eventType)
    {
        Debug.Log("in log event code");
        TextWriter tw = new StreamWriter(file, true); // had to add this because unity said something was null

        switch (eventType)
        {
            case 1: // p1 create note
                tw.WriteLine(time + ", " + "1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0");    //writing the headings 
                tw.Close();
                Debug.Log(events[eventType]);
                SendLabJackSignal(signalDelay, eventType - 1);
                break;
            case 2: // p2 create note
                tw.WriteLine(time + ", " + "0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0");
                tw.Close();

                Debug.Log(events[eventType]);
                SendLabJackSignal(signalDelay, eventType - 1);

                break;
            case 3: // p1 miss note
                tw.WriteLine(time + ", " + "0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0");
                tw.Close();

                Debug.Log(events[eventType]);
                SendLabJackSignal(signalDelay, eventType - 1);

                break;
            case 4: // p2 miss note
                tw.WriteLine(time + ", " + "0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0");
                tw.Close();

                Debug.Log(events[eventType]);
                SendLabJackSignal(signalDelay, eventType - 1);

                break;
            case 5: // p1 hit: good nice perfect
                tw.WriteLine(time + ", " + "0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0");
                tw.Close();

                Debug.Log(events[eventType]);
                SendLabJackSignal(signalDelay, 4);

                break;
            case 6: // p1 nice
                tw.WriteLine(time + ", " + "0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0");
                tw.Close();

                Debug.Log(events[eventType]);
                SendLabJackSignal(signalDelay, 4);

                break;
            case 7: //p1 perfect

                tw.WriteLine(time + ", " + "0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0");
                tw.Close();

                Debug.Log(events[eventType]);
                SendLabJackSignal(signalDelay, 4);

                break;
            case 8: // p2 hit: good 
                tw.WriteLine(time + ", " + "0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0");
                tw.Close();

                Debug.Log(events[eventType]);
                SendLabJackSignal(signalDelay, 5);

                break;
            case 9: // p2 nice
                tw.WriteLine(time + ", " + "0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0");
                tw.Close();
                Debug.Log(events[eventType]);
                SendLabJackSignal(signalDelay, 5);

                break;
            case 10: // p2 perfect
                tw.WriteLine(time + ", " + "0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0");
                tw.Close();
                Debug.Log(events[eventType]);
                SendLabJackSignal(signalDelay, 5);

                break;
            case 11: // start game
                tw.WriteLine(time + ", " + "0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0");
                tw.Close();
                Debug.Log(events[eventType]);
                SendLabJackSignal(signalDelay, 6);
                SendLabJackSignal(signalDelay, 7);

                break;
            case 12: // BG_CHANGE_BLACK
                tw.WriteLine(time + ", " + "0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0");
                tw.Close();
                Debug.Log(events[eventType]);
                SendLabJackSignal(signalDelay, eventType - 1);

                break;
            case 13: // AEP
                tw.WriteLine(time + ", " + "0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1");
                tw.Close();
                Debug.Log(events[eventType]);
                SendLabJackSignal(signalDelay, eventType - 1);

                break;
        }


    }


    public void Finish()
    {
        tw.Close();
    }

}
