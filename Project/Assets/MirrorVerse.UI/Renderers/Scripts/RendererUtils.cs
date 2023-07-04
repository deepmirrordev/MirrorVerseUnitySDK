using System;
namespace MirrorVerse.UI.Renderers
{
    public class RendererUtils
    {
        public static long NowMs()
        {
            // Milliseconds from epoch.
            return ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds();
        }

        public static Int64 NowNs()
        {
            // Nanoseconds from epoch.
            return (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) * 100;
        }
    }
}
