Shader "Custom/HoldShineShader"
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
				float brightness = clamp( abs(sin(_Time * 220) * 0.33), 0, 1);
				float contrast = 1;
				float saturation = 1;
				//brigtness����ֱ�ӳ���һ��ϵ����Ҳ����RGB�������ţ���������
				fixed3 finalColor = renderTex + brightness;
				//saturation���Ͷȣ����ȸ��ݹ�ʽ����ͬ����������±��Ͷ���͵�ֵ��
				fixed gray = 0.2125 * renderTex.r + 0.7154 * renderTex.g + 0.0721 * renderTex.b;
				fixed3 grayColor = fixed3(gray, gray, gray);
				//����Saturation�ڱ��Ͷ���͵�ͼ���ԭͼ֮���ֵ
				finalColor = lerp(grayColor, finalColor, saturation);
				//contrast�Աȶȣ����ȼ���Աȶ���͵�ֵ
				fixed3 avgColor = fixed3(0.5, 0.5, 0.5);
				//����Contrast�ڶԱȶ���͵�ͼ���ԭͼ֮���ֵ
				finalColor = lerp(avgColor, finalColor, contrast);
				//���ؽ����alphaͨ������
				return fixed4(finalColor, renderTex.a);
			}
			ENDCG
		}
	}
	//��ֹshaderʧЧ�ı��ϴ�ʩ
	FallBack Off
}