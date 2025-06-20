using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(BlackRenderer), PostProcessEvent.BeforeStack, "Custom/Black")]
public sealed class Black : PostProcessEffectSettings
{
    [Range(0f, 1f), Tooltip("Black effect intensity.")]
    public FloatParameter intensity = new FloatParameter { value = 0.5f };
}

public sealed class BlackRenderer : PostProcessEffectRenderer<Black>
{
    public override void Render(PostProcessRenderContext context)
    {
        context.command.BeginSample("Black");
       
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/Black"));
        sheet.properties.SetFloat("_Intensity", settings.intensity);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);

        context.command.EndSample("Black");
    }
}