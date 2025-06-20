using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class CircleView : IView
{
	[SerializeField] Image circle_1;
	[SerializeField] Image circle_2;
	[SerializeField] Image circle_3;
	[SerializeField] CanvasGroup group;
	Sequence sequence;
	bool isMove = false;
	double rotate = 0;
    bool isLaneThree = false;
	public bool IsLaneThree
    {
        get { return isLaneThree; }
        set { isLaneThree = value; }
    }

    public override void Init()
	{
		gameObject.SetActive(false);
		var color = Color.white;
		color.a = 0f;
		circle_1.color = circle_2.color = circle_3.color = color;
		group.alpha = 0.5f;
        isLaneThree = false;
		isShow = false;
    }

	void Update()
	{
		if (isMove)
		{
			var sharedContext = SharedContext.Instance;
            var clickPlayer = ClickPlayer.Instance;
            rotate = clickPlayer.TimeSpan.TotalSeconds / sharedContext.durQuater * 22.5;
			group.transform.localRotation = Quaternion.Euler(0f, 0f, (float)rotate);
			//circle_1.transform.localRotation = circle_2.transform.localRotation = circle_3.transform.localRotation = Quaternion.Euler(0f, 0f, (float)rotate);
		}
		else
		{
			group.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
			//circle_1.transform.localRotation = circle_2.transform.localRotation = circle_3.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		}
	}

	public async override Task Show(CancellationToken token)
	{
		token.ThrowIfCancellationRequested();
		
		isShow = true;
		gameObject.SetActive(true);

		isMove = true;

		var ts = new TaskCompletionSource<bool>();
        sequence = DOTween.Sequence()
        .Append(circle_1.DOFade(1f, 0.5f))
        .Join(circle_2.DOFade(1f, 0.5f));
        if(IsLaneThree) sequence.Join(circle_3.DOFade(1f, 0.5f));
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
		.Append(circle_1.DOFade(0f, 0.5f))
		.Join(circle_2.DOFade(0f, 0.5f))
		.Join(circle_3.DOFade(0f, 0.5f))
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
