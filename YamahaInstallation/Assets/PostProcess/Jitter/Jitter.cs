using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(JitterRenderer), PostProcessEvent.BeforeStack, "Custom/Jitter")]
public sealed class Jitter : PostProcessEffectSettings
{
    [Range(0f, 1f), Tooltip("Jitter effect intensity.")]
    public FloatParameter intensity = new FloatParameter {value = 0.5f};
    [Range(0f, 1f), Tooltip("Jitter effect scale.")]
    public FloatParameter scale = new FloatParameter { value = 1.0f };
    [Range(0F, 1F)]
    public FloatParameter threshold = new FloatParameter { value = 1.0f };
}

public sealed class JitterRenderer : PostProcessEffectRenderer<Jitter>
{
    public override void Render(PostProcessRenderContext context)
    {
        context.command.BeginSample("Jitter");

        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/Jitter"));
        sheet.properties.SetFloat("_Intensity", settings.intensity);
        sheet.properties.SetFloat("_Scale", settings.scale);
        sheet.properties.SetFloat("_Threshold", settings.threshold);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0); 
        context.command.EndSample("Jitter");
    }
}