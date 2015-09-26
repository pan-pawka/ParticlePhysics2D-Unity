#ifndef ParticlePhysics2D_QuadBlit
#define ParticlePhysics2D_QuadBlit

struct appdata_QuadBlit
{
	half4 vertex : POSITION;
	half2 texcoord : TEXCOORD0;
};

struct v2f_quadblit
{
	half4 pos : SV_POSITION;
	half2 uv : TEXCOORD0;
};

v2f_quadblit vert_quadblit( appdata_QuadBlit v )
{
	v2f_quadblit o;
	o.pos = v.vertex;//because we already assgined correct screen space co-ordinates in mesh setup
	o.uv = v.texcoord;
	return o;
}



#endif