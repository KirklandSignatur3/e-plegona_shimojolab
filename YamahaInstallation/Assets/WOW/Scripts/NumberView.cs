using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;

public class NumberView : IView
{
	[SerializeField] CanvasGroup group;
	[SerializeField] TextMeshProUGUI text;
	private int score;
	public int Score
	{
		get { return score; }
		set { score = value; }
	}
	Sequence sequence;

	private void OnDestroy()
	{
		sequence?.Kill(false);
	}

	public override void Init()
	{
		gameObject.SetActive(false);
		group.alpha = 0f;
        text.text = "#000";
		text.alpha = 0f;
	}

	public async override Task Show(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		isShow = true;
		gameObject.SetActive(true);

        var ts = new TaskCompletionSource<bool>();
		group.alpha = 0;
        //var color = Color.white;
        //color.a = 0f;
		text.alpha = 1f;
		//group.GetComponent<RectTransform>().localScale = new Vector3(1.2f, 1.2f, 1.2f);
		group.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
        text.text = "<size=70%>#<size=100%>" + String.Format("{0:000}", score);
        sequence = DOTween.Sequence()
		//.Append(group.GetComponent<RectTransform>().DOScale(1f, 1.5f).SetEase(Ease.OutCubic))
		.Append(group.DOFade(1f, 0.5f))
		//.Join(text.DOFade(1f, 0.5f))
		//.Join(text.DOText("<size=70%>#<size=100%>" + String.Format("{0:000}", score), 1f, scrambleMode: ScrambleMode.Numerals).SetEase(Ease.OutCubic))
		.OnComplete(() =>
		{
            sequence.Kill(false);
            sequence = null;
			ts.SetResult(true);
		});
		token.Register(() =>
		{
			sequence?.Kill(false);
			sequence = null;
			ts.TrySetCanceled();
		});
		await ts.Task;
	}

	public async override Task Hide(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		var ts = new TaskCompletionSource<bool>();

        if (!isShow)
        { 
			gameObject.SetActive(false);
			ts.SetResult(true);
			await ts.Task;
            return;
        }

        sequence = DOTween.Sequence()
		.Join(group.DOFade(0f, 0.5f).SetDelay(0.5f))
		.OnComplete(() =>
		{
            sequence.Kill(false);
            sequence = null;
			gameObject.SetActive(false);
			isShow = false;
			ts.SetResult(true);
        });
		token.Register(() =>
		{
			sequence?.Kill(false);
			sequence = null;
			ts.TrySetCanceled();
		});
		await ts.Task;
	}
}