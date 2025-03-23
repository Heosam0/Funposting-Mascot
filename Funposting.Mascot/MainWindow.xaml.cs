using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Funposting.Mascot
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private DispatcherTimer _timer;
        private RECT _lastWindowRect;
        private double _offsetX = 40; // Оффсет по X
        private double _offsetY = -100; // Оффсет по Y
        private bool isAvoid = false;
        public MainWindow()
        {
            InitializeComponent();
            this.Left = 0;
            this.Top = 0;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100); // Частота проверки активного окошк 
            _timer.Tick += Timer_Tick;
            _timer.Start();

            this.MouseEnter += OnMouseEnter;
        }



      
        private void Timer_Tick(object sender, EventArgs e)
        {
            IntPtr currentWindowHandle = new WindowInteropHelper(this).Handle;
            IntPtr foregroundWindow = GetForegroundWindow();
            if (foregroundWindow != IntPtr.Zero)
            {
                RECT rect;
                if (GetWindowRect(foregroundWindow, out rect) && foregroundWindow != currentWindowHandle)
                {
                    if (rect.Left != _lastWindowRect.Left || rect.Top != _lastWindowRect.Top ||
                        rect.Right != _lastWindowRect.Right || rect.Bottom != _lastWindowRect.Bottom)
                    {
                        AnimateToPosition(rect.Left + _offsetX, rect.Top + _offsetY);

                        _lastWindowRect = rect;
                    }
                }
            }
        }

        private void AnimateToPosition(double targetLeft, double targetTop)
        {
            if (double.IsNaN(this.Left))
                this.Left = 0;
            if (double.IsNaN(this.Top))
                this.Top = 0;

            targetLeft = Math.Max(0, Math.Min(targetLeft, SystemParameters.PrimaryScreenWidth - this.Width));
            targetTop = Math.Max(0, Math.Min(targetTop, SystemParameters.PrimaryScreenHeight - this.Height));

            DoubleAnimation leftAnimation = new DoubleAnimation
            {
                To = targetLeft,
                Duration = TimeSpan.FromMilliseconds(300), 
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            DoubleAnimation topAnimation = new DoubleAnimation
            {
                To = targetTop,
                Duration = TimeSpan.FromMilliseconds(300), // Длительность анимации
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } 
            };

            BeginAnimation(Window.LeftProperty, leftAnimation);
            BeginAnimation(Window.TopProperty, topAnimation);
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (_lastWindowRect.Right > 0 && _lastWindowRect.Bottom > 0)
            {
                double newLeft = _lastWindowRect.Left + (_lastWindowRect.Right - _lastWindowRect.Left - this.Width);

                if (!isAvoid)
                {
                    AnimateToPosition(newLeft - _offsetX, _lastWindowRect.Top + _offsetY);
                    isAvoid = true;
                }
                else
                {
                    AnimateToPosition(_lastWindowRect.Left + _offsetX, _lastWindowRect.Top + _offsetY);
                    isAvoid = false;
                }
            }

            else
            {
                IntPtr foregroundWindow = GetForegroundWindow();
                if (foregroundWindow != IntPtr.Zero)
                {
                    RECT rect;
                    if (GetWindowRect(foregroundWindow, out rect))
                    {
                        if (rect.Left != _lastWindowRect.Left || rect.Top != _lastWindowRect.Top ||
                            rect.Right != _lastWindowRect.Right || rect.Bottom != _lastWindowRect.Bottom)
                        {
                            AnimateToPosition(rect.Left + _offsetX, rect.Top + _offsetY);
                            _lastWindowRect = rect;
                        }
                    }
                }
            }
        }
    }
}