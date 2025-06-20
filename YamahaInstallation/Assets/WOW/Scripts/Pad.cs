using UnityEngine;
using UnityEngine.EventSystems;


public class Pad : MonoBehaviour
{
	[SerializeField] GameView gameView;
	[SerializeField] EventTrigger eventTrigger;
	[SerializeField] public ButtonAnimation anim;
	[SerializeField] public int track;
	[SerializeField] public Player player;
	Note create = null;
	Note tap = null;
	double lastNoteTap = 0;

	bool isCreate = false;
	bool isTap = false;
	LabJackLog LJScript;


	private void Start()////////////////
	{
		LJScript = GameObject.FindGameObjectWithTag("Log").GetComponent<LabJackLog>();
		//LJScript.SendLabJackSignal(LJScript.signalDelay, 7);

	}

    void Update()
	{
		if (isCreate)
		{
			var sharedContext = SharedContext.Instance;
			var clickPlayer = ClickPlayer.Instance;
			Note note = gameView.GetNoteOnTap(this);
			var gap = Quantizer.Quantize16(clickPlayer.TimeSpan.TotalSeconds, sharedContext.durEighth) - create.begin;
			if (note != null || gap > sharedContext.durBars * 2) TapUp();
		}

		if (isTap)
		{
			Note note = gameView.GetNoteOnTap(this);
			if (note == null) TapUp();
		}
	}

	public void Active()
	{
		lastNoteTap = 0;
		eventTrigger.enabled = true;
	}

	public void Negative()
	{
		TapUp();
		eventTrigger.enabled = false;
		lastNoteTap = 0;
	}

	public void TapDown()
	{
		if (this.create == null || this.tap == null)
		{

			var config = SharedConfig.Instance.config;
			var sharedContext = SharedContext.Instance;
			var context = SharedContext.Instance.context;
			var clickPlayer = ClickPlayer.Instance;
			var note = gameView.GetNoteOnTap(this);
			var create = gameView.GetNoteCreateOnTap(this);
			var gap = Quantizer.Quantize16(clickPlayer.TimeSpan.TotalSeconds, sharedContext.durEighth) - lastNoteTap;
			var numNote = gameView.GetNumNote(this);

			gameView.RingSonic(player);

            /*			Debug.Log(this.player);     //	
                        Debug.Log(this.track);     //	*/
            //LJScript.LogEvent(1, LJScript.P1_CREATE_NOTE, 0);


            if (note == null && create == null && gap >= sharedContext.durEighth)
			{
				if(numNote >= config.maxBall)
				{
					anim.SetRed();
					gameView.RingBeep();

					return;
				}

				Debug.Log("create"); //
				note = gameView.CreateNoteStart(this);
				this.create = note;
				isCreate = true;

			}
			else if (note != null)
			{
				Debug.Log("tap");//
				gameView.TapStart(this, note);
				this.tap = note;
				isTap = true;
			}
			//LJScript.SendLabJackSignal(LJScript.signalDelay, 7);  //////////////// Labjack signal code

		}
	}

	public void TapUp()
	{
		//Debug.Log("tapUp");
		var context = SharedContext.Instance.context;

		if (isCreate) //code is run when creating a note
		{
			//Debug.Log("delete create");
			gameView.CreateNoteEnd(this, this.create);
			this.create = null;
			isCreate = false;
		}

		if (isTap) // when playing/tapping on a note that was sent to you
        {
			//Debug.Log("delete tap");
			var sharedContext = SharedContext.Instance;
			var clickPlayer = ClickPlayer.Instance;
			lastNoteTap = Quantizer.Quantize16(clickPlayer.TimeSpan.TotalSeconds, sharedContext.durEighth);
			gameView.TapEnd(this, this.tap);
			this.tap = null;
			isTap = false;
		}
	}

	public void EnterTap()
	{
		//Debug.Log("Enter tap------------");
	}
	
	public void TapDown2()
	{
		//Debug.Log("tap down------------");
	}


}
