using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;

public class ScoreView : IView
{
	[SerializeField] CanvasGroup group;
	[SerializeField] TextMeshProUGUI text;
	[SerializeField] Image image;
	private int score;
	public int Score
	{
		get { return score; }
		set { score = value; }
	}
	Sequence sequence;

	private void OnDestroy()
	{
		sequence?.Kill();
	}

	public override void Init()
	{
		group.alpha = 0f;
        var color = Color.white;
        color.a = 0f;
        image.color = color;
        text.text = "0000";
		text.alpha = 0f;
	}

	public async override Task Show(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		var ts = new TaskCompletionSource<bool>();
		gameObject.SetActive(true);
		group.alpha = 1;
        var color = Color.white;
        color.a = 0f;
		image.color = color;
		text.alpha = 0f;
		text.text = "<size=40%>pt";
		image.fillAmount = 0f;

		image.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
		image.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0f, 0f, 44.9f);
		group.GetComponent<RectTransform>().localScale = new Vector3(1.2f, 1.2f, 1.2f);
		sequence = DOTween.Sequence()
		.Append(group.GetComponent<RectTransform>().DOScale(1f, 1f).SetEase(Ease.OutCubic))
		.Join(image.DOFade(1f, 0.5f))
		.Join(image.DOFillAmount(1f, 1f).SetEase(Ease.InOutCubic))
		.Join(image.GetComponent<RectTransform>().DOLocalRotate(new Vector3(0f, 0f, -135f), 1f).SetEase(Ease.InOutCubic))
		.Join(text.DOFade(1f, 0.5f).SetDelay(0.75f))
		
		.Join(text.DOText(String.Format("{0:0000}", score), 1.5f, scrambleMode: ScrambleMode.Numerals).SetDelay(0.0f).SetEase(Ease.OutCubic))
		.OnComplete(() =>
		{
			sequence.Kill(false);
            sequence = null;
			ts.SetResult(true);
		});
		token.Register(() =>
		{
			sequence?.Kill(false);
			ts.TrySetCanceled();
		});
		await ts.Task;
	}

	public async override Task Hide(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		var ts = new TaskCompletionSource<bool>();
        image.GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 1);
        sequence = DOTween.Sequence()
		.Append(image.DOFillAmount(0f, 1f).SetEase(Ease.InOutCubic))
		.Join(image.GetComponent<RectTransform>().DOLocalRotate(new Vector3(0f, 0f, -315f), 1f).SetEase(Ease.InOutCubic))
		.Join(group.DOFade(0f, 0.5f).SetDelay(0.5f))
		.OnComplete(() =>
		{
			sequence.Kill(false);
            sequence = null;
			gameObject.SetActive(false);
			ts.SetResult(true);
		});
		token.Register(() =>
		{
			sequence.Kill(false);
            sequence = null;
			gameObject.SetActive(false);
			ts.TrySetCanceled();
		});
		await ts.Task;
	}
}
