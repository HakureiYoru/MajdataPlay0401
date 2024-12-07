Shader "Custom/BrightnessAdjustAnimate"
{
	Properties
	{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"RenderType" = "Transparent"
			}
			Pass
				{
			ZTest Always
			Cull Off
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			sampler2D _MainTex;
			half _Brightness;
			half _Saturation;
			half _Contrast;

			//vert��frag����
			#pragma vertex vert
			#pragma fragment frag
			#include "Lighting.cginc"


			struct appdata_t
			{
				float4 vertex : POSITION;
				half4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};
			//��vertex shader����pixel shader�Ĳ���
			struct v2f
			{
				float4 pos : SV_POSITION; //����λ��
				half2  uv : TEXCOORD0;	  //UV����
				half4 color : COLOR;
			};

			//vertex shader
			v2f vert(appdata_t v)
			{
				v2f o;
				//������ռ�ת��ͶӰ�ռ�
				o.pos = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				//uv���긳ֵ��output
				o.uv = v.texcoord;
				return o;
			}

			//fragment shader
			fixed4 frag(v2f i) : COLOR
			{
				//��_MainTex�и���uv������в���
				fixed4 renderTex = tex2D(_MainTex, i.uv)*i.color;
				float brightness = 1;
				brightness = clamp( sin(_Time * 260) * 0.2, 0, 1);
				//brigtness����ֱ�ӳ���һ��ϵ����Ҳ����RGB�������ţ���������
				fixed3 finalColor = renderTex + brightness;
				//���ؽ����alphaͨ������
				return fixed4(finalColor, renderTex.a);
			}
			ENDCG
		}
	}
	//��ֹshaderʧЧ�ı��ϴ�ʩ
	FallBack Off
}