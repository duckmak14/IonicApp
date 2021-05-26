namespace Common.ImageManipulation
{
    using System;
    using System.Collections;
    using System.Drawing;
    using System.Drawing.Imaging;

    public class OctreeQuantizer : Quantizer
    {
        private int _maxColors;
        private Octree _octree;

        public OctreeQuantizer(int maxColors, int maxColorBits) : base(false)
        {
            if (maxColors > 0xff)
            {
                throw new ArgumentOutOfRangeException("maxColors", maxColors, "The number of colors should be less than 256");
            }
            if ((maxColorBits < 1) | (maxColorBits > 8))
            {
                throw new ArgumentOutOfRangeException("maxColorBits", maxColorBits, "This should be between 1 and 8");
            }
            this._octree = new Octree(maxColorBits);
            this._maxColors = maxColors;
        }

        protected override ColorPalette GetPalette(ColorPalette original)
        {
            ArrayList list = this._octree.Palletize(this._maxColors - 1);
            for (int i = 0; i < list.Count; i++)
            {
                original.Entries[i] = (Color) list[i];
            }
            original.Entries[this._maxColors] = Color.FromArgb(0, 0, 0, 0);
            return original;
        }

        protected override void InitialQuantizePixel(Quantizer.Color32 pixel)
        {
            this._octree.AddColor(pixel);
        }

        protected override byte QuantizePixel(Quantizer.Color32 pixel)
        {
            byte paletteIndex = (byte) this._maxColors;
            if (pixel.Alpha > 0)
            {
                paletteIndex = (byte) this._octree.GetPaletteIndex(pixel);
            }
            return paletteIndex;
        }

        private class Octree
        {
            private int _leafCount;
            private int _maxColorBits;
            private int _previousColor;
            private OctreeNode _previousNode;
            private OctreeNode[] _reducibleNodes;
            private OctreeNode _root;
            private static int[] mask = new int[] { 0x80, 0x40, 0x20, 0x10, 8, 4, 2, 1 };

            public Octree(int maxColorBits)
            {
                this._maxColorBits = maxColorBits;
                this._leafCount = 0;
                this._reducibleNodes = new OctreeNode[9];
                this._root = new OctreeNode(0, this._maxColorBits, this);
                this._previousColor = 0;
                this._previousNode = null;
            }

            public void AddColor(Quantizer.Color32 pixel)
            {
                if (this._previousColor == pixel.ARGB)
                {
                    if (null == this._previousNode)
                    {
                        this._previousColor = pixel.ARGB;
                        this._root.AddColor(pixel, this._maxColorBits, 0, this);
                    }
                    else
                    {
                        this._previousNode.Increment(pixel);
                    }
                }
                else
                {
                    this._previousColor = pixel.ARGB;
                    this._root.AddColor(pixel, this._maxColorBits, 0, this);
                }
            }

            public int GetPaletteIndex(Quantizer.Color32 pixel)
            {
                return this._root.GetPaletteIndex(pixel, 0);
            }

            public ArrayList Palletize(int colorCount)
            {
                while (this.Leaves > colorCount)
                {
                    this.Reduce();
                }
                ArrayList palette = new ArrayList(this.Leaves);
                int paletteIndex = 0;
                this._root.ConstructPalette(palette, ref paletteIndex);
                return palette;
            }

            public void Reduce()
            {
                int index = this._maxColorBits - 1;
                while ((index > 0) && (null == this._reducibleNodes[index]))
                {
                    index--;
                }
                OctreeNode node = this._reducibleNodes[index];
                this._reducibleNodes[index] = node.NextReducible;
                this._leafCount -= node.Reduce();
                this._previousNode = null;
            }

            protected void TrackPrevious(OctreeNode node)
            {
                this._previousNode = node;
            }

            public int Leaves
            {
                get
                {
                    return this._leafCount;
                }
                set
                {
                    this._leafCount = value;
                }
            }

            protected OctreeNode[] ReducibleNodes
            {
                get
                {
                    return this._reducibleNodes;
                }
            }

            protected class OctreeNode
            {
                private int _blue;
                private OctreeQuantizer.Octree.OctreeNode[] _children;
                private int _green;
                private bool _leaf;
                private OctreeQuantizer.Octree.OctreeNode _nextReducible;
                private int _paletteIndex;
                private int _pixelCount;
                private int _red;

                public OctreeNode(int level, int colorBits, OctreeQuantizer.Octree octree)
                {
                    this._leaf = level == colorBits;
                    this._red = this._green = this._blue = 0;
                    this._pixelCount = 0;
                    if (this._leaf)
                    {
                        octree.Leaves++;
                        this._nextReducible = null;
                        this._children = null;
                    }
                    else
                    {
                        this._nextReducible = octree.ReducibleNodes[level];
                        octree.ReducibleNodes[level] = this;
                        this._children = new OctreeQuantizer.Octree.OctreeNode[8];
                    }
                }

                public void AddColor(Quantizer.Color32 pixel, int colorBits, int level, OctreeQuantizer.Octree octree)
                {
                    if (this._leaf)
                    {
                        this.Increment(pixel);
                        octree.TrackPrevious(this);
                    }
                    else
                    {
                        int num = 7 - level;
                        int index = (((pixel.Red & OctreeQuantizer.Octree.mask[level]) >> (num - 2)) | ((pixel.Green & OctreeQuantizer.Octree.mask[level]) >> (num - 1))) | ((pixel.Blue & OctreeQuantizer.Octree.mask[level]) >> num);
                        OctreeQuantizer.Octree.OctreeNode node = this._children[index];
                        if (null == node)
                        {
                            node = new OctreeQuantizer.Octree.OctreeNode(level + 1, colorBits, octree);
                            this._children[index] = node;
                        }
                        node.AddColor(pixel, colorBits, level + 1, octree);
                    }
                }

                public void ConstructPalette(ArrayList palette, ref int paletteIndex)
                {
                    if (this._leaf)
                    {
                        this._paletteIndex = paletteIndex++;
                        palette.Add(Color.FromArgb(this._red / this._pixelCount, this._green / this._pixelCount, this._blue / this._pixelCount));
                    }
                    else
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            if (null != this._children[i])
                            {
                                this._children[i].ConstructPalette(palette, ref paletteIndex);
                            }
                        }
                    }
                }

                public int GetPaletteIndex(Quantizer.Color32 pixel, int level)
                {
                    int num = this._paletteIndex;
                    if (this._leaf)
                    {
                        return num;
                    }
                    int num2 = 7 - level;
                    int index = (((pixel.Red & OctreeQuantizer.Octree.mask[level]) >> (num2 - 2)) | ((pixel.Green & OctreeQuantizer.Octree.mask[level]) >> (num2 - 1))) | ((pixel.Blue & OctreeQuantizer.Octree.mask[level]) >> num2);
                    if (null == this._children[index])
                    {
                        throw new Exception("Didn't expect this!");
                    }
                    return this._children[index].GetPaletteIndex(pixel, level + 1);
                }

                public void Increment(Quantizer.Color32 pixel)
                {
                    this._pixelCount++;
                    this._red += pixel.Red;
                    this._green += pixel.Green;
                    this._blue += pixel.Blue;
                }

                public int Reduce()
                {
                    this._red = this._green = this._blue = 0;
                    int num = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        if (null != this._children[i])
                        {
                            this._red += this._children[i]._red;
                            this._green += this._children[i]._green;
                            this._blue += this._children[i]._blue;
                            this._pixelCount += this._children[i]._pixelCount;
                            num++;
                            this._children[i] = null;
                        }
                    }
                    this._leaf = true;
                    return (num - 1);
                }

                public OctreeQuantizer.Octree.OctreeNode[] Children
                {
                    get
                    {
                        return this._children;
                    }
                }

                public OctreeQuantizer.Octree.OctreeNode NextReducible
                {
                    get
                    {
                        return this._nextReducible;
                    }
                    set
                    {
                        this._nextReducible = value;
                    }
                }
            }
        }
    }
}

