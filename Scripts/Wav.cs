using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Blazor.Extensions.Storage;

namespace SoundSignalWebApp.Scripts
{
    public class Wav
    {
        
        private struct WavModel
        {
            private readonly ushort channel;
            private readonly ushort bitPerSample;
            private readonly uint sampleRate;

            public WavModel(ushort channel, ushort bitPerSample, uint sampleRate)
            {
                this.channel = channel;
                this.bitPerSample = bitPerSample;
                this.sampleRate = sampleRate;
            }

            private IEnumerable<byte> Header(uint fileSize, uint formatChunkSize, ushort formatId, uint bytePerSec, ushort blockSize, uint dataChunkSize) 
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
            
            public byte[] Output()
            {
                var fileData = new List<byte>();
                var formatChunkSize = (uint)16;
                var formatId = (ushort)1;

                var numberOfBytePerSample = ((ushort)(Math.Ceiling((double)bitPerSample / 8)));
                var blockSize = (ushort)(numberOfBytePerSample * channel);
                var bytePerSec = sampleRate * channel * numberOfBytePerSample;
                var dataLength = (sampleRate * 100) / 1000;
                var dataChunkSize = blockSize * dataLength;
                var fileSize = dataChunkSize + 44;
                
                fileData.AddRange(Header(fileSize, formatChunkSize, formatId, bytePerSec, blockSize, dataChunkSize));

                for (var cnt = 0; cnt < dataLength; cnt++)
                {
                    var radian = (double)cnt / sampleRate;
                    radian *= 2 * Math.PI;
         
                    var wave = Math.Sin(radian * 10);
         
                    var data = (short)(wave * 30000);
         
                    fileData.AddRange(BitConverter.GetBytes(data));
                    fileData.AddRange(BitConverter.GetBytes(data));
                }
                return fileData.ToArray();
            }
        }

        public async Task Create(IJSRuntime js)
        {
            var localStorage = new LocalStorage(js);
            
            var channel = (ushort)2;
            var sampleRate = (uint)44100;
            var bitPerSample = (ushort)16;
            
            var model = new WavModel(channel, bitPerSample, sampleRate);
            
            await localStorage.SetItem("test.wav", model.Output());
        }
    }
}