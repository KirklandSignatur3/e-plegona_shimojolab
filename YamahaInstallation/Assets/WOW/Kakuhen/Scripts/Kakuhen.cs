using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Kakuhen : MonoBehaviour
{
    [SerializeField] public Particles particles;
    [SerializeField] public ColorManager colorManager;
    [SerializeField] GameObject circleOrigin;
    [SerializeField] GameObject rectOrigin;
    [SerializeField] GameObject lineOrigin;
    [SerializeField] KakuhenLights lights;
    [SerializeField] KakuhenEdges edges;
    Sequence sequence;
    bool isOn = false;
    float beatReaction = 0;
    int emitCount = 0;

    [ContextMenu("Kakuhen On")]
    public void On()
    {
        isOn = true;
        DOTween.To(() => particles.kakuhen, (x) => particles.kakuhen = x, 1f, 0.1f);
        ClickPlayer.Instance.onClickEvent.AddListener(Click);
    }

    [ContextMenu("Kakuhen Off")]
    public void Off()
    {
        isOn = false;
        DOTween.To(() => particles.kakuhen, (x) => particles.kakuhen = x, 0f, 0.1f);
        ClickPlayer.Instance.onClickEvent.RemoveListener(Click);
    }

    void Update()
    {
        var config = SharedConfig.Instance.config;
        var context = SharedContext.Instance.context;

        if (isOn)
        {
            emitCount++;
            if (emitCount > 10)
            {
                var colors = colorManager.GetColors((Player)(Random.Range(0, ((int)Player.Two) + 1)));
                lights.Add(colors);
                edges.Add(colors);
                emitCount = 0;
            }

            lights.beatReaction = edges.beatReaction = beatReaction;
        }
    }

    public void Click()
    {
        sequence = DOTween.Sequence()
        .Append(DOTween.To(() => beatReaction, (x) => beatReaction = x, 1.1f, 0.001f))
        .Append(DOTween.To(() => beatReaction, (x) => beatReaction = x, 1f, 0.75f));

        for (int i = 0; i < 10; i++)
        {
            var p = UnityEngine.Random.onUnitSphere;
            p = Vector3.Normalize(p);
            p *= Random.Range(0f, 1.5f);
            p.z = -1;
            var circle = Instantiate(circleOrigin, p, Quaternion.identity, this.transform).GetComponent<KakuhenCircle>();
            circle.Show();
        }

		/*
        for (int i = 0; i < 10; i++)
        {
            var p = UnityEngine.Random.onUnitSphere;
            p = Vector3.Normalize(p);
            p *= Random.Range(0f, 1.4f);
            var rect = Instantiate(rectOrigin, p, Quaternion.identity, this.transform).GetComponent<KakuhenRect>();
            rect.Show();
        }
		*/

        for (int i = 0; i < 15; i++)
        {
            var p = UnityEngine.Random.onUnitSphere;
            p = Vector3.Normalize(p);
            p *= Random.Range(0f, 1.5f);
            p.z = -1;
            var line = Instantiate(lineOrigin, p, Quaternion.identity, this.transform).GetComponent<KakuhenLine>();
            line.Show();
        }
    }
}
