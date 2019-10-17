using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace SoundSignalWebApp.Scripts
{
    public static class FileUtil
    {
        public static ValueTask<object> SaveAs(this IJSRuntime js, string filename, byte[] data)
            => js.InvokeAsync<object>(
                "saveAsFile",
                filename,
                Convert.ToBase64String(data));
        
        public static ValueTask<object> PlaySound(this IJSRuntime js, string fileData)
            => js.InvokeAsync<object>(
                "playSound",
                fileData);
    }
}