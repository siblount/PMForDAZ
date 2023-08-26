using DAZ_Installer.IO;
using System;
using System.IO;
using System.Diagnostics.CodeAnalysis;

namespace DAZ_Installer
{
    /// <summary>
    /// A class dedicated to try and resolve exceptions in try/catch blocks. For example, dealing with files that are hidden, this
    /// class contains methods that will attempt to unhide the file and then try again.
    /// </summary>
    public static class TryHelper
    {
        /// <summary>
        /// <inheritdoc cref="TryFixFilePermissions(IDPFileInfo, out Exception?)"/>
        /// </summary>
        /// <param name="info">The file that we may not have access to.</param>
        /// <returns>Whether the application now has access to the file.</returns>
        public static bool TryFixFilePermissions(IDPFileInfo info) => TryFixFilePermissions(info, out _);
        /// <summary>
        /// Tries to fix the file permissions of <paramref name="info"/>. If it fails, it will return false.
        /// </summary>
        /// <param name="info">The file that we may not have access to.</param>
        /// <param name="ex">The exception that was thrown, if any.</param>
        /// <returns>Whether the application now has access to the file.</returns>
        public static bool TryFixFilePermissions(IDPFileInfo info, [NotNullWhen(false)] out Exception? ex)
        {
            // If exists returns false, it means that either the file does not exist or we do not have access to it.
            // We can't do anything if we don't have access to the file. But we can if we can not open, move, rename it 
            // due to file permissions.
            ex = null;
            if (!info.Exists) return false;
            try
            {
                info.Attributes = FileAttributes.Normal;
                info.OpenRead().Close(); // test that we have access.
                return true;
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }
        }

        /// <summary>
        /// Tries to fix the directory permissions of <paramref name="info"/>. If it fails, it will return false.
        /// </summary>
        /// <param name="info">The directory that we may not have access to.</param>
        /// <returns>Whether </returns>
        public static bool TryFixDirectoryPermissions(DirectoryInfo info) => TryFixDirectoryPermissions(info, out _);
        /// <summary>
        /// Tries to fix the directory permissions of <paramref name="info"/>. If it fails, it will return false.
        /// </summary>
        /// <param name="info">The directory that we may not have access to.</param>
        /// param name="ex">The exception that was thrown, if any.</param>
        /// <returns>Whether </returns>
        public static bool TryFixDirectoryPermissions(DirectoryInfo info, [NotNullWhen(false)] out Exception? ex)
        {
            // If exists returns false, it means that either the file does not exist or we do not have access to it.
            // We can't do anything if we don't have access to the file. But we can if we can not open, move, rename it 
            // due to file permissions.
            ex = null;
            if (!info.Exists) return false;
            try
            {
                info.Attributes = FileAttributes.Normal;
                info.EnumerateDirectories(); // test that it works.
                return true;
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }
        }
        #region Generic Try Methods
        /// <summary>
        /// <inheritdoc cref="Try(Action, out Exception?)"/>
        /// </summary>
        /// <param name="action">The function to execute in the try/catch block.</param>
        /// <returns>Whether the action ran without any exception thrown.</returns>
        public static bool Try(Action action) => Try(action, out _);
        /// <summary>
        /// Runs <paramref name="action"/> in a try/catch block and returns whether it succeeded or not.
        /// Succeeded means whether it threw an exception or not.
        /// </summary>
        /// <param name="action">The function to execute in the try/catch block.</param>
        /// <param name="ex">The exception that was thrown; this will be null if <see langword="true"/> is returned.</param>
        /// <returns>Whether the action ran without any exception thrown.</returns>
        public static bool Try(Action action, [NotNullWhen(false)] out Exception? ex)
        {
            ex = null;
            try { action(); }
            catch (Exception e)
            {
                ex = e;
                return false;
            }
            return true;
        }
        /// <summary>
        /// <inheritdoc cref="Try{T}(Func{T}, out T?, out Exception?)"/>
        /// </summary>
        /// <param name="func">he function to execute in the try/catch block.</param>
        /// <param name="result">The result that <paramref name="result"/> returned if it did not error, otherwise this will return <c>default</c>.</param>
        /// <returns>Whether the function ran without any exception thrown.</returns>
        public static bool Try<T>(Func<T> func, out T? result) => Try(func, out result, out _);
        /// <summary>
        /// Runs <paramref name="func"/> in a try/catch block and returns whether it succeeded or not.
        /// Succeeded means whether it threw an exception or not. <para/>
        /// It also returns the result of <paramref name="func"/> in <paramref name="result"/>.
        /// </summary>
        /// <param name="func">The function to execute in the try/catch block.</param>
        /// <param name="result">The result that <paramref name="result"/> returned if it did not error, otherwise this will return <c>default</c>.</param>
        /// <param name="ex">The exception that was thrown; this will be null if <see langword="true"/> is returned.</param>
        /// <returns>Whether the function ran without any exception thrown.</returns>
        public static bool Try<T>(Func<T> func, out T? result, [NotNullWhen(false)] out Exception? ex)
        {
            result = default;
            ex = null;
            try { result = func(); }
            catch (Exception e)
            {
                ex = e;
                return false;
            }
            return true;
        }
        #endregion
    }
}
