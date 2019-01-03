using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NGS.AdvancedRenderSystem
{
    public class TexturesManager
    {
        private List<BillboardTexture> _textures = new List<BillboardTexture>();

        public RenderTexture GetTexture(BaseBillboardData data, Camera camera)
        {
            int textureSize = GetTextureSize(data, camera);

            int index = 0;
            BillboardTexture billboardTexture = GetTextureBySize(textureSize, ref index); 

            if (billboardTexture.texture == null)
            {
                RenderTexture renderTexture = new RenderTexture(textureSize, textureSize, 24);

                renderTexture.Create();

                return renderTexture;
            }

            _textures.RemoveAt(index);

            return billboardTexture.texture;
        }

        public void FreeTextureUsage(BaseBillboardData data)
        {
            RenderTexture texture = data.texture;
            int size = texture.width;

            texture.Release();

            _textures.Add(new BillboardTexture(texture, size));
        }


        private BillboardTexture GetTextureBySize(int size, ref int index)
        {
            BillboardTexture texture = new BillboardTexture();

            for (int i = 0; i < _textures.Count; i++)
            {
                if (_textures[i].textureSize == size)
                {
                    texture = _textures[i];
                    index = i;

                    break;
                }
            }

            return texture;
        }

        private int GetTextureSize(BaseBillboardData data, Camera camera)
        {
            return Mathf.Min(GetPowerOfTwo((int)((((data.billboardSize / data.distance) * Mathf.Rad2Deg) * Screen.height) / camera.fieldOfView)), 1024);
        }

        private int GetPowerOfTwo(int x)
        {
            int y = 1;

            while (y < x)
                y = y * 2;

            return y;
        }
    }

    public struct BillboardTexture
    {
        public RenderTexture texture { get; private set; }
        public int textureSize { get; private set; }

        public BillboardTexture(RenderTexture texture, int textureSize)
        {
            this.textureSize = textureSize;
            this.texture = texture;
        }
    }
}
