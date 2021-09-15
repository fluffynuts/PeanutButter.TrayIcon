using System;

namespace PeanutButter.TrayIcon
{
    /// <summary>
    /// Thrown when attempting to manipulate a TrayIcon which hasn't been
    /// initialized yet, or which has been disposed.
    /// </summary>
    public class TrayIconNotInitializedException : Exception
    {
        /// <inheritdoc />
        public TrayIconNotInitializedException()
            : base("This instance of the TrayIcon has either been disposed or is not fully initialized yet")
        {
        }
    }
}