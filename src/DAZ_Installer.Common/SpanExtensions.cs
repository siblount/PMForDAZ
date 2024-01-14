using System;

namespace DAZ_Installer
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
        public static bool Contains(this Span<char> span, string msg) => span.IndexOf(msg) != -1;
        /// <inheritdoc cref="Contains(Span{char}, string)"/>
        public static bool Contains(this ReadOnlySpan<char> span, string msg) => span.IndexOf(msg) != -1;
    }
}
