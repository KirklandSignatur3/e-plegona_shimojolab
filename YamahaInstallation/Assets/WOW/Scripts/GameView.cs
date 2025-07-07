using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using UniRx;
using System.Threading;
using System.Linq;
using Shapes;
using UnityEngine.Profiling;
//using static UnityEditor.Profiling.RawFrameDataView;

public class GameView : IView
{
	[SerializeField] Kakuhen kakuhen;
	[SerializeField] SceneManager sceneManager;
	[SerializeField] AbletonManager abletonManager;
	[SerializeField] ColorManager colorManager;
	[SerializeField] GameObject container;
	[SerializeField] GameObject noteViewOrigin;
	[SerializeField] GameObject hamonOrigin;
	[SerializeField] Particles particles;
	[SerializeField] Sparks sparks;
	[SerializeField] List<AudioTrack> tracks = new List<AudioTrack>();
	[SerializeField] CountDownView countDownView;
	[SerializeField] Button tutorialSkipButton;
	[SerializeField] Button gameSkipButton;

	#region UI
	[SerializeField] TimerView timerView;
	[SerializeField] ButtonsView buttonsView;
	[SerializeField] CircleView circleView;
	#endregion
	#region Demo_1
	[SerializeField] NumberView numberView;
	[SerializeField] List<TextView> textViews = new List<TextView>();
	bool isTutorial = false;
	#endregion
	CancellationTokenSource cts = null;
	SingleAssignmentDisposable musicChangeDisposable = null;
	SingleAssignmentDisposable kakuhenDisposable = null;
	int sparkCount = 0;

    LabJackLog LJScript;

    private void Start()////////////////
    {
        LJScript = GameObject.FindGameObjectWithTag("Log").GetComponent<LabJackLog>(); // MM
        //LJScript.SendLabJackSignal(LJScript.signalDelay, 7);

    }


    /// <summary>
    /// 
    /// </summary>
    private void OnDestroy()
	{
		musicChangeDisposable?.Dispose();
		cts?.Cancel();
		timerView.StopTimer();
	}

