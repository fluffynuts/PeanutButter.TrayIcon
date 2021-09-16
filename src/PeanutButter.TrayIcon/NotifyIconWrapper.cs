using System;
using System.Drawing;
using System.Windows.Forms;

namespace PeanutButter.TrayIcon
{
    /// <summary>
    /// Most of your needs should be met by the wrapping ITrayIcon, but if there's
    /// something missing, you can always "reach under the covers" here...
    /// </summary>
    public interface INotifyIcon : IDisposable
    {
        /// <summary>
        /// Provides raw access to the underlying win32 NotifyIcon
        /// </summary>
        NotifyIcon Actual { get; }

        /// <summary>
        /// Provides raw access to the icon
        /// </summary>
        Icon Icon { get; set; }

        /// <summary>
        /// Provides direct access to icon visibility
        /// </summary>
        bool Visible { get; set; }

        /// <summary>
        /// Provides direct access to the context menu
        /// </summary>
        ContextMenu ContextMenu { get; set; }

        /// <summary>
        /// Provides direct access to displaying a balloon tip
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="tipTitle"></param>
        /// <param name="tipText"></param>
        /// <param name="tipIcon"></param>
        void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon);
    }

    internal class NotifyIconWrapper : INotifyIcon
    {
        public NotifyIcon Actual { get; }

        public Icon Icon
        {
            get { return Actual.Icon; }
            set { Actual.Icon = value; }
        }

        public bool Visible
        {
            get { return Actual.Visible; }
            set { Actual.Visible = value; }
        }

        public ContextMenu ContextMenu
        {
            get { return Actual.ContextMenu; }
            set { Actual.ContextMenu = value; }
        }

        public string Text
        {
            get { return Actual.Text; }
            set { Actual.Text = value; }
        }

        public NotifyIconWrapper(NotifyIcon actual)
        {
            Actual = actual;
        }

        public void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon)
        {
            Actual.ShowBalloonTip(timeout, tipTitle, tipText, tipIcon);
        }

        public void Dispose()
        {
            Actual?.Dispose();
        }
    }
}