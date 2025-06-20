using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(EchoTraceRenderer), PostProcessEvent.BeforeStack, "Custom/EchoTrace")]
public sealed class EchoTrace : PostProcessEffectSettings
{
    [Range(0f, 1f), Tooltip("EchoTrace effect intensity.")]
    public FloatParameter intensity = new FloatParameter { value = 0.5f };
    [Tooltip("EchoTrace flow persistence.")]
    public BoolParameter difference = new BoolParameter { value = true };
    [Range(0f, 1f), Tooltip("EchoTrace effect scale.")]
    public FloatParameter gain = new FloatParameter { value = 0.5f };
    [Range(0f, 1f), Tooltip("EchoTrace flow persistence.")]
    public FloatParameter threshold = new FloatParameter { value = 0.9f };
    [Tooltip("EchoTrace flow persistence.")]
    public BoolParameter invert = new BoolParameter { value = true };
    private RenderTexture _prevTexture;
    public RenderTexture prevTexture
    {
        get
        {
            if (_prevTexture == null)
            {
                _prevTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
                _prevTexture.Create();
            }

            return _prevTexture;
        }
    }
}

public sealed class EchoTraceRenderer : PostProcessEffectRenderer<EchoTrace>
{
    public override void Render(PostProcessRenderContext context)
    {
        context.command.BeginSample("EchoTrace");

        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/EchoTrace"));

        //0
        sheet.properties.SetFloat("_Intensity", settings.intensity);
        sheet.properties.SetInt("_Difference", settings.difference ? 1 : 0);
        sheet.properties.SetFloat("_Gain", settings.gain);
        sheet.properties.SetFloat("_Threshold", settings.threshold);
        sheet.properties.SetInt("_Invert", settings.invert ? 1 : 0);
        sheet.properties.SetTexture("_PrevTex", settings.prevTexture);

        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
       
        //
        context.command.BlitFullscreenTriangle(context.destination, settings.prevTexture);

        context.command.EndSample("EchoTrace");
    }
}