	/// <summary>
	/// 
	/// </summary>
	public override void Init()
	{
		var context = SharedContext.Instance.context;
		var config = SharedConfig.Instance.config;

		gameObject.SetActive(false);

		timerView.TimeLimit = config.gameDuration;
		timerView.Init();
		countDownView.Init();
		numberView.Init();
		circleView.Init();
		buttonsView.Init();
		foreach (var item in textViews) item.Init();

		isTutorial = false;
		int musicChange = context.musicChange;

		tutorialSkipButton.gameObject.SetActive(false);
		gameSkipButton.gameObject.SetActive(false);

		kakuhen.Off();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="token"></param>
	/// <returns></returns>
	public async override Task Show(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		var config = SharedConfig.Instance.config;

		gameObject.SetActive(true);
		abletonManager.StopMusic();
		abletonManager.StopClick();
		abletonManager.Pause();
		abletonManager.VolumeZero();
		abletonManager.SpecialZero();

		SharedContext.Instance.Load();
		SharedContext.Instance.context.ResetGameData();
		SharedContext.Instance.context.playerId++;
		SharedContext.Instance.context.startTime = DateTime.Now;

		QueueTutorial();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="token"></param>
	/// <returns></returns>
	public async override Task Hide(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		tutorialSkipButton.gameObject.SetActive(false);
		gameSkipButton.gameObject.SetActive(false);

		kakuhen.Off();
		kakuhenDisposable?.Dispose();
		musicChangeDisposable?.Dispose();
		cts?.Cancel();
		timerView.StopTimer();
		abletonManager.VolumeDown();

		var sharedContext = SharedContext.Instance;
		sharedContext.StopRecord();
		sharedContext.Save();
		sharedContext.ExportCSV();

		//先に無効にしてから全部削除.
		buttonsView.Negative();
		ClearAllNotes();

		List<Task> arrayTask = new List<Task>();
		arrayTask.Add(timerView.Hide(token));
		arrayTask.Add(circleView.Hide(token));
		arrayTask.Add(buttonsView.Hide(token));
		await Task.WhenAll(arrayTask);

		await Task.Delay(1000, cancellationToken: token);
		gameObject.SetActive(false);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	async void QueueTutorial()
	{
		var config = SharedConfig.Instance.config;
		var context = SharedContext.Instance.context;

		isTutorial = true;

		cts?.Cancel();
		cts = new CancellationTokenSource();

		CancellationToken token = this.cts.Token;
		try
		{
			numberView.Score = SharedContext.Instance.context.playerId;
			await numberView.Show(token);
			await Task.Delay(3000, cancellationToken: token);
			await numberView.Hide(token);

			tutorialSkipButton.gameObject.SetActive(true);

			await textViews[0].Show(token);
			await Task.Delay(8000, cancellationToken: token);

			abletonManager.SetUpInstruments();
			abletonManager.VolumeZero();
			abletonManager.PlayClick();
			abletonManager.Play();
			abletonManager.VolumeUp();
			SetUpColor();

			await textViews[0].Hide(token);

			buttonsView.IsLaneThree = circleView.IsLaneThree = false;
			var arrayTask = new List<Task>();
			arrayTask.Add(buttonsView.Show(token));
			arrayTask.Add(circleView.Show(token));
			await Task.WhenAll(arrayTask);

			await textViews[1].Show(token);
			await buttonsView.Tutorial1Start(token);
			await Task.Delay(5000, cancellationToken: token);
			await textViews[1].Hide(token);

			await textViews[2].Show(token);
			await Task.Delay(5000, cancellationToken: token);
			await textViews[2].Hide(token);

			await textViews[3].Show(token);
			await Task.Delay(20000, cancellationToken: token);
			//await textViews[3].Hide(token);

			QueueStartGame();
		}
		catch (OperationCanceledException)
		{
			Debug.Log("cancel");
		}
		finally
		{
			cts?.Dispose();
			cts = null;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	async void QueueStartGame()
	{
		var config = SharedConfig.Instance.config;
		var context = SharedContext.Instance.context;

		cts?.Cancel();
		cts = new CancellationTokenSource();
        Debug.Log("real game start"); // MM
		

        try
		{
			CancellationToken token = this.cts.Token;

			tutorialSkipButton.gameObject.SetActive(false);

			//先に無効にしてから全部削除.
			buttonsView.Negative();
			ClearAllNotes();
			particles.RefreshAll(token);
			isTutorial = false;
			abletonManager.VolumeDown();

			var tutorialTask = new List<Task>();
			tutorialTask.Add(numberView.Hide(token));
			tutorialTask.Add(textViews[0].Hide(token));
			tutorialTask.Add(textViews[1].Hide(token));
			tutorialTask.Add(textViews[2].Hide(token));
			tutorialTask.Add(textViews[3].Hide(token));
			tutorialTask.Add(buttonsView.Hide(token));
			tutorialTask.Add(circleView.Hide(token));
			await Task.WhenAll(tutorialTask);
			await textViews[4].Show(token);
			await Task.Delay(2000, cancellationToken: token);
			//await textViews[4].Hide(token);

			abletonManager.StopClick();
			abletonManager.SetUpInstruments();
			abletonManager.Pause();
			abletonManager.ChangeMusic(0);
			SetUpColor();

			musicChangeDisposable = new SingleAssignmentDisposable();
			musicChangeDisposable.Disposable = this.ObserveEveryValueChanged(x => context.musicChange)
			.Subscribe(value =>
			{
				var context = SharedContext.Instance.context;
				var s = (float)Math.Min(context.score, config.maxScore) / (float)config.maxScore;
				int i = (int)(s * 5f);
				abletonManager.ChangeMusic(i);
			})
			.AddTo(gameObject);

			kakuhenDisposable = new SingleAssignmentDisposable();
			kakuhenDisposable.Disposable = this.ObserveEveryValueChanged(x => context.level)
			.Subscribe(value =>
			{
				if (value == Level.Level4)
				{
					kakuhen.On();
					abletonManager.SpecialUp();
				}
				else
				{
					kakuhen.Off();
					abletonManager.SpecialDown();
				}
			})
			.AddTo(gameObject);

			abletonManager.Play();
			abletonManager.VolumeUp();

			var arrayTask = new List<Task>();
			arrayTask.Add(textViews[4].Hide(token));
			//arrayTask.Add(countDownView.Show(token));
			await Task.WhenAll(arrayTask);

			await countDownView.Show(token);
			await countDownView.Hide(token);
			var arrayTask2 = new List<Task>();
			//arrayTask2.Add(countDownView.Hide(token));
			buttonsView.IsLaneThree = true;
			circleView.IsLaneThree = true;
			arrayTask2.Add(buttonsView.Show(token));
			arrayTask2.Add(circleView.Show(token));
			arrayTask2.Add(timerView.Show(token));
			await Task.WhenAll(arrayTask2);

			timerView.StartTimer();
            
            SharedContext.Instance.StartRecord();

			gameSkipButton.gameObject.SetActive(true);
            LJScript.LogEvent(timerView.GetElapsedTime(), LJScript.START_GAME);// MM
        }
		catch (OperationCanceledException)
		{
			Debug.Log("cancel");
		}
		finally
		{
			cts?.Dispose();
			cts = null;
			
            LJScript.LogEvent(timerView.GetElapsedTime(), LJScript.START_GAME);// MM
        }
        LJScript.LogEvent(timerView.GetElapsedTime(), LJScript.START_GAME);// MM

    }

    public void SkipTutorial()
	{
		cts?.Cancel();
		QueueStartGame();
	}

	/// <summary>
	/// Noteを作成.    
	/// </summary>
	public Note CreateNoteStart(Pad pad)
	{
/*        Debug.Log(pad.player); // MM PUT LABJACK CODE HERE
        Debug.Log(pad.track); // MM P*/

		
		if ((int)pad.player == 0)
        {
            LJScript.LogEvent(timerView.GetElapsedTime(), LJScript.P1_CREATE_NOTE_START);

        }
        else if ((int)pad.player == 1)
		{
            LJScript.LogEvent(timerView.GetElapsedTime(), LJScript.P2_CREATE_NOTE_START);

        }

        var config = SharedConfig.Instance.config;
		var sharedContext = SharedContext.Instance;
		var context = sharedContext.context;
		var clickPlayer = ClickPlayer.Instance;

		//create and add note.
		var note = (Instantiate(noteViewOrigin, new Vector3(0f, 0f, 0f), Quaternion.identity) as GameObject).GetComponent<Note>();
		var colors = colorManager.GetColors(pad.player);
		note.CreateStart(Quantizer.Quantize16(clickPlayer.TimeSpan.TotalSeconds, sharedContext.durEighth), pad, colors[0]);
		note.gameObject.transform.SetParent(tracks[pad.track].gameObject.transform);
		tracks[pad.track].AddNote(note);

		//ring tap sonic.
		//abletonManager.RingSonic(note.pad.player);

		note.OnDivisionEvent.AddListener(OnDivision);
		note.OnTapDownEvent.AddListener(OnTapDown);
		note.OnTapDownUpdateEvent.AddListener(OnTapDownUpdate);
		note.OnTapUpEvent.AddListener(OnTapUp);
		note.OnCompleteEvent.AddListener(OnCompleteNote);
		note.OnDeathEvent.AddListener(OnDeathNote);

		return note;
	}

	/// <summary>
	/// Noteの作成終了.    
	/// </summary>
	public void CreateNoteEnd(Pad pad, Note note)
	{
		//when a note is finished being created. 
		// passes in what button was pressed and if it was p1 or p2 (player, #1-3); note: more about the note obj not who made it (not relevant)?
		var config = SharedConfig.Instance.config;
		var sharedContext = SharedContext.Instance;
		var context = sharedContext.context;
		var clickPlayer = ClickPlayer.Instance;

		note.CreateEnd(Quantizer.Quantize16(clickPlayer.TimeSpan.TotalSeconds, sharedContext.durEighth));
		if (note.isLong)
		{
			int offset = abletonManager.longNoteOffset;
			note.note = UnityEngine.Random.Range(offset, offset + abletonManager.numLongNote);
		}
		else
		{
			var index = abletonManager.instruments[(int)pad.player * 3 + (int)pad.track];
			note.note = UnityEngine.Random.Range(index * 12, index * 12 + 8);
		}

		if (!isTutorial) ScoreCalculator.AddCreateScore(pad);

        if ((int)pad.player == 0)
        {
            LJScript.LogEvent(timerView.GetElapsedTime(), LJScript.P1_CREATE_NOTE_END);

        }
        else if ((int)pad.player == 1)
        {
            LJScript.LogEvent(timerView.GetElapsedTime(), LJScript.P2_CREATE_NOTE_END);

        }

    }

	/// <summary>
	/// Noteを作成.    
	/// </summary>
	public void TapStart(Pad pad, Note note)
	{
		// event logs here
		note.TapStart();
	}

	/// <summary>
	/// Noteを作成.    
	/// </summary>
	public void TapEnd(Pad pad, Note note)
	{
		note.TapEnd();
	}

	/// <summary>
	/// Noteを作成.    
	/// </summary>
	public void OnTapDown(Note note)
	{
		var config = SharedConfig.Instance.config;
		var sharedContext = SharedContext.Instance;
		var context = sharedContext.context;

		//calc score.
		Hit hit; // hit: none = -1, good = 0, nice = 1, perfect = 2
		if (!isTutorial) hit = ScoreCalculator.AddHitScore(note); // create "hit", is good/nice/perfect
		else hit = ScoreCalculator.CalcHit(note.GetGap(0.5));
		if (note.hit == Hit.None) note.hit = hit;

		if ((int)note.pad.player == 1) //MM, log the type of hit // chagned from 0 to 1 (7/7/25)
		{ // IF PLAYER 1
            switch (hit)
            {
                case Hit.None:
                    LJScript.LogEvent(timerView.GetElapsedTime(), LJScript.P1_MISS_NOTE);
                    break;

                case Hit.Good:
                    LJScript.LogEvent(timerView.GetElapsedTime(), LJScript.P1_GOOD);
                    break;

                case Hit.Nice:
                    LJScript.LogEvent(timerView.GetElapsedTime(), LJScript.P1_NICE);
                    break;

                case Hit.Perfect:
                    LJScript.LogEvent(timerView.GetElapsedTime(), LJScript.P1_PERFECT);
                    break;

            }
		}
		else // if PLAYER 2
		{
            switch (hit)
            {
                case Hit.None:
                    LJScript.LogEvent(timerView.GetElapsedTime(), LJScript.P2_MISS_NOTE);
                    break;

                case Hit.Good:
                    LJScript.LogEvent(timerView.GetElapsedTime(), LJScript.P2_GOOD);
                    break;

                case Hit.Nice:
                    LJScript.LogEvent(timerView.GetElapsedTime(), LJScript.P2_NICE);
                    break;

                case Hit.Perfect:
                    LJScript.LogEvent(timerView.GetElapsedTime(), LJScript.P2_PERFECT);
                    break;

            }
        }



        //add center ball.
        var colors = colorManager.GetColors(note.pad.player);
		var hamon = Instantiate(hamonOrigin, note.position, Quaternion.identity, container.transform).GetComponent<Hamon>();
		hamon.Show(note.hit);
		if ((int)note.hit >= (int)Hit.Good) particles.Add(note.position, UnityEngine.Random.Range(0.2f, 1f), colors);
		for (int i = 0; i < config.numSpark[(int)note.hit]; i++) sparks.Add(note.position, colors);

		//ring sound.
		//abletonManager.RingSonic(note.pad.player);
		abletonManager.NoteOn(note.note, config.hitVelocity[(int)note.hit]);
	}

	/// <summary>
	///
	/// </summary>
	public void OnTapDownUpdate(Note note) // i think this is for sustained notes 
	{
		var context = SharedContext.Instance.context;
		sparkCount++;
		if (sparkCount > 20)
		{
			sparks.Add(note.position, colorManager.GetColors(note.pad.player));
			sparkCount = 0;
			var hit = ScoreCalculator.AddHitScore(Hit.Good, note);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public void OnTapUp(Note note)
	{
		//stop ring.
		abletonManager.NoteOff(note.note);
        // CSV LOGGING
        if ((int)note.pad.player == 1)
        {
            LJScript.LogEvent(timerView.GetElapsedTime(), LJScript.P1_TAP_END);

        }
        else if ((int)note.pad.player == 0)
        {
            LJScript.LogEvent(timerView.GetElapsedTime(), LJScript.P2_TAP_END);

        }

    }

	/// <summary>
	/// 
	/// </summary>
	public void OnCompleteNote(Note note)
	{
		RemoveNoteListener(note);
		tracks[note.pad.track].RemoveNote(note);
		Destroy(note.gameObject);
	}

	/// <summary>
	/// 
	/// </summary>
	public void OnDeathNote(Note note)
	{
		RemoveNoteListener(note);
		tracks[note.pad.track].RemoveNote(note);
		Destroy(note.gameObject);
		if (!isTutorial) ScoreCalculator.AddDeathScore(); // does this do anything?
	}

	/// <summary>
	/// 
	/// </summary>
	public void OnDivision(Note note)
	{
		var config = SharedConfig.Instance.config;
		var sharedContext = SharedContext.Instance;
		var context = sharedContext.context;
		var clickPlayer = ClickPlayer.Instance;
		var copy = (Instantiate(noteViewOrigin, new Vector3(0f, 0f, 0f), Quaternion.identity) as GameObject).GetComponent<Note>();
		var d = clickPlayer.TimeSpan.TotalSeconds - 0.5 * sharedContext.dur4Bars;
		copy.CreateStartEnd(note.begin, Quantizer.Quantize16(d, sharedContext.durEighth), note.pad, note.color);
		copy.gameObject.transform.SetParent(tracks[note.pad.track].gameObject.transform);
		tracks[note.pad.track].AddNote(copy);
		copy.OnDeathEvent.AddListener(OnDeathNote);
	}

	public void RemoveNoteListener(Note note)
	{
		note.OnDivisionEvent.RemoveListener(OnDivision);
		note.OnTapDownEvent.RemoveListener(OnTapDown);
		note.OnTapDownUpdateEvent.RemoveListener(OnTapDownUpdate);
		note.OnTapUpEvent.RemoveListener(OnTapUp);
		note.OnCompleteEvent.RemoveListener(OnCompleteNote);
		note.OnDeathEvent.RemoveListener(OnDeathNote);
	}

	/// <summary>
	/// タップしたPadの一番近いNoteを返す
	/// </summary>

	public Note GetNoteOnTap(Pad pad)
	{
		var sharedContext = SharedContext.Instance;
		var config = SharedConfig.Instance.config;
		var notes = tracks[pad.track].GetNotes();
		var taps = notes
		.Where(note => note.pad.track == pad.track)
		.Where(note => note.pad.player != pad.player)
		.Where(note => note.isActive)
		.Where(note => { return note.GetGap(0.5) < config.hitArea[(int)Hit.Good]; })
		.OrderBy(note => { return note.GetGap(0.5); }).ToArray();
		if (taps.Length > 0) return taps[0];

		return null;
	}

	public Note GetNoteOnTap2(Pad pad)
	{
		var sharedContext = SharedContext.Instance;
		var config = SharedConfig.Instance.config;
		var notes = tracks[pad.track].GetNotes();
		var taps = notes
		.Where(note => note.pad.track == pad.track)
		.Where(note => note.pad.player != pad.player)
		.Where(note => note.isActive)
		.Where(note => { return note.GetGap(0.5) < sharedContext.durEighth; })
		.OrderBy(note => { return note.GetGap(0.5); }).ToArray();
		if (taps.Length > 0) return taps[0];

		return null;
	}

	/// <summary>
	/// タップしたPadの一番近いNoteを返す
	/// </summary>
	public Note GetNoteCreateOnTap(Pad pad)
	{
		var sharedContext = SharedContext.Instance;
		var context = sharedContext.context;
		var config = SharedConfig.Instance.config;
		var notes = tracks[pad.track].GetNotes();
		var taps = notes
		.Where(note => note.pad.track == pad.track)
		.Where(note => note.pad.player == pad.player)
		.Where(note => { return note.GetGap(0.0) < sharedContext.durEighth * 0.4; })
		.OrderBy(note => { return note.GetGap(0.0); }).ToArray();
		if (taps.Length > 0) return taps[0];

		return null;
	}

	public int GetNumNote(Pad pad)
	{
		var sharedContext = SharedContext.Instance;
		var config = SharedConfig.Instance.config;
		var c = 0;
		foreach (var track in tracks)
		{
			var notes = track.GetNotes();
			var taps = notes
			//.Where(note => note.pad.track == pad.track)
			//.Where(note => note.pad.player == pad.player)
			.Where(note => note.isActive)
			.ToArray();
			c += taps.Length;
		}

		return c;
	}
	
	/// <summary>
	/// 
	/// </summary>
	public void OnTimeUp()
	{
		sceneManager.Goto(Scene.Finish);
	}

	/// <summary>
	/// 
	/// </summary>
	public void ClearAllNotes()
	{
		foreach (AudioTrack item in tracks)
		{
			foreach (var note in item.GetNotes()) note.Kill();
		}
	}

	/// <summary>
	/// 
	/// </summary>
	void SetUpColor()
	{
		var context = SharedContext.Instance.context;
		context.ballColor = UnityEngine.Random.Range(0, colorManager.GetNumTexture());
	}

	public void RingBeep()
	{
		abletonManager.RingBeep();
	}

	public void RingSonic(Player player)
	{
		abletonManager.RingSonic(player);
	}
}