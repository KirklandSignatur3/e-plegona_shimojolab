using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(OpticalDistortRenderer), PostProcessEvent.BeforeStack, "Custom/OpticalDistort")]
public sealed class OpticalDistort : PostProcessEffectSettings
{
    [Range(0f, 1f), Tooltip("OpticalDistort effect intensity.")]
    public FloatParameter intensity = new FloatParameter { value = 0.5f };
    [Range(0f, 10f), Tooltip("OpticalDistort effect scale.")]
    public FloatParameter scale = new FloatParameter { value = 4.5f };
    [Range(0f, 1f), Tooltip("OpticalDistort flow persistence.")]
    public FloatParameter maskHold = new FloatParameter { value = 0.9f };
    [Range(0f, 10f), Tooltip("OpticalDistort effect offset.")]
    public FloatParameter offset = new FloatParameter { value = 0.01f };
    [Range(0f, 1f), Tooltip("OpticalDistort noise removal.")]
    public FloatParameter lamda = new FloatParameter { value = 0.1f };

    private RenderTexture _prevTexture;
    private RenderTexture _tempMaskTexture;
    private RenderTexture _maskTexture;
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
    public RenderTexture tempMaskTexture
    {
        get
        {
            if (_tempMaskTexture == null)
            {
                _tempMaskTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
                _tempMaskTexture.Create();
            }

            return _tempMaskTexture;
        }
    }

    public RenderTexture maskTexture
    {
        get
        {
            if (_maskTexture == null)
            {
                _maskTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32);
                _maskTexture.Create();
            }

            return _maskTexture;
        }
    }
}


public sealed class OpticalDistortRenderer : PostProcessEffectRenderer<OpticalDistort>
{
    public override void Render(PostProcessRenderContext context)
    {
        context.command.BeginSample("OpticalDistort");

        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/OpticalDistort"));

        //0
        sheet.properties.SetFloat("_MaskHold", settings.maskHold);
        sheet.properties.SetFloat("_Scale", settings.scale);
        sheet.properties.SetFloat("_Offset", settings.offset);
        sheet.properties.SetFloat("_Lamda", settings.lamda);

        sheet.properties.SetTexture("_PrevTex", settings.prevTexture);
        sheet.properties.SetTexture("_MaskTex", settings.maskTexture);

        context.command.BlitFullscreenTriangle(context.source, settings.tempMaskTexture, sheet, 0);

        sheet.properties.SetFloat("_Intensity", settings.intensity);
        sheet.properties.SetTexture("_MaskTex", settings.tempMaskTexture);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 1);

        //
        context.command.BlitFullscreenTriangle(settings.tempMaskTexture, settings.maskTexture);
        context.command.BlitFullscreenTriangle(context.source, settings.prevTexture);

        context.command.EndSample("OpticalDistort");
    }
}