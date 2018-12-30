Shader "Hidden/zBloom"
{
    Properties
    {
        _MainTex ("", 2D) = "" {}
        _BaseTex ("", 2D) = "" {}
        _Level1 ("", 2D) = "" {}
        _Level2 ("", 2D) = "" {}
        _Level3 ("", 2D) = "" {}
        //_Level4 ("", 2D) = "" {}
        //_Level5 ("", 2D) = "" {}
        _Mask ("", 2D) = "" {}
    }

	CGINCLUDE
    #pragma target 3.0

	#include "UnityCG.cginc"
	#define MOBILE_OR_CONSOLE (defined(SHADER_API_MOBILE) || defined(SHADER_API_PSSL) || defined(SHADER_API_XBOXONE) || defined(SHADER_API_WIIU))

	sampler2D _MainTex;
	float4 _MainTex_TexelSize;
	float4 _MainTex_ST;

	sampler2D _BaseTex; float4 _BaseTex_TexelSize;
	sampler2D _Level1;  float4 _Level1_TexelSize;
	sampler2D _Level2;  float4 _Level2_TexelSize;
	sampler2D _Level3;  float4 _Level3_TexelSize;
	//sampler2D _Level4;  float4 _Level4_TexelSize;
	//sampler2D _Level5;  float4 _Level5_TexelSize;

    sampler2D _Mask;
    half4 _Intensity;
    half _Scale;
	half _Attenuation;

    half4 _Color;

	struct appdata
	{
		float4 vertex : POSITION;
		float4 texcoord : TEXCOORD0;
	};

    struct v2f 
    {
        float4 pos : SV_POSITION;
        float2 uv : TEXCOORD0;
    };

    v2f vert(appdata v)
    {
        v2f o;
        o.pos = UnityObjectToClipPos(v.vertex);
        o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord.xy, _MainTex_ST);

//	#if UNITY_UV_STARTS_AT_TOP
//        if (_BaseTex_TexelSize.y < 0.0)
//            o.uvBase.y = 1.0 - o.uvBase.y;
//	#endif

        return o; 
    }

	inline half4 Sample4(sampler2D tex, half2 uv, half2 size) {
		return (
			tex2D(tex, uv + size.xy * _Scale * half2( 0,-1)) +
			tex2D(tex, uv + size.xy * _Scale * half2( 0,+1)) +
			tex2D(tex, uv + size.xy * _Scale * half2(+1, 0)) +
			tex2D(tex, uv + size.xy * _Scale * half2(-1, 0))
		)/4.0;
	}

    half4 frag0(v2f i) : SV_Target {
		half4 c = tex2D(_MainTex, i.uv);
		c.gb *= _Color.x;
		half m = (c.r + c.g + c.b)/3;
		c.rgb = (c.rgb - m) * _Color.y + m;
		c.rgb = (c.rgb + _Color.z) * _Color.w;
		return c;
    }		

    half4 frag1(v2f i) : SV_Target {
		half4 s = Sample4(_MainTex, i.uv, _MainTex_TexelSize);

        // Pixel brightness
        half br = max(s.r, max(s.g, s.b));

        // Under-threshold part: quadratic curve
        half rq = clamp(br - _Intensity.x, 0.0, _Intensity.y);
        rq = _Intensity.z * rq * rq;

        // Combine and apply the brightness response curve.
        s *= max(rq, br - _Intensity.w) / max(br, 1e-5);

		return s;
    }		

    half4 frag2(v2f i) : SV_Target {
		return Sample4(_MainTex, i.uv, _MainTex_TexelSize);
    }		

    half4 frag3(v2f i) : SV_Target {
		return Sample4(_MainTex, i.uv, _MainTex_TexelSize * 0.5);
    }		

    half4 frag4(v2f i) : SV_Target {
        half4 d1 = Sample4(_Level1, i.uv, _Level1_TexelSize);
        half4 d2 = Sample4(_Level2, i.uv, _Level2_TexelSize);
        half4 d3 = Sample4(_Level3, i.uv, _Level3_TexelSize);
        //half4 d4 = Sample4(_Level4, i.uv, _Level4_TexelSize);
        //half4 d5 = Sample4(_Level5, i.uv, _Level5_TexelSize);
        half4 mask = tex2D(_Mask, i.uv);
        half4 main = tex2D(_MainTex, i.uv);
		half4 s = saturate((d1*0.5 + d2 + d3*2 /* + d4*0.75 + d5 */) * _Attenuation);
		return main + mask * s;
    } 

	ENDCG

    SubShader
    {
        ZTest Always Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag1
            ENDCG
        }

        Pass
        {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag2
            ENDCG
        }

        Pass
        {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag3
            ENDCG
        }

        Pass
        {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag4
            ENDCG
        }

        Pass
        {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag0
            ENDCG
        }
		
    }
}