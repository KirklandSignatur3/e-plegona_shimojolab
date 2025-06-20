using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using LabJack.LabJackUD;



public class StanbyView : IView
{
    [SerializeField] public SceneManager sceneManager;
    [SerializeField] AbletonManager abletonManager;
    [SerializeField] CanvasGroup textGroup;
    [SerializeField] CanvasGroup buttonGroup;
    [SerializeField] EventTrigger button1;
    //[SerializeField] EventTrigger button2;
    [SerializeField] Image image_1;
    [SerializeField] Image image_2;
    [SerializeField] TextMeshProUGUI text_1;
    [SerializeField] TextMeshProUGUI text_2;

    private string text1 = "What revs your heart";
    //private string text2 = "and makes waves?";
    private string text2 = "and [TEST EDIT]?";


    Sequence sequence;
	Tween tween1, tween2;

    /// <summary>
    /// 
    /// </summary>
    public override void Init()
    {
        gameObject.SetActive(false);
        textGroup.alpha = 0;
        buttonGroup.alpha = 0;
        button1.enabled = false;
        //button2.enabled = false;
        //button1.gameObject.GetComponent<Image>().DOFade(1f, 0f);
		//button2.gameObject.GetComponent<Image>().DOFade(1f, 0f);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async override Task Show(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        gameObject.SetActive(true);
        abletonManager.Pause();
        abletonManager.SpecialZero();

        var ts = new TaskCompletionSource<bool>();

        var duration_1 = text1.Length * 0.05f;
        var duration_2 = text2.Length * 0.05f;
        var width1 = 364;
        var width2 = 364;

        text_1.text = text_2.text = "";
		var color = Color.white;
		color.a = 1f;
		//button1.gameObject.GetComponent<Image>().DOFade(1f, 0f);
		//button2.gameObject.GetComponent<Image>().DOFade(1f, 0f);
        image_1.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 0);
        image_2.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 0);

        buttonGroup.alpha = 0;
        textGroup.alpha = 1;

        SEPlayer.Instance.PlaySE(0);

        sequence = DOTween.Sequence()
        .Append(text_1.DOText(text1, duration_1).SetEase(Ease.InOutCubic))
        .Join(text_2.DOText(text2, duration_2).SetEase(Ease.InOutCubic))
        .Join(image_1.GetComponent<RectTransform>().DOSizeDelta(new Vector2(50, width1), duration_1).SetEase(Ease.InOutCubic))
        .Join(image_2.GetComponent<RectTransform>().DOSizeDelta(new Vector2(50, width2), duration_2).SetEase(Ease.InOutCubic))
        .Join(buttonGroup.DOFade(1, 0.5f))
        .OnComplete(() =>
        {
			//tween1 = button1.gameObject.GetComponent<Image>().DOFade(0.4f, 0.4f).SetLoops(-1, LoopType.Yoyo);
			//tween2 = button2.gameObject.GetComponent<Image>().DOFade(0.4f, 0.4f).SetLoops(-1, LoopType.Yoyo);
            
			sequence.Kill(false);
            sequence = null;
        	button1.enabled = true;//button2.enabled = true;
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public async override Task Hide(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var ts = new TaskCompletionSource<bool>();
		
		button1.enabled = false;//button2.enabled = false;
		tween1?.Kill(false);
		//tween2?.Kill(false);
		tween1 = tween2 = null;

        sequence = DOTween.Sequence()
        .Append(textGroup.DOFade(0, 0.5f))
        .Join(buttonGroup.DOFade(0, 0.5f))
        .OnComplete(() =>
        {
            sequence.Kill(false);
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

    /// <summary>
    /// 
    /// </summary>
    public void OnTapDownPad()
    {
        SEPlayer.Instance.PlaySE(1);
        sceneManager.Goto(Scene.Game);
    }
}