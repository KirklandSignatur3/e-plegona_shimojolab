using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class TextView : IView
{
	[SerializeField] AbletonManager abletonManager;
	[SerializeField] CanvasGroup textGroup;
	[SerializeField] List<string> texts = new List<string>();
	[SerializeField] List<Image> image_1 = new List<Image>();
	[SerializeField] List<Image> image_2 = new List<Image>();
	[SerializeField] List<TextMeshProUGUI> text_1 = new List<TextMeshProUGUI>();
	[SerializeField] List<TextMeshProUGUI> text_2 = new List<TextMeshProUGUI>();
	Sequence sequence;
	float imageWidth_1 = 0f;
	float imageWidth_2 = 0f;
	float posY;

	private void OnDestroy()
	{
		sequence?.Kill(false);
	}

	public override void Init()
	{
		gameObject.SetActive(false);
		textGroup.alpha = 0f;
		isShow = false;
	}

	public async override Task Show(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		isShow = true;
		gameObject.SetActive(true);

		var ts = new TaskCompletionSource<bool>();

		SEPlayer.Instance.PlaySE(0);
		
		sequence = DOTween.Sequence();
		
		for (var i = 0; i < texts.Count; i++)
		{
			text_1[i].text = texts[i];
			var w = text_1[i].preferredWidth + 26;
			var duration = texts[i].Length * 0.04f;
			var delay = 0.05f * i;

			text_1[i].text = text_2[i].text = string.Empty;
			image_1[i].GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 30f);
			image_2[i].GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 30f);
			
			sequence.Join(textGroup.DOFade(1f, 0f))
			.Join(text_1[i].DOText(texts[i], duration).SetEase(Ease.InOutCubic).SetDelay(delay))
			.Join(text_2[i].DOText(texts[i], duration).SetEase(Ease.InOutCubic).SetDelay(delay))
			.Join(image_1[i].GetComponent<RectTransform>().DOSizeDelta(new Vector2(w, 30),duration).SetEase(Ease.InOutCubic).SetDelay(delay))
			.Join(image_2[i].GetComponent<RectTransform>().DOSizeDelta(new Vector2(w, 30),duration).SetEase(Ease.InOutCubic).SetDelay(delay));
		}

		sequence.OnComplete(() =>
		{
			sequence.Kill(false);
            sequence = null;
			ts.SetResult(true);
		});

		token.Register(() =>
		{
			sequence.Kill(false);
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
		.Append(textGroup.DOFade(0, 0.5f))
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
