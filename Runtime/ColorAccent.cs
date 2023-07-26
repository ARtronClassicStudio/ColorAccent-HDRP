using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[Serializable, VolumeComponentMenu("Color Accent")]
public class ColorAccent : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    public BoolParameter enabled = new(false, false);
    public ColorParameter accentColor = new(Color.red, false);
    public ColorParameter logicColor = new(new(0.5843138f, 0.9921569f, 0.6817982f, 1), false);
    public VolumeParameter<Logic> logic = new() { value = Logic.Color };
    public ClampedFloatParameter lerp = new(1, 0, 1, false);
    public FloatParameter threshold = new(0.3f, false);
    public FloatParameter contrast = new(2, false);
    public BoolParameter invert = new(false, false);


    public enum Logic
    {
        Const,
        Color,
        Luminance,
    }

    Material m_Material;

    public bool IsActive() => enabled.value;

    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.AfterPostProcess;

    const string kShaderName = "Hidden/ColorAccent";

    public override void Setup()
    {
       
        if (Shader.Find(kShaderName) != null)
            m_Material = new Material(Shader.Find(kShaderName));
        else
            Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume Code is unable to load.");
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;

        m_Material.SetColor(Shader.PropertyToID("_AccentColor"), accentColor.value);
        m_Material.SetColor(Shader.PropertyToID("_LogicColor"), logicColor.value);
        m_Material.SetFloat(Shader.PropertyToID("_Threshold"), threshold.value);
        m_Material.SetFloat(Shader.PropertyToID("_Contrast"), contrast.value);
        m_Material.SetFloat(Shader.PropertyToID("_Lerp"), lerp.value);
        m_Material.SetFloat(Shader.PropertyToID("_Invert"), Convert.ToSingle(invert.value));


        switch (logic.value)
        {
            case Logic.Const: m_Material.SetFloat(Shader.PropertyToID("_Logic"), 0); break;
            case Logic.Color: m_Material.SetFloat(Shader.PropertyToID("_Logic"), 1); break;
            case Logic.Luminance: m_Material.SetFloat(Shader.PropertyToID("_Logic"), 2); break;
        }
        m_Material.SetTexture("_MainTex", source);
        cmd.Blit(source, destination, m_Material, 0);
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
    }

}