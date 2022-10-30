#ifndef TRIPLANAR_CUBE
#define TRIPLANAR_CUBE

void TriplanarCubes_float(float3 Pos, out float2 UV)
{
	float2 tx = Pos.zy;
	float2 ty = Pos.xz;
	float2 tz = Pos.xy;


	float3 blockPos = Pos - floor(Pos);
	float2 cc = (blockPos.x < 0.0001 || blockPos.x > 0.9999 ? tx : float2(0,0));
	cc = (blockPos.y < 0.0001 || blockPos.y > 0.9999 ? ty : cc);
	cc = (blockPos.z < 0.0001 || blockPos.z > 0.9999 ? tz : cc);
	UV = cc;
}

#endif