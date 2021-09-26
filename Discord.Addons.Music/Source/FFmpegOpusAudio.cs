using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Discord.Addons.Music.Source
{
    public class FFmpegOpusAudio : FFmpegAudio
    {
        public FFmpegOpusAudio(string source, int bitrate = 128, string codec = null, string executable = "ffmpeg", List<string> args = null)
            : base(source, executable, args)
        {
            if (codec == "opus" || codec == "libopus")
            {
                codec = "copy";
            }
            else
            {
                codec = "libopus";
            }

            args.Add($"-i {source}");
            args.Add("-f opus");
            args.Add($"-c:a {codec}");
            args.Add("-ar 48000");
            args.Add("-ac 2");
            args.Add($"-b:a {bitrate}k");
            args.Add("-logleve warning");
            args.Add("pipe:1");

            SpawnProcess(args);

            // Run send audio loop
            // ...
        }

        public static async Task<FFmpegOpusAudio> FromProbe(string source, List<string> args = null)
        {
            var data = await Task.Run(() => 
            { 
                return ProbeCodecNative(source);
            });

            //string codec = (string)data.GetValueOrDefault("codec", null);
            //decimal bitrate = data.GetValueOrDefault("bitrate", null);


            //return new FFmpegOpusAudio(source, bitrate: Convert.ToInt32(bitrate), codec: codec);
            return new FFmpegOpusAudio(source);
        }

        public static Dictionary<string, dynamic> ProbeCodecNative(string source)
        {
            string args = $"ffprobe -v quiet -print_format json -show_streams -select_streams a:0 {source}";

            Process probeProcess = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = args,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            probeProcess.WaitForExit();

            string output = probeProcess.StandardOutput.ReadToEnd();
            if (output.Length > 0)
            {
                var data = JObject.Parse(output);
                JObject stream = data.Value<JArray>("streams")[0].Value<JObject>();
                string codec = stream.Value<string>("codec_name");
                decimal bitrate = decimal.Parse(stream.Value<string>("bit_rate"));
                decimal temp = Math.Round(bitrate / 1000);
                if (temp < 512)
                {
                    bitrate = 512;
                }
                return new Dictionary<string, dynamic>()
                {
                    {"bitrate", bitrate },
                    {"codec", codec }
                };
            }

            return null;
        }
    }
}
