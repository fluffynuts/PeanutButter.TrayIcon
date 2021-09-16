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
        /// Exposes raw access to the underlying win32 NotifyIcon
        /// </summary>
        NotifyIcon Actual { get; }
        /// <summary>
        /// The Icon image shown in the tray
        /// </summary>
        Icon Icon { get; set; }
        /// <summary>
        /// Flag: is this tray icon visible or not
        /// </summary>
        bool Visible { get; set; }
        /// <summary>
        /// Raw access to the context menu
        /// </summary>
        ContextMenu ContextMenu { get; set; }
        /// <summary>
        /// The tooltip displayed when a user mouses over the icon
        /// </summary>
        string Text { get; set; }
        /// <summary>
        /// Shows a balloon tip (win10: this ends up in the notification center)
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
        
        public event MouseEventHandler MouseMove {
            add { Actual.MouseMove += value; }
            remove { Actual.MouseMove -= value; }
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