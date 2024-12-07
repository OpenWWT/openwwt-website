#nullable disable

using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace WWT.Providers
{
    public class Mandelbrot : IMandelbrot
    {
        private readonly ActivitySource _activitySource;
        private readonly Color[] _colorMap;

        public Mandelbrot([FromKeyedServices(Constants.ActivitySourceName)]ActivitySource activitySource)
        {
            _activitySource = activitySource;
            _colorMap = CreateColorMap();
        }

        public Stream CreateMandelbrot(int level, int tileX, int tileY)
        {
            using (var image = CreateMandelbrotBitmap(level, tileX, tileY))
            {
                return image.ToPngStream();
            }
        }

        private Image CreateMandelbrotBitmap(int level, int tileX, int tileY)
        {
            using var activity = _activitySource.StartImageProcessing();

            double tileWidth = (4 / (Math.Pow(2, level)));
            double Sy = ((double)tileY * tileWidth) - 2;
            double Fy = Sy + tileWidth;
            double Sx = ((double)tileX * tileWidth) - 4;
            double Fx = Sx + tileWidth;

            int MAXITER = 100 + level * 100;

            var b = new Image<Rgb24>(256, 256);
            double x, y, x1, y1, xx, xmin, xmax, ymin, ymax = 0.0;
            int looper, s, z = 0;
            double intigralX, intigralY = 0.0;
            xmin = Sx;
            ymin = Sy;
            xmax = Fx;
            ymax = Fy;
            intigralX = (xmax - xmin) / 256;
            intigralY = (ymax - ymin) / 256;
            x = xmin;
            for (s = 0; s < 256; s++)
            {
                y = ymin;
                for (z = 0; z < 256; z++)
                {
                    x1 = 0;
                    y1 = 0;
                    looper = 0;
                    while (looper < MAXITER && ((x1 * x1) + (y1 * y1)) < 4)
                    {
                        looper++;
                        xx = (x1 * x1) - (y1 * y1) + x;
                        y1 = 2 * x1 * y1 + y;
                        x1 = xx;
                    }
                    double perc = looper / (256.0);
                    int val = looper % 254;
                    b[s, z] = looper == MAXITER ? Color.Black : _colorMap[val];
                    y += intigralY;
                }
                x += intigralX;
            }

            return b;
        }

        private static Color[] CreateColorMap()
        {
            var c = new Color[256];

            using (var stream = typeof(Mandelbrot).Assembly.GetManifestResourceStream(typeof(Mandelbrot), "Services.colors.map"))
            using (var sr = new StreamReader(stream))
            {
                var lines = new List<string>();
                var line = sr.ReadLine();

                while (line != null)
                {
                    lines.Add(line);
                    line = sr.ReadLine();
                }

                int i = 0;
                for (i = 0; i < Math.Min(256, lines.Count); i++)
                {
                    var curC = lines[i];
                    var temp = new Color(new Rgb24(byte.Parse(curC.Split(' ')[0]), byte.Parse(curC.Split(' ')[1]), byte.Parse(curC.Split(' ')[2])));
                    c[i] = temp;
                }

                for (int j = i; j < 256; j++)
                {
                    c[j] = Color.White;
                }
            }

            return c;
        }
    }
}
