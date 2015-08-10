using System.Runtime.InteropServices;
public class Approx
{
	public static float Sqrt(float z)
	{
		if (z == 0) return 0;
		FloatIntUnion u;
		u.tmp = 0;
		u.f = z;
		u.tmp -= 1 << 23; /* Subtract 2^m. */
		u.tmp >>= 1; /* Divide by 2. */
		u.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */
		return u.f;
	}
	
	//John Carmack's method, slower than the first
	public static float Sqrt2(float z)
	{
		if (z == 0) return 0;
		FloatIntUnion u;
		u.tmp = 0;
		float xhalf = 0.5f * z;
		u.f = z;
		u.tmp = 0x5f375a86 - (u.tmp >> 1);
		u.f = u.f * (1.5f - xhalf * u.f * u.f);
		return u.f * z;
	}
	
	[StructLayout(LayoutKind.Explicit)]
	private struct FloatIntUnion
	{
		[FieldOffset(0)]
		public float f;
		
		[FieldOffset(0)]
		public int tmp;
	}
}