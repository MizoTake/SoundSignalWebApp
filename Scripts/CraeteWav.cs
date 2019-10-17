using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Blazor.Extensions.Storage;

namespace SoundSignalWebApp.Scripts
{
    public class CraeteWav
    {
        
        private struct WavModel
        {
            private readonly uint fileSize;
            private readonly uint formatChunkSize;
            private readonly ushort formatId;
            private readonly ushort channel;
            private readonly uint sampleRate;
            private readonly uint bytePerSec;
            private readonly ushort blockSize;
            private readonly ushort bitPerSample;
            private readonly uint dataChunkSize;

            public WavModel(uint fileSize, uint formatChunkSize, ushort formatId, ushort channel, uint sampleRate, uint bytePerSec, ushort blockSize, ushort bitPerSample, uint dataChunkSize)
            {
                this.fileSize = fileSize;
                this.formatChunkSize = formatChunkSize;
                this.formatId = formatId;
                this.channel = channel;
                this.sampleRate = sampleRate;
                this.bytePerSec = bytePerSec;
                this.blockSize = blockSize;
                this.bitPerSample = bitPerSample;
                this.dataChunkSize = dataChunkSize;
            }
            
            public byte[] Output() 
            {
                var data = new byte[44];
                Array.Copy(Encoding.ASCII.GetBytes("RIFF"), 0, data, 0, 4);
                Array.Copy(BitConverter.GetBytes(fileSize - 8), 0, data, 4, 4);
                Array.Copy(Encoding.ASCII.GetBytes("WAVE"), 0, data, 8, 4);
                Array.Copy(Encoding.ASCII.GetBytes("fmt "), 0, data, 12, 4);
                Array.Copy(BitConverter.GetBytes(formatChunkSize), 0, data, 16, 4);
                Array.Copy(BitConverter.GetBytes(formatId), 0, data, 20, 2);
                Array.Copy(BitConverter.GetBytes(channel), 0, data, 22, 2);
                Array.Copy(BitConverter.GetBytes(sampleRate), 0, data, 24, 4);
                Array.Copy(BitConverter.GetBytes(bytePerSec), 0, data, 28, 4);
                Array.Copy(BitConverter.GetBytes(blockSize), 0, data, 32, 2);
                Array.Copy(BitConverter.GetBytes(bitPerSample), 0, data, 34, 2);
                Array.Copy(Encoding.ASCII.GetBytes("data"), 0, data, 36, 4);
                Array.Copy(BitConverter.GetBytes(dataChunkSize), 0, data, 40, 4);
                return data;
            }
        }

        public async Task Create(IJSRuntime js)
        {
            var fileName = @"https://localhost:5001/audio/text.wav";
            var localStorage = new LocalStorage(js);
            List<byte> fileData = new List<byte>();
            
//            using (var filStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
//            using (var binWriter = new BinaryWriter(filStream))
            {
                var formatChunkSize = (uint)16;
                var formatId = (ushort)1;
                var channel = (ushort)2;
                var sampleRate = (uint)44100;
                var bitPerSample = (ushort)16;
         
                int numberOfBytePerSample = ((ushort)(Math.Ceiling((double)bitPerSample / 8)));
                var blockSize = (ushort)(numberOfBytePerSample * channel);
                var bytePerSec = sampleRate * channel * (uint)numberOfBytePerSample;
                var dataLength = (uint)(sampleRate * DateTime.Now.Millisecond) / 1000;
                var dataChunkSize = blockSize * dataLength;
                var fileSize = dataChunkSize + 44;

                var model = new WavModel(fileSize, formatChunkSize, formatId, channel, sampleRate, bytePerSec, blockSize, bitPerSample, dataChunkSize);
                
//                binWriter.Write(model.Output());
                fileData.AddRange(model.Output());

                for (var cnt = 0; cnt < dataLength; cnt++)
                {
                    var radian = (double)cnt / sampleRate;
                    radian *= 2 * Math.PI;
         
                    // 10Hzの正弦波を作る。
                    var wave = Math.Sin(radian * 10);
         
                    var data = (short)(wave * 30000);
         
//                    binWriter.Write(BitConverter.GetBytes(data));
//                    binWriter.Write(BitConverter.GetBytes(data));
                    fileData.AddRange(BitConverter.GetBytes(data));
                    fileData.AddRange(BitConverter.GetBytes(data));
                    fileData.AddRange(BitConverter.GetBytes(data));
                    fileData.AddRange(BitConverter.GetBytes(data));
                    fileData.AddRange(BitConverter.GetBytes(data));
                    fileData.AddRange(BitConverter.GetBytes(data));
                }
                
                await localStorage.SetItem("test.wav", fileData.ToArray());
            }
        }
    }
}