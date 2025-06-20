using System.Runtime.InteropServices;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks;
using System.Threading;
using TMPro;

//0.5902778
public class CountDownView : IView
{
	[SerializeField] CanvasGroup group;
	[SerializeField] Image maskImage;
	[SerializeField] Image maskedImage;
    [SerializeField] GameObject containerWhite;
    [SerializeField] GameObject containerBlack;
	[SerializeField] List<Image> numberWhite;
	[SerializeField] List<Image> numberBlack;
	float number = 0;
	Image currentWhite = null;
	Image currentBlack = null;
	Sequence sequence1 = null;
	Sequence sequence2 = null;

	public override void Init()
	{
		gameObject.SetActive(false);
		group.alpha = 0f;
		var color = Color.white;
		color.a = 0;
		maskedImage.color = color;
	}

	public async override Task Show(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		gameObject.SetActive(true);
		group.alpha = 1f;
		var ts = new TaskCompletionSource<bool>();
        foreach(var item in numberWhite) item.gameObject.SetActive(false);
        foreach(var item in numberBlack) item.gameObject.SetActive(false);
		currentWhite = numberWhite[0];
		currentBlack = numberBlack[0];
		number = 0f;
		
		//group.GetComponent<RectTransform>().localScale = new Vector3(0.5902778f, 0.5902778f, 0.5902778f);
		group.GetComponent<RectTransform>().localScale = Vector3.one * 0.5f;
		maskImage.GetComponent<RectTransform>().sizeDelta = new Vector2(576f, 576f);
		var color = Color.white;
		color.a = 0;
		maskedImage.color = color;
		sequence1 = DOTween.Sequence()
		.Append(DOTween.To(() => number, (x) => number = x, 4, 4).SetEase(Ease.Linear).OnUpdate(() =>
		{
			var current = (int)Mathf.Floor(number);
			currentWhite.gameObject.SetActive(false);
			currentBlack.gameObject.SetActive(false);
			currentWhite = numberWhite[current];
			currentWhite.gameObject.SetActive(true);
			currentBlack = numberBlack[current];
			currentBlack.gameObject.SetActive(true);
		}))
		.Join(maskImage.GetComponent<RectTransform>().DOSizeDelta(new Vector2(0f, 0f), 5f).SetEase(Ease.Linear))
		.Join(maskedImage.DOFade(1, 0.5f))
		.Join(group.GetComponent<RectTransform>().DOScale(new Vector3(0.6f, 0.6f, 0.6f), 0.5f).SetEase(Ease.OutCubic))
		//.AppendInterval(1f)
		.OnComplete(() =>
		{
			currentWhite.gameObject.SetActive(false);
			currentBlack.gameObject.SetActive(false);
			ts.SetResult(true);
		});
		token.Register(() =>
		{
			ts.TrySetCanceled();
			sequence1?.Kill();
			sequence2?.Kill();
		});

		float numScale = 1.5f;
		
		numberWhite[0].GetComponent<RectTransform>().localScale = new Vector3(numScale, numScale, numScale);
		numberBlack[0].GetComponent<RectTransform>().localScale = new Vector3(numScale, numScale, numScale);
		//numberWhite[0].DOFade(0f, 0f);
		//numberBlack[0].DOFade(0f, 0f);
		sequence2 = DOTween.Sequence()
		//.Append(maskImage.GetComponent<RectTransform>().DOSizeDelta(new Vector2(576f * 0.9f, 576f * 0.9f), 0.5f).SetEase(Ease.OutCubic))
		.Join(numberWhite[0].GetComponent<RectTransform>().DOScale(Vector3.one, 0.5f).SetEase(Ease.OutCubic))
		.Join(numberBlack[0].GetComponent<RectTransform>().DOScale(Vector3.one, 0.5f).SetEase(Ease.OutCubic))
		//.Join(numberWhite[0].DOFade(1f, 0.5f))
		//.Join(numberBlack[0].DOFade(1f, 0.5f))
		.AppendInterval(0.5f)

		//.Append(numberWhite[1].DOFade(0f, 0f))
		//.Append(numberBlack[1].DOFade(0f, 0f))
		.Append(numberWhite[1].GetComponent<RectTransform>().DOScale(new Vector3(numScale, numScale, numScale), 0f))
		.Append(numberBlack[1].GetComponent<RectTransform>().DOScale(new Vector3(numScale, numScale, numScale), 0f))
		//.Append(maskImage.GetComponent<RectTransform>().DOSizeDelta(new Vector2(576f * 0.7f, 576f * 0.7f), 0.5f).SetEase(Ease.OutCubic))
		.Join(numberWhite[1].GetComponent<RectTransform>().DOScale(Vector3.one, 0.5f).SetEase(Ease.OutCubic))
		.Join(numberBlack[1].GetComponent<RectTransform>().DOScale(Vector3.one, 0.5f).SetEase(Ease.OutCubic))
		//.Join(numberWhite[1].DOFade(1f, 0.5f))
		//.Join(numberBlack[1].DOFade(1f, 0.5f))
		.AppendInterval(0.5f)
		
		//.Append(numberWhite[2].DOFade(0f, 0f))
		//.Append(numberBlack[2].DOFade(0f, 0f))
		.Append(numberWhite[2].GetComponent<RectTransform>().DOScale(new Vector3(numScale, numScale, numScale), 0f))
		.Append(numberBlack[2].GetComponent<RectTransform>().DOScale(new Vector3(numScale, numScale, numScale), 0f))
		//.Append(maskImage.GetComponent<RectTransform>().DOSizeDelta(new Vector2(576f * 0.5f, 576f * 0.5f), 0.5f).SetEase(Ease.OutCubic))
		.Join(numberWhite[2].GetComponent<RectTransform>().DOScale(Vector3.one, 0.5f).SetEase(Ease.OutCubic))
		.Join(numberBlack[2].GetComponent<RectTransform>().DOScale(Vector3.one, 0.5f).SetEase(Ease.OutCubic))
		//.Join(numberWhite[2].DOFade(1f, 0.5f))
		//.Join(numberBlack[2].DOFade(1f, 0.5f))
		.AppendInterval(0.5f)
		
		//.Append(numberWhite[3].DOFade(0f, 0f))
		//.Append(numberBlack[3].DOFade(0f, 0f))
		.Append(numberWhite[3].GetComponent<RectTransform>().DOScale(new Vector3(numScale, numScale, numScale), 0f))
		.Append(numberBlack[3].GetComponent<RectTransform>().DOScale(new Vector3(numScale, numScale, numScale), 0f))
		//.Append(maskImage.GetComponent<RectTransform>().DOSizeDelta(new Vector2(576f * 0.3f, 576f * 0.3f), 0.5f).SetEase(Ease.OutCubic))
		.Join(numberWhite[3].GetComponent<RectTransform>().DOScale(Vector3.one, 0.5f).SetEase(Ease.OutCubic))
		.Join(numberBlack[3].GetComponent<RectTransform>().DOScale(Vector3.one, 0.5f).SetEase(Ease.OutCubic))
		//.Join(numberWhite[3].DOFade(1f, 0.5f))
		//.Join(numberBlack[3].DOFade(1f, 0.5f))
		.AppendInterval(0.5f)
		
		//.Append(numberWhite[4].DOFade(0f, 0f))
		//.Append(numberBlack[4].DOFade(0f, 0f))
		.Append(numberWhite[4].GetComponent<RectTransform>().DOScale(new Vector3(numScale, numScale, numScale), 0f))
		.Append(numberBlack[4].GetComponent<RectTransform>().DOScale(new Vector3(numScale, numScale, numScale), 0f))
		//.Append(maskImage.GetComponent<RectTransform>().DOSizeDelta(new Vector2(0f, 0f), 0.5f).SetEase(Ease.OutCubic))
		.Join(numberWhite[4].GetComponent<RectTransform>().DOScale(Vector3.one, 0.5f).SetEase(Ease.OutCubic))
		.Join(numberBlack[4].GetComponent<RectTransform>().DOScale(Vector3.one, 0.5f).SetEase(Ease.OutCubic));
		//.Join(numberWhite[4].DOFade(1f, 0.5f))
		//.Join(numberBlack[4].DOFade(1f, 0.5f));

		await ts.Task;
	}

	public async override Task Hide(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		var ts = new TaskCompletionSource<bool>();
		group.alpha = 1;
		sequence2?.Kill();
		sequence1 = DOTween.Sequence()
		//.Append(group.DOFade(0, 0.5f))
		.Append(maskImage.GetComponent<RectTransform>().DOSizeDelta(new Vector2(576f, 576f), 0.75f).SetEase(Ease.OutCubic))
		.Join(maskedImage.DOFade(0, 0.5f).SetDelay(0.25f))
		//.Join(group.GetComponent<RectTransform>().DOScale(n5ew Vector3(0.65f, 0.65f, 0.65f), 0.75f).SetEase(Ease.OutCubic))
		.OnComplete(() =>
		{
			group.alpha = 0f;
			gameObject.SetActive(false);
			ts.SetResult(true);
		});
		token.Register(() =>
		{
			ts.TrySetCanceled();
			sequence1?.Kill();
		});
		await ts.Task;
	}
}