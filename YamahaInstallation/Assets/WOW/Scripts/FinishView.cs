using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class FinishView : IView
{
	[SerializeField] public SceneManager sceneManager;
	Sequence sequence = null;
	[SerializeField] Particles particles;
	[SerializeField] ScoreView scoreView;
	[SerializeField] NumberView numberView;
	[SerializeField] CanvasGroup logoGroup;
	[SerializeField] Image logo1;
	[SerializeField] Image logo2;

	private CancellationTokenSource cts = null;

	public override void Init()
	{
		gameObject.SetActive(false);
		scoreView.Init();
		logoGroup.alpha = 0f;
	}

	public async override Task Show(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		var config = SharedConfig.Instance.config;
		var context = SharedContext.Instance.context;
		gameObject.SetActive(true);

		logoGroup.alpha = 0f;
		scoreView.Init();
		scoreView.Score = Math.Max(context.score, 325);

		SEPlayer.Instance.PlaySE(1);
        
        numberView.Score = SharedContext.Instance.context.playerId;
        var arrayTask = new List<Task>();
		arrayTask.Add(numberView.Show(token));
		arrayTask.Add(scoreView.Show(token));
		await Task.WhenAll(arrayTask);

		//await scoreView.Show(token);
		await Task.Delay(3000, cancellationToken: token);
		await particles.RefreshAll(token);
		//await scoreView.Hide(token);

		var arrayTask2 = new List<Task>();
		arrayTask2.Add(numberView.Hide(token));
		arrayTask2.Add(scoreView.Hide(token));
		await Task.WhenAll(arrayTask2);

		await ShowLogo(token);
		await Task.Delay(5000, cancellationToken: token);
		sceneManager.Goto(Scene.Standby);
	}

	public async override Task Hide(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		cts?.Cancel();
		var ts = new TaskCompletionSource<bool>();
		sequence?.Kill(false);
		sequence = DOTween.Sequence()
		.Append(logoGroup.DOFade(0, 0.5f))
		.OnComplete(async () =>
		{
			sequence?.Kill(false);
			sequence = null;
			gameObject.SetActive(false);
			ts.SetResult(true);
		});
		token.Register(() =>
		{
			sequence?.Kill(false);
			sequence = null;
			gameObject.SetActive(false);
			ts.TrySetCanceled();
		});
		await ts.Task;
	}

	async Task ShowLogo(CancellationToken token)
	{
		gameObject.SetActive(true);
		token.ThrowIfCancellationRequested();
		var ts = new TaskCompletionSource<bool>();
		//logo1.GetComponent<RectTransform>().localScale =
		//logo2.GetComponent<RectTransform>().localScale = Vector3.one* 1.3f;
		sequence?.Kill(false);
		sequence = DOTween.Sequence()
		.Append(logoGroup.DOFade(1, 1f))
		//.Join(logo1.GetComponent<RectTransform>().DOScale(Vector3.one, 1f))
		//.Join(logo2.GetComponent<RectTransform>().DOScale(Vector3.one, 1f))
		.OnComplete(() =>
		{
			sequence?.Kill(false);
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
}