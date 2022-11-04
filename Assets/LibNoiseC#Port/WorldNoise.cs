
public class WorldNoise
{
    // noisegen.cpp
    //
    // Copyright (C) 2003, 2004 Jason Bevins
    //
    // This library is free software; you can redistribute it and/or modify it
    // under the terms of the GNU Lesser General Public License as published by
    // the Free Software Foundation; either version 2.1 of the License, or (at
    // your option) any later version.
    //
    // This library is distributed in the hope that it will be useful, but WITHOUT
    // ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
    // FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public
    // License (COPYING.txt) for more details.
    //
    // You should have received a copy of the GNU Lesser General Public License
    // along with this library; if not, write to the Free Software Foundation,
    // Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
    //
    // The developer's email is jlbezigvins@gmzigail.com (for great email, take
    // off every 'zig'.)
    //

    // ported to C#, removed unused code
    // removed all original comments

    static int X_NOISE_GEN = 1619;
    static int Y_NOISE_GEN = 31337;
    static int Z_NOISE_GEN = 6971;
    static int SEED_NOISE_GEN = 1013;

    // ported to C#, removed unused code
    // removed all original comments

    static int IntValueNoise3D(int x, int y, int z, int seed)
    {
        int n = (
            X_NOISE_GEN * x
            + Y_NOISE_GEN * y
            + Z_NOISE_GEN * z
            + SEED_NOISE_GEN * seed)
            & 0x7fffffff;
        n = (n >> 13) ^ n;
        return (n * (n * n * 60493 + 19990303) + 1376312589) & 0x7fffffff;
    }

    // ported to C#
    // removed all original comments
    // removed quality settings

    public static double ValueCoherentNoise3D(double x, double y, double z, int seed)
    {
        int x0 = (x > 0.0 ? (int)x : (int)x - 1);
        int x1 = x0 + 1;
        int y0 = (y > 0.0 ? (int)y : (int)y - 1);
        int y1 = y0 + 1;
        int z0 = (z > 0.0 ? (int)z : (int)z - 1);
        int z1 = z0 + 1;
        double xs = 0;
        double ys = 0;
        double zs = 0;

        xs = SCurve5(x - (double)x0);
        ys = SCurve5(y - (double)y0);
        zs = SCurve5(z - (double)z0);
        double n0 = 0;
        double n1 = 0;
        double ix0 = 0;
        double ix1 = 0;
        double iy0 = 0;
        double iy1 = 0;
        n0 = ValueNoise3D(x0, y0, z0, seed);
        n1 = ValueNoise3D(x1, y0, z0, seed);
        ix0 = LinearInterp(n0, n1, xs);
        n0 = ValueNoise3D(x0, y1, z0, seed);
        n1 = ValueNoise3D(x1, y1, z0, seed);
        ix1 = LinearInterp(n0, n1, xs);
        iy0 = LinearInterp(ix0, ix1, ys);
        n0 = ValueNoise3D(x0, y0, z1, seed);
        n1 = ValueNoise3D(x1, y0, z1, seed);
        ix0 = LinearInterp(n0, n1, xs);
        n0 = ValueNoise3D(x0, y1, z1, seed);
        n1 = ValueNoise3D(x1, y1, z1, seed);
        ix1 = LinearInterp(n0, n1, xs);
        iy1 = LinearInterp(ix0, ix1, ys);
        return LinearInterp(iy0, iy1, zs);
    }

    // ported to C#
    // removed all original comments

    static double ValueNoise3D(int x, int y, int z, int seed)
    {
        return 1.0 - ((double)IntValueNoise3D(x, y, z, seed) / 1073741824.0);
    }

    // ported to C#
    // removed all original comments

    static double LinearInterp(double n0, double n1, double a)
    {
        return ((1.0 - a) * n0) + (a * n1);
    }

    // ported to C#
    // removed all original comments

    public static double SCurve5(double a)
    {
        double a3 = a * a * a;
        double a4 = a3 * a;
        double a5 = a4 * a;
        return (6.0 * a5) - (15.0 * a4) + (10.0 * a3);
    }

    // made this function based on the function ValueCoherentNoise3D but for interpolating known values

    public static double interpQuad(double x, double y, double v1, double v2, double v3, double v4)
    {
        double xs = 0;
        double ys = 0;

        xs = SCurve5(x);
        ys = SCurve5(y);
        double n0 = 0;
        double n1 = 0;
        double ix0 = 0;
        double ix1 = 0;
        n0 = v1;
        n1 = v2;
        ix0 = LinearInterp(n0, n1, xs);
        n0 = v3;
        n1 = v4;
        ix1 = LinearInterp(n0, n1, xs);
        return LinearInterp(ix0, ix1, ys);
    }

}