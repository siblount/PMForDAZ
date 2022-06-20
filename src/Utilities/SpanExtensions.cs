using System;

namespace DAZ_Installer.Utilities
{
    internal static class SpanExtensions
    {
        /// <summary>
        /// Acts similar <see cref="String.Contains(string)"/>. Returns whether or not
        /// the <paramref name="span"/> contains <paramref name="msg"/>.
        /// </summary>
        /// <param name="span">The span to see if it contains msg.</param>
        /// <param name="msg">The msg to find inside span.</param>
        /// <returns><see langword="true"/> if <paramref name="span"/> contains <paramref name="msg"/>.
        /// Otherwise, <see langword="false"/>.</returns>
        public static bool Contains(this Span<char> span, string msg)
        {
            if (msg.Length > span.Length) return false;
            for (int i = 0; i < span.Length; i++)
            {
                var contains = false;
                for (int j = 0; j < msg.Length; j++)
                {
                    if (i + j >= span.Length || span[i + j] != msg[j]) break;
                    contains = true;
                }
                if (contains) return true;
            }
            return false;
        }
        /// <inheritdoc cref="Contains(Span{char}, string)"/>
        public static bool Contains(this ReadOnlySpan<char> span, string msg)
        {
            if (msg.Length > span.Length) return false;
            for (int i = 0; i < span.Length; i++)
            {
                var contains = false;
                for (int j = 0; j < msg.Length; j++)
                {
                    if (i + j >= span.Length || span[i + j] != msg[j]) break;
                    contains = j == msg.Length - 1;
                }
                if (contains) return true;
            }
            return false;
        }
    }
}
