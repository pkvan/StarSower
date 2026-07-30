// Shader cong sang toi gian cho FX nhat sao (S2-005).
//
// Vi sao phai tu viet: URP/2D/Sprite-Unlit-Default hardcode "Blend SrcAlpha OneMinusSrcAlpha"
// trong SubShader, khong co property nao doi duoc blend, nen khong the lam additive bang material.
//
// Co tinh KHONG dung include cua URP: day chi la mot pass unlit trong suot, khong can anh sang,
// khong can keyword cua pipeline. Nho vay no chay duoc o ca Built-in lan URP va an toan tren iOS.
// Mau cua SpriteRenderer di vao qua vertex color nen PooledStarFX van fade duoc bang renderer.color.
Shader "StarSower/StarFX Additive"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha One

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                // Nhan truoc voi alpha: nho vay ha alpha la ha do sang, fade ra dan chu khong
                // bi giu nguyen do choi roi bien mat dot ngot nhu additive thuan.
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }

    Fallback "Sprites/Default"
}
