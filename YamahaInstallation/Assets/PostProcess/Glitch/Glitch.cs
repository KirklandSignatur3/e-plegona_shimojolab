using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(GlitchRenderer), PostProcessEvent.BeforeStack, "Custom/Glitch")]
public sealed class Glitch : PostProcessEffectSettings
{
    [Range(0f, 1f), Tooltip("Glitch effect intensity.")]
    public FloatParameter intensity = new FloatParameter {value = 0.5f};
    [Range(0f, 1f), Tooltip("Glitch effect scale.")]
    public FloatParameter scale = new FloatParameter { value = 1.0f };
    [Range(0f, 1.0f), Tooltip("Glitch effect speed.")]
    public FloatParameter speed = new FloatParameter { value = 1.0f };
    [Range(0f, 1.0f), Tooltip("Glitch effect seed.")]
    public FloatParameter seed = new FloatParameter { value = 1.0f };
}

public sealed class GlitchRenderer : PostProcessEffectRenderer<Glitch>
{
    public override void Render(PostProcessRenderContext context)
    {
        context.command.BeginSample("Glitch");

        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/Glitch"));
        sheet.properties.SetFloat("_Intensity", settings.intensity);
        sheet.properties.SetFloat("_Scale", settings.scale);
        sheet.properties.SetFloat("_Speed", settings.speed);
        sheet.properties.SetFloat("_Seed", settings.seed);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0); 
        context.command.EndSample("Glitch");
    }
}