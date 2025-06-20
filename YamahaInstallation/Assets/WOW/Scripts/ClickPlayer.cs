using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using System;
using OscJack;
using TMPro;
using UniRx;
using System.Globalization;

public class ClickPlayer : SingletonMonoBehaviour<ClickPlayer>
{
    [SerializeField] int port = 4587;
    [SerializeField] public UnityEvent onClickEvent = new UnityEvent();
    [SerializeField] TextMeshProUGUI dumpText;
    OscServer oscServer;
    DateTime startDateTime;
    DateTime dateTime;
    DateTime midiDateTime = new DateTime();
    TimeSpan timeSpan = new TimeSpan();
    public TimeSpan TimeSpan
	{
		get { return timeSpan; }
	}
    int bars = 0;
    int beats = 0;
    int sixteenths = 0;


    [SerializeField] public float beatReaction = 0;
    SingleAssignmentDisposable disposable = null;
    
    [SerializeField] public bool receiveMTC = true;
    
    private void OnDestroy() 
	{
        disposable?.Dispose();
		oscServer?.Dispose();	
	}

    void Start()
    {
        dateTime = DateTime.Now;
        startDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0);
        oscServer = new OscServer(port);
        oscServer.MessageDispatcher.AddCallback(
		  	"time",
		  	(string address, OscDataHandle data) =>
		  	{
                if( receiveMTC)
                {
                    midiDateTime = new DateTime( dateTime.Year, dateTime.Month, dateTime.Day, data.GetElementAsInt(0), data.GetElementAsInt(1),
                        data.GetElementAsInt(2),(int)(Convert.ToDouble(data.GetElementAsInt(3)) * 33.33333333333333333));
                    dateTime = DateTime.Now;
                }
            });
        oscServer.MessageDispatcher.AddCallback(
		  	"beats",
		  	(string address, OscDataHandle data) =>
		  	{
                bars = data.GetElementAsInt(0);
                beats = data.GetElementAsInt(1);
                sixteenths = data.GetElementAsInt(2);
            });

        disposable = new SingleAssignmentDisposable();
		disposable.Disposable =
        this.ObserveEveryValueChanged(x => x.beats)
            .Subscribe(value => 
            {
                beatReaction = 1f;
                onClickEvent.Invoke();
            })
            .AddTo(gameObject);
    }

    void Update()
    {
        beatReaction += (0 - beatReaction) * 0.1f; 

        var ts = (DateTime.Now - dateTime);
        dateTime = DateTime.Now;
        midiDateTime = midiDateTime.Add(ts);
        timeSpan = midiDateTime - startDateTime;
        var st = //timeSpan.TotalSeconds.ToString() +"\n" +
            String.Format("{0:HH:mm:ss.fff}", midiDateTime) + " " + bars + "." + beats + "." + sixteenths;
        dumpText.text = st;
    }

    void FixedUpdate()
    {
        var ts = (DateTime.Now - dateTime);
        dateTime = DateTime.Now;
        midiDateTime = midiDateTime.Add(ts);
        timeSpan = midiDateTime - startDateTime;
        var st = //timeSpan.TotalSeconds.ToString() +"\n" +
            String.Format("{0:HH:mm:ss.fff}", midiDateTime) + " " + bars + "." + beats + "." + sixteenths;
        dumpText.text = st;
    }

}