using System.ComponentModel.Design;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Events;
using Mathd;
using Shapes;
using DG.Tweening;
public class Note : MonoBehaviour
{

	public Pad pad;
	public Disc view = null;
	public int note = 108;
	public bool isLong = false;
	public Hit hit = Hit.None;
	public Color color;

	public UnityEvent<Note> OnDivisionEvent = new UnityEvent<Note>();
	public UnityEvent<Note> OnTapDownEvent = new UnityEvent<Note>();
	public UnityEvent<Note> OnTapDownUpdateEvent = new UnityEvent<Note>();
	public UnityEvent<Note> OnTapUpEvent = new UnityEvent<Note>();
	public UnityEvent<Note> OnCompleteEvent = new UnityEvent<Note>();
	public UnityEvent<Note> OnDeathEvent = new UnityEvent<Note>();

	public bool isActive = false;
	public double begin = 0d;
	double end = 0d;
	double offset = 0d;
	bool isCreate = false;
	bool isTap = false;
	public double s, e;
	float scale = 0f;
	public Vector3 position;

	//Debug.Log(cam.ScreenToWorldPoint(new Vector3(0, 169, 10f) + new Vector3(576 / 2, 576 / 2, 0f)));
	double[] radiuses = new double[3] { 3.21, 4.17, 5.13 };

	private void OnDestroy()
	{
		Destroy(view);
		view = null;
	}

	public double GetGap(double point)
	{
		var sharedContext = SharedContext.Instance;
		var gap = 0d;
		//範囲内の場合は0を返す.それ以外の場合は頭かお尻からの距離で短い方.
		if (s >= point && e <= point) gap = 0;
		else gap = Math.Min(System.Math.Abs(s - point), System.Math.Abs(e - point));
		return gap * sharedContext.dur4Bars;
	}

	void Update()
	{
		var sharedContext = SharedContext.Instance;
		var context = sharedContext.context;
		var config = SharedConfig.Instance.config;
		var clickPlayer = ClickPlayer.Instance;

		//発音中.
		if (isTap)
		{
			if (isLong)
			{
				OnTapDownUpdateEvent?.Invoke(this);
				begin = Math.Max(clickPlayer.TimeSpan.TotalSeconds - 0.5 * sharedContext.dur4Bars, begin);
				if (Math.Abs(begin - end) < sharedContext.durEighth * 0.25f)
				{
					begin = end;
					Complete();
				}
			}
			else
			{
				begin = end;
				Complete();
			}
		}

		s = (clickPlayer.TimeSpan.TotalSeconds - begin) / sharedContext.dur4Bars;
		if (isCreate) e = 0f;
		else e = (clickPlayer.TimeSpan.TotalSeconds - end) / sharedContext.dur4Bars;

		s = Math.Min(s, config.noteLife);
		if (s <= e) s = e - 0.0001f;
		if (e >= config.noteLife) Death();

		var radian = (0.5f + offset - 0.25d) * System.Math.PI * 2f;
		var radius = radiuses[pad.track];
		position = new Vector3(Mathf.Sin(-(float)radian) * (float)radius, Mathf.Cos((float)radian) * (float)radius, 0f);

		UpdateDisc();
	}

	public void UpdateDisc()
	{
		var clickPlayer = ClickPlayer.Instance;
		var config = SharedConfig.Instance.config;
		view.AngRadiansStart = (float)((s + offset) * System.Math.PI * 2f);
		view.AngRadiansEnd = (float)((e + offset) * System.Math.PI * 2f);
		view.Radius = (float)radiuses[pad.track];
		view.Thickness = scale * 0.85f + clickPlayer.beatReaction * 0.15f;
		view.Color = color;
	}

	public void CreateStart(double start, Pad pad, Color color)
	{
		var sharedContext = SharedContext.Instance;
		var clickPlayer = ClickPlayer.Instance;

		this.pad = pad;
		this.color = color;
		this.color.a = 1f;
		view.Color = this.color;

		offset = 0.5d * (int)pad.player + 0.25d;
		begin = start;
		end = start;
		isCreate = true;

		s = (clickPlayer.TimeSpan.TotalSeconds - begin) / sharedContext.dur4Bars;
		e = 0f;

		scale = 1f;
		UpdateDisc();
		isActive = false;
		DOTween.To(() => scale, (x) => scale = x, 0.5f, 0.2f).SetDelay(0.1f).SetEase(Ease.OutCubic);
	}

	public void CreateEnd(double end)
	{
		var context = SharedContext.Instance.context;
		this.end = end;
		isLong = true;
		if (end - begin < 0.2d)
		{
			this.end = begin;
			isLong = false;
		}

		isCreate = false;
		isActive = true;
		//Debug.Log("create end" + isLong);
	}

	public void CreateStartEnd(double start, double end, Pad pad, Color color)
	{
		var sharedContext = SharedContext.Instance;
		var clickPlayer = ClickPlayer.Instance;
		this.pad = pad;
		this.color = color;
		view.Color = this.color;

		offset = 0.5d * (int)pad.player + 0.25d;
		begin = start;
		this.end = end;
		scale = 0.5f;
		isCreate = false;

		s = (clickPlayer.TimeSpan.TotalSeconds - begin) / sharedContext.dur4Bars;
		e = (clickPlayer.TimeSpan.TotalSeconds - end) / sharedContext.dur4Bars;

		UpdateDisc();
		isActive = true;
	}

	public void TapStart()
	{
		var clickPlayer = ClickPlayer.Instance;
		var sharedContext = SharedContext.Instance;
		var d = clickPlayer.TimeSpan.TotalSeconds - 0.5 * sharedContext.dur4Bars;
		if (isLong && d - begin > sharedContext.durEighth) OnDivisionEvent?.Invoke(this);
		isTap = true;
		OnTapDownEvent?.Invoke(this);
	}

	public void TapEnd()
	{
		isTap = false;
		OnTapUpEvent?.Invoke(this);
	}

	void Death()
	{
		if (!isActive) return;
		TapEnd();
		isActive = false;
		DOTween.To(() => color.a, (x) => color.a = x, 0f, 0.1f).SetDelay(0.1f);
		DOTween.To(() => scale, (x) => scale = x, 0f, 0.25f).SetEase(Ease.OutCubic)
		.OnComplete(() =>
		{
			color.a = 0f;
			OnDeathEvent?.Invoke(this);
		});
	}

	public void Complete()
	{
		if (!isActive) return;
		TapEnd();
		isActive = false;
		DOTween.To(() => color.a, (x) => color.a = x, 0f, 0.1f).SetDelay(0.1f);
		DOTween.To(() => scale, (x) => scale = x, 0f, 0.25f).SetEase(Ease.OutCubic)
		.OnComplete(() =>
		{
			color.a = 0f;
			OnCompleteEvent?.Invoke(this);
		});
	}

	public void Kill()
	{
		//if(!isCreate) CreateEnd(end);
		//if (!isActive) return;
		TapEnd();
		isActive = false;
		DOTween.To(() => color.a, (x) => color.a = x, 0f, 0.1f);
		DOTween.To(() => scale, (x) => scale = x, 0f, 0.1f).SetEase(Ease.OutCubic)
		.OnComplete(() =>
		{
			color.a = 0f;
			OnCompleteEvent?.Invoke(this);
		});
	}
}