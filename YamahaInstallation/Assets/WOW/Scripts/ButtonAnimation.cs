using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.EventSystems;

public class ButtonAnimation : MonoBehaviour
{
	[SerializeField] Image image;
	[SerializeField] Image imageLight;
	[SerializeField] Image imageExcuse;
	[SerializeField] Image imageTap;
	Color baseColor = Color.white;
	Sequence sequence;
	Sequence tapSequence;
	bool isShow = false;
	float tapFade = 0f;
	void Start()
	{
		baseColor = Color.white;
		var color = baseColor;
		color.a = 0f;
		image.color = imageLight.color = imageTap.color = imageExcuse.color = color;
	}

	void Update()
	{
		if (isShow)
		{
			var clickPlayer = ClickPlayer.Instance;
			var color1 = baseColor;
			color1.a = clickPlayer.beatReaction * 0.7f + 0.3f;
			image.color = color1;
			var color2 = baseColor;
			color2.a = clickPlayer.beatReaction * 0.8f + 0.2f;
			imageLight.color = color2;
			var color3 = baseColor;
			color3.a = tapFade;
			imageTap.color = color3;
		}
		else
		{
			var color = Color.white;
			color.a = 0f;
			image.color = imageLight.color = color;
		}
	}

	public void Show()
	{
		isShow = true;
		baseColor = Color.white;
		var color = baseColor;
		color.a = 0f;
		image.color = imageLight.color = imageTap.color = imageExcuse.color = color;
	}

	public void Hide()
	{
		isShow = false;
		var color = baseColor;
		color.a = 0f;
		image.color = color;
	}

	public void SetRed()
	{
		Color color;
		ColorUtility.TryParseHtmlString("#D21E2D", out color);

		tapSequence?.Kill();
		tapSequence = DOTween.Sequence()
		.Append(DOTween.To(() => baseColor, (x) => baseColor = x, color, 0.2f))
		.AppendInterval(1f)
		.Append(DOTween.To(() => baseColor, (x) => baseColor = x, Color.white, 0.4f));
	}

	public void SetWhite()
	{
		Color color = Color.white;
		DOTween.To(() => baseColor, (x) => baseColor = x, color, 0.2f);
	}

	public void Tap()
	{
		DOTween.To(() => tapFade, (x) => tapFade = x, 1f, 0f);
		DOTween.To(() => tapFade, (x) => tapFade = x, 0f, 0.3f);
		imageTap.DOFade(0f, 0.3f);
		imageTap.GetComponent<RectTransform>().DOScale(1f, 0f);
		imageTap.GetComponent<RectTransform>().DOScale(2f, 0.6f).SetEase(Ease.OutCubic);
	}

	public async Task Excuse(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		var ts = new TaskCompletionSource<bool>();

		sequence = DOTween.Sequence()
		.Append(imageExcuse.DOFade(0f, 0f))
		.Join(imageExcuse.GetComponent<RectTransform>().DOScale(1.8f, 0f))
		.Append(imageExcuse.DOFade(0.5f, 0.2f))
		.Join(imageExcuse.GetComponent<RectTransform>().DOScale(1f, 0.6f).SetEase(Ease.OutCubic))
		.Append(imageExcuse.DOFade(0f, 0.4f))
		.AppendInterval(0.2f)
		.SetLoops(3, LoopType.Restart)
		.OnComplete(() =>
		{
			ts.SetResult(true);
		});

		token.Register(() =>
		{
			ts.TrySetCanceled();
			sequence?.Kill(false);
			sequence = null;
		});

		await ts.Task;
	}

	public async Task ExcuseHold(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();

		var ts = new TaskCompletionSource<bool>();

		sequence = DOTween.Sequence()
		.Append(imageExcuse.DOFade(0f, 0f))
		.Join(imageExcuse.GetComponent<RectTransform>().DOScale(1.8f, 0f))
		.Append(imageExcuse.DOFade(0.5f, 0.2f))
		.Join(imageExcuse.GetComponent<RectTransform>().DOScale(1f, 0.6f).SetEase(Ease.OutCubic))
		.AppendInterval(1.2f)
		.Append(imageExcuse.DOFade(0f, 0.4f))
		.OnComplete(() =>
		{
			ts.SetResult(true);
		});

		token.Register(() =>
		{
			ts.TrySetCanceled();
			sequence?.Kill(false);
			sequence = null;
		});

		await ts.Task;
	}
}