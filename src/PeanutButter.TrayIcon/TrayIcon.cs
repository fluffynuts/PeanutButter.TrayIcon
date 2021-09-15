using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace PeanutButter.TrayIcon
{
    /// <inheritdoc />
    public class TrayIcon : ITrayIcon
    {
        /// <inheritdoc />
        public INotifyIcon NotifyIcon { get; internal set; }

        private NotifyIcon RawIcon => NotifyIcon?.Actual
            ?? throw new TrayIconNotInitializedException();

        private Icon _icon;

        /// <inheritdoc />
        public bool ShowingDefaultBalloonTip => _showingDefaultBalloonTip;

        private bool _showingDefaultBalloonTip;

        /// <inheritdoc />
        public int DefaultBalloonTipTimeout { get; set; }

        private readonly object _lock = new object();

        /// <summary>
        /// Provides an array of the currently-registered mouse click handlers
        /// </summary>
        public IEnumerable<MouseClickHandler> MouseClickHandlers => _mouseClickHandlers.ToArray();

        private readonly List<MouseClickHandler> _mouseClickHandlers = new List<MouseClickHandler>();

        /// <inheritdoc />
        public Icon Icon
        {
            get { return _icon; }
            set
            {
                _icon = value;
                RawIcon.Icon = _icon;
            }
        }

        /// <inheritdoc />
        public string DefaultTipText { get; set; }

        /// <inheritdoc />
        public string DefaultTipTitle { get; set; }

        /// <inheritdoc />
        public Action DefaultBalloonTipClickedAction { get; set; }

        /// <inheritdoc />
        public Action DefaultBalloonTipClosedAction { get; set; }

        /// <summary>
        /// Parameterless constructor. Don't forget to Init later!
        /// </summary>
        public TrayIcon()
        {
        }

        /// <summary>
        /// Construct and Init with an Icon
        /// </summary>
        /// <param name="icon"></param>
        public TrayIcon(Icon icon)
        {
            Init(icon);
        }

        /// <summary>
        /// Construct and Init with a Bitmap
        /// </summary>
        /// <param name="icon"></param>
        public TrayIcon(Bitmap icon)
        {
            Init(icon);
        }

        /// <summary>
        /// Construct and Init with a stream providing icon data
        /// </summary>
        /// <param name="iconImageStream"></param>
        public TrayIcon(Stream iconImageStream)
        {
            Init(iconImageStream);
        }

        /// <inheritdoc />
        public void Init(Stream iconImageStream)
        {
            var icon = new Icon(iconImageStream);
            Init(icon);
        }

        /// <summary>
        /// Construct and Init with a path to an icon file
        /// </summary>
        /// <param name="pathToIcon"></param>
        public TrayIcon(string pathToIcon)
        {
            Init(pathToIcon);
        }

        /// <inheritdoc />
        public void Init(string pathToIcon)
        {
            using (var fileStream = new FileStream(pathToIcon, FileMode.Open, FileAccess.Read))
            {
                Init(fileStream);
            }
        }

        /// <inheritdoc />
        public void Init(Bitmap bitmap)
        {
            Init(Icon.FromHandle(bitmap.GetHicon()));
        }

        internal BalloonTipClickHandlerRegistration BalloonTipClickHandlers => _balloonTipClickHandlers;
        private BalloonTipClickHandlerRegistration _balloonTipClickHandlers;
        private bool _alreadyInitialized;

        /// <inheritdoc />
        public void ShowBalloonTipFor(int timeoutInMilliseconds,
            string title,
            string text,
            ToolTipIcon icon,
            Action clickAction = null,
            Action closeAction = null)
        {
            lock (this)
            {
                _balloonTipClickHandlers = new BalloonTipClickHandlerRegistration(
                    clickAction,
                    closeAction
                );
            }

            NotifyIcon.ShowBalloonTip(timeoutInMilliseconds, title, text, icon);
        }

        /// <inheritdoc />
        public MenuItem AddSubMenu(
            string text
        )
        {
            return AddSubMenu(text, null);
        }

        /// <inheritdoc />
        public MenuItem AddSubMenu(
            string text,
            MenuItem parent
        )
        {
            lock (_lock)
            {
                var menuItem = CreateMenuItemWithText(text);
                AddMenuToParentOrRoot(parent, menuItem);
                return menuItem;
            }
        }

        /// <inheritdoc />
        public MenuItem AddMenuItem(
            string withText,
            Action withCallback
        )
        {
            return AddMenuItem(
                withText,
                withCallback,
                null
            );
        }

        /// <inheritdoc />
        public MenuItem AddMenuItem(string withText,
            Action withCallback,
            MenuItem parent)
        {
            lock (_lock)
            {
                var menuItem = CreateMenuItemWithText(withText);
                if (withCallback != null)
                {
                    menuItem.Click += (s, e) => withCallback();
                }

                AddMenuToParentOrRoot(parent, menuItem);
                return menuItem;
            }
        }

        /// <inheritdoc />
        public void AddMenuSeparator()
        {
            AddMenuSeparator(null);
        }

        /// <inheritdoc />
        public void AddMenuSeparator(MenuItem subMenu)
        {
            AddSubMenu("-", subMenu);
        }

        /// <inheritdoc />
        public void RemoveMenuItem(string withText)
        {
            lock (_lock)
            {
                var toRemove = FindMenusByText(withText);
                foreach (var item in toRemove)
                {
                    item.Parent.MenuItems.Remove(item);
                }
            }
        }

        /// <inheritdoc />
        public void Show()
        {
            RawIcon.MouseClick += OnIconMouseClick;
            RawIcon.MouseDoubleClick += OnIconMouseDoubleClick;
            RawIcon.Icon = _icon;
            RawIcon.Visible = true;
        }

        /// <inheritdoc />
        public void Hide()
        {
            RawIcon.Visible = false;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            lock (_lock)
            {
                _alreadyInitialized = false;
                NotifyIcon?.Dispose();
                NotifyIcon = null;
            }
        }

        /// <inheritdoc />
        public MouseClickHandler AddMouseClickHandler(MouseClicks clicks, MouseButtons button, Action handler)
        {
            lock (_lock)
            {
                var handlerItem = new MouseClickHandler(clicks, button, handler);
                _mouseClickHandlers.Add(handlerItem);
                return handlerItem;
            }
        }

        /// <inheritdoc />
        public void RemoveMouseClickHandler(MouseClickHandler handler)
        {
            lock (_lock)
            {
                _mouseClickHandlers.Remove(handler);
            }
        }

        private void AddMenuToParentOrRoot(MenuItem parent, MenuItem menuItem)
        {
            var addTo = parent as Menu ?? RawIcon.ContextMenu;
            addTo.MenuItems.Add(menuItem);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        internal void OnIconMouseClick(object sender, MouseEventArgs e)
        {
            var handlers = FindHandlersFor(MouseClicks.Single, e.Button);
            RunMouseClickHandlers(handlers);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        internal void OnIconMouseDoubleClick(object sender, MouseEventArgs e)
        {
            var handlers = FindHandlersFor(MouseClicks.Double, e.Button);
            RunMouseClickHandlers(handlers);
        }

        private void RunMouseClickHandlers(MouseClickHandler[] handlers)
        {
            var exceptions = handlers
                .Select(handler => TryDo(handler.Action))
                .Where(ex => ex != null)
                .ToArray();
            if (exceptions.Any())
                throw new AggregateException(exceptions);
        }

        private Exception TryDo(Action action)
        {
            try
            {
                action();
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }

        private MouseClickHandler[] FindHandlersFor(MouseClicks clicks, MouseButtons button)
        {
            lock (_lock)
            {
                return _mouseClickHandlers
                    .Where(o => o.Clicks == clicks && o.Button == button)
                    .ToArray();
            }
        }

        private MenuItem[] FindMenusByText(string text)
        {
            var allMenuItems = FindAllMenuItems();
            var matches = allMenuItems.Where(mi => mi.Text == text);
            return matches.ToArray();
        }

        private List<MenuItem> FindAllMenuItems(List<MenuItem> foundSoFar = null, Menu parent = null)
        {
            foundSoFar = foundSoFar ?? new List<MenuItem>();
            parent = parent ?? RawIcon.ContextMenu;
            foreach (var item in parent.MenuItems)
            {
                var menuItem = item as MenuItem;
                if (menuItem == null)
                    continue; // not sure what else we could find in here?
                foundSoFar.Add(menuItem);
                FindAllMenuItems(foundSoFar, menuItem);
            }

            return foundSoFar;
        }

        /// <inheritdoc />
        public void Init(Icon icon)
        {
            if (_alreadyInitialized)
            {
                throw new TrayIconAlreadyInitializedException();
            }

            _alreadyInitialized = true;
            _icon = icon;
            DefaultBalloonTipTimeout = 2000;
            NotifyIcon = new NotifyIconWrapper(new NotifyIcon
            {
                ContextMenu = new ContextMenu()
            });
            RawIcon.MouseMove += ShowDefaultBalloonTip;
            RawIcon.BalloonTipClicked += BalloonTipClickedHandler;
            RawIcon.BalloonTipClosed += BalloonTipClosedHandler;
        }

        private void BalloonTipClosedHandler(object sender, EventArgs e)
        {
            var toRun = GetCustomBalloonTipClosedAction() ?? DefaultBalloonTipClosedAction;
            toRun?.Invoke();
        }

        private void BalloonTipClickedHandler(object sender, EventArgs e)
        {
            var toRun = GetCustomBalloonTipClickAction() ?? DefaultBalloonTipClickedAction;
            toRun?.Invoke();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        internal void ShowDefaultBalloonTip(object sender, MouseEventArgs e)
        {
            if (string.IsNullOrEmpty(DefaultTipText) || string.IsNullOrEmpty(DefaultTipTitle)) return;
            lock (this)
            {
                if (HaveRegisteredClickHandlers()) return;
                if (_showingDefaultBalloonTip) return;
                _showingDefaultBalloonTip = true;
            }

            ShowBalloonTipFor(
                DefaultBalloonTipTimeout,
                DefaultTipTitle,
                DefaultTipText,
                ToolTipIcon.Info,
                DefaultBalloonTipClickedAction,
                () =>
                {
                    _showingDefaultBalloonTip = false;
                    _balloonTipClickHandlers = null;
                    var closedAction = DefaultBalloonTipClosedAction;
                    closedAction?.Invoke();
                });
        }

        private bool HaveRegisteredClickHandlers()
        {
            lock (_lock)
            {
                return CustomBalloonTipHandlerExists_UNLOCKED();
            }
        }

        private bool CustomBalloonTipHandlerExists_UNLOCKED()
        {
            return _balloonTipClickHandlers != null;
        }

        private Action GetCustomBalloonTipClickAction()
        {
            lock (_lock)
            {
                return CustomBalloonTipHandlerExists_UNLOCKED()
                    ? _balloonTipClickHandlers.ClickAction
                    : null;
            }
        }

        private Action GetCustomBalloonTipClosedAction()
        {
            lock (_lock)
            {
                return CustomBalloonTipHandlerExists_UNLOCKED()
                    ? _balloonTipClickHandlers.ClosedAction
                    : null;
            }
        }

        private static MenuItem CreateMenuItemWithText(string withText)
        {
            return new MenuItem()
            {
                Text = withText
            };
        }

        /// <inheritdoc />
        public void RenameMenuItem(
            string from,
            string to
        )
        {
            var items = FindMenusByText(from);
            foreach (var item in items)
            {
                item.Text = to;
            }
        }

        /// <inheritdoc />
        public void ClearMenu()
        {
            NotifyIcon.ContextMenu = new ContextMenu();
        }

        /// <inheritdoc />
        public void ExitApplication()
        {
            Hide();
            Application.Exit();
        }
    }
}