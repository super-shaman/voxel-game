using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class RLEVoxelChunk : VoxelChunk
{

    public RLEVoxelChunk(byte size, int memIndex, bool initMem) : base(size, memIndex, false)
    {
        chunkType = 1;
    }

    public List<byte> data = new List<byte>();
    public void Load(int index1, int index2, int index3, TerrainChunk terrain, VoxelChunk vc)
    {
        this.index1 = index1;
        this.index2 = index2;
        this.index3 = index3;
        this.terrain = terrain;
        data.Clear();
        chunks[13] = this;
        for (int i = 0; i < 3; i++)
        {
            for (int ii = 0; ii < 3; ii++)
            {
                for (int iii = 0; iii < 3; iii++)
                {
                    if (vc.chunks[i * 9 + ii * 3 + iii] != null)
                    {
                        chunks[i * 9 + ii * 3 + iii] = vc.chunks[i * 9 + ii * 3 + iii];
                        vc.chunks[i * 9 + ii * 3 + iii] = null;
                        chunks[i * 9 + ii * 3 + iii].chunks[(2-i)*9+(2-ii)*3+2-iii] = this;
                    }else
                    {
                        chunks[i * 9 + ii * 3 + iii] = null;
                    }
                }
            }
        }
        chunks[13] = this;
    }

    public void Decompress(VoxelChunk vc)
    {
        int aa = 0;
        for (int i = 0; i < types.Length; i+=2)
        {
            int t = types[i];
            int a = types[i+1];
            for (int ii = 0; ii < a; ii++)
            {
                vc.SetTypeFast(aa, t);
                aa++;
            }
        }
        types = null;
        vc.chunks[13] = vc;
        for (int i = 0; i < 3; i++)
        {
            for (int ii = 0; ii < 3; ii++)
            {
                for (int iii = 0; iii < 3; iii++)
                {
                    if (chunks[i * 9 + ii * 3 + iii] != null)
                    {
                        vc.chunks[i * 9 + ii * 3 + iii] = chunks[i * 9 + ii * 3 + iii];
                        chunks[i * 9 + ii * 3 + iii] = null;
                        vc.chunks[i * 9 + ii * 3 + iii].chunks[(2 - i) * 9 + (2 - ii) * 3 + 2 - iii] = vc;
                    }
                    else
                    {
                        vc.chunks[i * 9 + ii * 3 + iii] = null;
                    }
                }
            }
        }
        vc.chunks[13] = vc;

    }

}