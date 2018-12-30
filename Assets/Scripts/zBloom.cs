using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class zBloom : MonoBehaviour {

    public Shader shader;
    public Texture2D mask;
    public Vector4 color = new Vector4(0.7f,0.75f,0.01f,0.75f);
    public Vector4 intensity = new Vector4(1,1.64f,2.87f,1.69f);
    public float attenuation = 0.05f;
    public float scale = 2f;
	public bool allowVR = true;
    Material mat;
    Camera cam;

    int _Scale;
    int _Intensity;
    int _Color;
    int _Attenuation;
    int _Level1;
    int _Level2;
    int _Level3;
    int _Level4;
    int _Level5;
    int _Mask;

    private void OnEnable() {

        _Scale       = Shader.PropertyToID("_Scale");
        _Intensity   = Shader.PropertyToID("_Intensity");
        _Color       = Shader.PropertyToID("_Color");
        _Attenuation = Shader.PropertyToID("_Attenuation");
        _Level1      = Shader.PropertyToID("_Level1");
        _Level2      = Shader.PropertyToID("_Level2");
        _Level3      = Shader.PropertyToID("_Level3");
        _Level4      = Shader.PropertyToID("_Level4");
        _Level5      = Shader.PropertyToID("_Level5");
        _Mask        = Shader.PropertyToID("_Mask");

        mat = new Material(shader);
        mat.SetTexture(_Mask, mask);
        cam = GetComponent<Camera>();
    }

    RenderTexture GetTex(RenderTexture reference, int divisor) {
        return RenderTexture.GetTemporary(
            reference.width / divisor,
            reference.height / divisor, 0,
            RenderTextureFormat.DefaultHDR,
            RenderTextureReadWrite.Default, 1);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {

        var c0 = GetTex(source, 1);
        var d1 = GetTex(source, 2);
        var d2 = GetTex(source, 4);
        var d3 = GetTex(source, 8);
        //var u2 = GetTex(source, 4);
        //var u1 = GetTex(source, 2);

        Graphics.Blit(source, c0, mat, 4); // color grading

        Graphics.Blit(c0, d1, mat, 0); // prefilter + down
        Graphics.Blit(d1, d2, mat, 1); // down
        Graphics.Blit(d2, d3, mat, 1); // down
        //Graphics.Blit(d3, u2, mat, 2); // up
        //Graphics.Blit(u2, u1, mat, 2); // up

        mat.SetFloat(_Scale, scale);
        mat.SetVector(_Intensity, intensity);
        mat.SetVector(_Color, color);
        mat.SetFloat(_Attenuation, attenuation);

        mat.SetTexture(_Level1, d1);
        mat.SetTexture(_Level2, d2);
        mat.SetTexture(_Level3, d3);
        //mat.SetTexture(_Level4, u2);
        //mat.SetTexture(_Level5, u1);
        Graphics.Blit(c0, destination, mat, 3); // up
        
        RenderTexture.ReleaseTemporary(c0);
        RenderTexture.ReleaseTemporary(d1);
        RenderTexture.ReleaseTemporary(d2);
        RenderTexture.ReleaseTemporary(d3);
        //RenderTexture.ReleaseTemporary(u2);
        //RenderTexture.ReleaseTemporary(u1);
    }

}