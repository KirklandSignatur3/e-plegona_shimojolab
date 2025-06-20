using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Shapes;

public class KakuhenLine : MonoBehaviour
{
	Sequence sequence = null;
	[SerializeField] Line line;
	float alpha = 0f;
	float begin = 0;
	float end = 0;

	public void Show()
	{
		alpha = 1;
		Color color = Color.white;
		color.a = alpha;
		line.Color = color;
		this.transform.localScale = new Vector3(1f, 1f, 1f);

		var start = Random.Range(0f, 0.5f);
		var goal = Random.Range(start, -0.5f);
		begin = end = start;
		
		sequence = DOTween.Sequence()
		.Append(DOTween.To(() => end, (x) => end = x, goal, 1f).SetEase(Ease.InOutCubic))
		.Join(DOTween.To(() => begin, (x) => begin = x, goal, 1f).SetDelay(0.5f).SetEase(Ease.InOutCubic))
		.OnComplete(() =>
		{
			sequence.Kill(false);
			sequence = null;
			Destroy(this.gameObject);
		});
	}

	private void OnDestory()
	{
		sequence?.Kill();
	}

	private void Update()
	{
		line.Start = new Vector3(begin, begin, 0);
		line.End = new Vector3(end, end, 0);
	}
}