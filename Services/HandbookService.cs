using RomDiscord.Models.Db;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;

namespace RomDiscord.Services
{
	public class HandbookService
	{
        public List<HandbookState> ScanPicture(Stream stream, ulong discordUserId, ref string error, bool save = true)
        {
            Image<Rgba32> img = Image.Load<Rgba32>(stream);
            Directory.CreateDirectory("scanfiles");
            if (save)
            {
                int id = Directory.GetFiles("scanfiles").Length;
                img.SaveAsPng("scanfiles/" + discordUserId + "_" + Directory.GetFiles("scanfiles").Length + ".png");
                Console.WriteLine("Scanning picture " + id + ".png");
            }
            int centerX = img.Width / 2;
            int brightest = 0;
            int maxLineWidth = 0;
            int maxlineWidthMin = 0;
            int maxlineWidthMax = 0;

            for (int y = img.Height / 2; y < img.Height; y++)
            {
                bool lineok = true;

                int lineWidthMin = 0;
                int lineWidthMax = 0;
                for (int x = centerX; x < img.Width && lineWidthMax == 0; x++)
                    if (img[x, y] != img[centerX, y])
                        lineWidthMax = x - centerX;
                for (int x = centerX; x > 0 && lineWidthMin == 0; x--)
                    if (img[x, y] != img[centerX, y])
                        lineWidthMin = centerX - x;

                maxlineWidthMin = Math.Max(maxlineWidthMin, lineWidthMin);
                maxlineWidthMax = Math.Max(maxlineWidthMax, lineWidthMax);
            }

            for (int y = img.Height / 2; y < img.Height; y++)
            {
                bool lineok = true;

                int lineWidthMin = 0;
                int lineWidthMax = 0;
                for (int x = centerX; x < img.Width && lineWidthMax == 0; x++)
                    if (img[x, y] != img[centerX, y])
                        lineWidthMax = x - centerX;
                for (int x = centerX; x > 0 && lineWidthMin == 0; x--)
                    if (img[x, y] != img[centerX, y])
                        lineWidthMin = centerX - x;


                for (int x = centerX - (maxlineWidthMin - 2); x < centerX + (maxlineWidthMax - 2); x++)
                    if (img[x, y] != img[centerX, y] ||
                        img[x, y - 1] != img[centerX, y - 1] ||
                        img[x, y - 2] != img[centerX, y - 2] ||
                        img[x, y - 3] != img[centerX, y - 3])
                        lineok = false;

                if (lineok && (lineWidthMin + lineWidthMax) >= maxLineWidth && Brightness(img[centerX, y]) >= Brightness(img[centerX, brightest]))
                {
                    brightest = y;
                    maxLineWidth = lineWidthMin + lineWidthMax;
                }
            }

            int minX = centerX;
            int maxX = centerX;

            for (int x = centerX; x < img.Width; x++)
            {
                if (Math.Abs(Brightness(img[x, brightest]) - Brightness(img[centerX, brightest])) < 10)
                    maxX = x;
                else
                    break;
            }
            for (int x = centerX; x > 0; x--)
            {
                if (Math.Abs(Brightness(img[x, brightest]) - Brightness(img[centerX, brightest])) < 10)
                    minX = x;
                else
                    break;
            }

            minX = Math.Max(0, minX - 1);
            maxX = Math.Min(img.Width - 1, maxX + 1);

            using var cropped = img.Clone(ctx => ctx.Crop(new Rectangle(minX, 0, maxX - minX, img.Height)).Grayscale());
            /*for(int x = 0; x < cropped.Width; x++)
			{
                for (int y = 0; y < cropped.Height; y++)
				{
                    var pixel = cropped[x, y];
                    if(pixel.R < 128)
					{
                        pixel.R /= 5;
                        pixel.G /= 5;
                        pixel.B /= 5;
                    }
                    cropped[x, y] = pixel;
                }
			}*/

            using var cropped2 = cropped.Clone(ctx => ctx.Crop(new Rectangle(0, 0, cropped.Width / 2, cropped.Height)));
            using var cropped3 = cropped.Clone(ctx => ctx.Crop(new Rectangle(cropped.Width / 2, 0, cropped.Width / 2, cropped.Height)));
            cropped2.SaveAsPng("cropped1.png");
            cropped3.SaveAsPng("cropped2.png");
            string txt = GetTextBlockFromImage("cropped1.png");
            txt += "\n" + GetTextBlockFromImage("cropped2.png");
            txt = txt.Replace("\r", "");
            txt = txt.Replace("~", " ");
            txt = txt.Replace("#", " ");
            txt = txt.Replace(",", ".");
            txt = txt.Replace("  ", " ");
            txt = txt.Replace("  ", " ");
            txt = txt.Replace("  ", " ");
            txt = txt.Replace("bamage", "damage");
            txt = txt.Replace("Aaainst", "Against");
            txt = txt.Replace("Ghost Element amage", "Ghost Element Damage");
            txt = txt.Replace("\n\n", "\n");
            txt = txt.Replace("\n\n", "\n");
            txt = txt.Replace("\n\n", "\n");

            txt = txt.Replace("\t", " ");
            var lines = txt.Split("\n");

            List<HandbookState> stats = new List<HandbookState>();

            foreach (var line in lines)
            {
                try
                {
                    var trimmed = line.Trim().ToLower();
                    if (trimmed.Length == 0 || trimmed.Contains("adventure handbook attribute"))
                        continue;

                    if (trimmed.Length == 0 || trimmed.Contains("class b attribute"))
                        return null;

                    var segments = trimmed.Split(" ");
                    for (int i = 0; i < segments.Length; i++)
                    {
                        int statLength = 0;
                        for (int len = 1; len <= segments.Length - i; len++)
                        {
                            string stat = string.Join("", segments.Skip(i).Take(len).ToList());
                            if (StatUtil.IsStat(stat) && stat != "0")
                            {
                                statLength = len;
                                var intValue = 0;
                                string value = string.Join("", segments.Skip(i + len).ToList());
                                if (value.Contains("."))
                                    value = value.Substring(0, value.IndexOf("."));
                                value = value.Trim("abcdefghijklmnopqrstuvwzyz-+% ().┬ó".ToCharArray());
                                try
                                {
                                    if (value != "")
                                    {
                                        intValue = Math.Abs(int.Parse(value.Replace("]", "1").Replace("%", "").Replace(".0", "")));
                                        stats.Add(new HandbookState { Date = DateOnly.FromDateTime(DateTime.Now), DiscordUserId = discordUserId, Stat = StatUtil.AsEnum(stat), Value = intValue });
                                    }
                                }
                                catch (Exception e)
                                {
                                    error += "Unknown Value: " + line + "\n";
                                    Console.WriteLine("Unknown value: " + line);
                                    //Console.WriteLine(e);
                                }
                                i++; //skip the actual stat
                                break;
                            }
                            var s = StatUtil.StartsWithStat(stat);
                            if (s != "")
                            {
                                var value = stat.Substring(s.Length);
                                if (value.Contains("+"))
                                    value = value.Substring(value.IndexOf("+"));
                                if (value.Contains("."))
                                    value = value.Substring(0, value.IndexOf("."));
                                value = value.Trim("abcdefghijklmnopqrstuvwzyz-+% ().┬ó".ToCharArray());
                                if (value == "")
                                {
                                    value = segments[i + len];
                                    i++;
                                }
                                var intValue = 0;
                                try
                                {
                                    intValue = Math.Abs(int.Parse(value.Trim('.').Replace("-", "").Replace("%", "").Replace(".0", "")));
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                                stats.Add(new HandbookState { Date = DateOnly.FromDateTime(DateTime.Now), DiscordUserId = discordUserId, Stat = StatUtil.AsEnum(s), Value = intValue });
                                statLength = len;
                                break;
                            }

                            if (segments[i + len - 1].StartsWith("+"))
                            {
                                if (line != lines.Last())
                                {
                                    error += "Unknown stat: " + line + "\n";
                                    Console.WriteLine("Unknown stat: " + line);
                                }
                                break;
                            }
                        }
                        i += Math.Max(0, statLength - 1);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return stats;
        }


        public string GetTextLineFromImage(string fileName, int mode = 0)
        {
            string beststr = "";
            string psm = "7";
            if ((mode & 2) == 0)
                beststr = "_default";
            else if ((mode & 2) == 1)
                beststr = "_best";
            else
                beststr = "_fast";

            if (((mode >> 2) & 3) == 1)
                psm = "8";
            else if (((mode >> 2) & 3) == 2)
                psm = "10";

            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = "tessarect\\tesseract.exe",
                Arguments = $"{fileName} stdout --tessdata-dir tessarect\\tessdata{beststr} --psm {psm}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            });

            string data = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            data = data.Replace("\r\n", "\n");
            data = data.Trim(new char[] { '\f', '\n', '\r', ' ', '\t' });

            if (error != "")
            {
                if (error.Contains("count=0"))
                    return "";
                Console.WriteLine(error);
            }

            return data;
        }

        public string GetTextBlockFromImage(string fileName)
        {
            var process = Process.Start(new ProcessStartInfo()
            {
                FileName = "tessarect\\tesseract.exe",
                Arguments = $"{fileName} stdout --tessdata-dir tessarect\\tessdata_best --psm 4",
                RedirectStandardOutput = true
            });

            string data = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            data = data.Replace("\r\n", "\n");
            data = data.Trim(new char[] { '\f', '\n', '\r', ' ', '\t' });
            return data;
        }

        private int Brightness(Rgba32 pixel)
        {
            return (pixel.R + pixel.G + pixel.B); //TODO: maybe check for pure colors
        }
    }
}
