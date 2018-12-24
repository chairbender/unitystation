//same as Sprite Fov Masked.shader but designed specifically for occluding wallmounts
//(which the normal Sprite Fov Mask doesn't do)

Shader "Stencil/Wall unlit background masked" {
	Properties{
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
	}

		SubShader{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100

		//Stencil{
		//	Ref 1
		//	Comp equal
		//}


		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fog

#include "UnityCG.cginc"

	struct appdata_t 
	{
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
		float4 color : COLOR;
	};

	struct v2f {
		float4 vertex : SV_POSITION;
		half2 texcoord : TEXCOORD0;
		half2 screencoord : TEXCOORD1;
		float4 color : COLOR;
	};

	sampler2D _MainTex;
	sampler2D _WallFovMask;
	float4 _FovMaskTransformation;
	float4 _MainTex_ST;

	v2f vert(appdata_t v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

		o.screencoord = (ComputeScreenPos(o.vertex) - 0.5 + _FovMaskTransformation.xy) * _FovMaskTransformation.zw + 0.5; 
		o.color = v.color;

		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		fixed4 textureSample = tex2D(_MainTex, i.texcoord);
		fixed4 maskSample = tex2D(_WallFovMask, i.screencoord);

		fixed4 final = textureSample * i.color;

        maskSample.r = maskSample.a;
        maskSample.a = 1;
        maskSample.g = 0;
        maskSample.b = 0;

        //return maskSample;

        if (maskSample.r > 0.5)
        {
            final.a = textureSample.a;
        }
        else
        {
            final.a = 0;
        }


        return final;
	}
		ENDCG
	}
	}

}