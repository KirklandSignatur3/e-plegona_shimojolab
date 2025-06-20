using System;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using System.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine.UI;

public class TimerView : IView
{
	[SerializeField] CanvasGroup group;
    [SerializeField] GameObject timer_1;
    [SerializeField] GameObject timer_2;
	[SerializeField] TextMeshProUGUI text_1;
	[SerializeField] TextMeshProUGUI text_2;
	private float timeLimit = 60;
	public float TimeLimit
	{
		get { return timeLimit; }
		set { timeLimit = value; }
	}

	private float startTime;
	private bool running = false;
	private bool timeUp = false;
	private bool isBig = false;
	[SerializeField] UnityEvent onTimeUp = new UnityEvent();
	Tweener tweener = null;

	void Update()
	{
		if (running && !timeUp)
		{
            float elapsedTime = Time.time - startTime;
            var b = (timeLimit - elapsedTime) < 30f; 
            if(b && isBig != b) 
			{
				group.DOFade(1, 0.5f);
				Scale(1.75f, true); 
			} 
            isBig = b;
			
			if (elapsedTime >= timeLimit)
			{
				timeUp = true;
				onTimeUp?.Invoke();
			}
		}
		
		var t = new TimeSpan();
		if(!timeUp) t = TimeSpan.FromSeconds(Math.Floor(Convert.ToDouble(timeLimit - GetElapsedTime())));
		text_1.text = text_2.text = t.ToString(@"mm\:ss");
	}

	public override void Init()
	{
		gameObject.SetActive(false);
		startTime = Time.time;
		var t = TimeSpan.FromSeconds(Math.Floor(Convert.ToDouble(timeLimit - GetElapsedTime())));
		text_1.text = text_2.text = t.ToString(@"mm\:ss");
		group.alpha = 0;
        running = false;
        timeUp = false;
        isBig = false;
        Scale(1f, false);
	}

	public async override Task Show(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		gameObject.SetActive(true);
		var ts = new TaskCompletionSource<bool>();
		tweener = group.DOFade(0, 0f).OnComplete(() => { ts.SetResult(true); });
		token.Register(() =>
		{
			ts.TrySetCanceled();
			tweener?.Kill(false);
		});
		await ts.Task;
	}

	public async override Task Hide(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		var ts = new TaskCompletionSource<bool>();
		tweener = group.DOFade(0, 0.5f).OnComplete(() =>
		{
			gameObject.SetActive(false);
			ts.SetResult(true);
		});
		token.Register(() =>
		{
			ts.TrySetCanceled();
			tweener?.Kill(false);
			gameObject.SetActive(false);
		});
		await ts.Task;
	}

	public void Scale(float s, bool anim)
	{
		if (anim)
		{
			text_1.GetComponent<RectTransform>().DOScale(new Vector3(s, s, s), 0.5f).SetEase(Ease.OutCubic);
			text_2.GetComponent<RectTransform>().DOScale(new Vector3(s, s, s), 0.5f).SetEase(Ease.OutCubic);
		}
		else
		{
			text_1.GetComponent<RectTransform>().localScale =
			text_2.GetComponent<RectTransform>().localScale = new Vector3(s, s, s);
		}
	}

	public void StartTimer()
	{
		startTime = Time.time;
		running = true;
	}

	public void StopTimer()
	{
		running = false;
	}

	public float GetElapsedTime()
	{
		if (running)
		{
			return Time.time - startTime;
		}
		return 0;
	}
}