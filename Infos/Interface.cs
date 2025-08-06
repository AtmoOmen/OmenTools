using System.Numerics;
using Dalamud.Interface.Style;

namespace OmenTools.Infos;

public static partial class InfosOm
{
    public static StyleModel DalamudStandardStyle = StyleModel.Deserialize(
        "DS1H4sIAAAAAAAACq1XS3PbNhD+Kx2eNR6QAAFQt9hq40PS8cTupO0NlmCJFSWqFKU8PPnvxWsBEJQUTVJduIC+b7G7WOwCr5nIpvkNmmTP2fQ1+zObcj34y3y/TbJ5NiV6YpFNkf5Kh0IOhW5KhXpROibZ0k2vnMbajcWzE/5xZOLI2CyxdhONQ20SQ4hBbROunW2T2cLM7kZG6tl/1d/Grk7ZR7Wwz6aV/vZqQtkyyQ7Gpkl2dBo/ue9np+uLV1xG3n89uZwQbhoPvJ23ys/X7El+7t3/ufvffP9234/mq/AaOKv34rmRi/HqhmC+nvCx3i7aT7fLYBQFq6iDa0Hh0U2FFeFuVTeLGA9wQDuwVv7Q7g67GMsBzAHNQTdR+Nu2W8jOwwl2cC1Elts4WvDjSij7r7Lmt05sZGRNUTmwFgxaC1Y/Cfj79ii7OJoEwknAKuJoEevNvK+PIf0pkCiQKJAo0/tW901sGyoKWhCCIQJ2XIQ9CWOjhJeIMs5IUJUYgHmVE1RAKuSoyhkqQV0YWm0FRyXBQdld2zRit4+i8IP63svt4VZ0kac5REYLlkyi/Hycd2rt5wHl0iZ7/NtOFxKwNncULRiOFsaLaFK63wS4BLjkLDcJeglULdhUial3Kzlfvxfd2hM4HD4tGIIWwlpNrVJ+4NnZbEwZaUIySEgGCcki3u2h71soouiGgSNaMGgt+Jy36DRwmBKVA3DIEOME55hBEYrGrgCUqOIoqEsMJpzktOBgdo4QY2UFxodhquxeirimeHu0YOMFhx7nHj468ycrqNodz0jDC9GF4MZ7IneiE317baHz+J8LMLOnkMQa0yLxozv2Qe7rr/JtV4deykCNFmzKQKCLckBJ3WLQJbRgmTyulYH5f5n/FB2oAvZCC7YrYNhtarGj7OCY5chvIkaYEcTgPIah3QZrpNWU5jirKGYV+J+XymLVBSDH/XCs6I/tSzs/xAUaUa8G5ZX6QTBzwpH6gVMVK0iiI7Eqx6Xj27hQow4ymxRxvZm183W9XT508ljL0JYLmp46rc66EVi/bnb9l7hBw4qwEdFCD03bv6u3ch+OHNQoLdhDl58iDHcPrlPR2cNlQruv9327VH3dr+Vze9CBTjDOLeb7eHx307c2W03i/ghraWEUBsNxN6G+a7fLSy2vPE18Vy9X/aX8H/E+DG+MF9pxgL9pvneD1TnrrrCPspHzXsb3zAsphHU9mXViOeva3ZPolvLcUsE4fW5+F8d75Xsz9P/cOtZ/xbFXZpWvKfm8YyxhzupN5BqcLDih4FehL0ztQjSWdx1JBUO/f9QlNJtmM9GIzWHxy2MvtgvRLdS5z9TrzD4qxCgrh0GyDofKyJKkj+8L6uF3Beq6J4m86qXzEiLhG4aRbDRcy7DY5chThmB3Y52rUU7TEyvXI5Qvrdj8Iiy8YfWKJee6Vp6N9Pps0sYVtgka01BXNA5jqFawKDmhLtzx8tzqS8+KxcHTOThNsA3hMDjhAkD1/7aSIG/hIDjqiZ1sS4mgWMU6w0WJ+wuy6qhQ5ct4q0M/4L5jwfYY7Q6poKoyq9lv/wGOQIbC1BAAAA==")!;

    public static Vector4 ColorGradient()
    {
        const float period = 1f;
        
        var t     = (float)ImGui.GetTime() % period                 / period;
        var red   = (MathF.Sin(2 * MathF.PI * t)               + 1) / 2;
        var green = (MathF.Sin(2 * MathF.PI * (t + (1f / 3f))) + 1) / 2;
        var blue  = (MathF.Sin(2 * MathF.PI * (t + (2f / 3f))) + 1) / 2;

        return new Vector4(red, green, blue, 1f);
    }
}
