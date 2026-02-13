using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LechYTDLP.Services
{
    public record InfoBarMessage(
        string Title,
        string Message,
        InfoBarSeverity Severity,
        int DurationMs = 5000,
        bool IsCancelable = false
    );

    public class InfoBarService
    {
        private readonly Queue<InfoBarMessage> _queue = new();
        private InfoBar? _infoBar;
        private bool _isShowing;
        private CancellationTokenSource? _cts;

        public void Register(InfoBar infoBar)
        {
            _infoBar = infoBar;
        }

        public void Show(InfoBarMessage msg)
        {
            _queue.Enqueue(msg);
            TryShowNext();
        }

        private async void TryShowNext()
        {
            if (_isShowing || _infoBar == null || _queue.Count == 0)
                return;

            _isShowing = true;
            _cts = new CancellationTokenSource();

            var msg = _queue.Dequeue();

            _infoBar.Title = msg.Title;
            _infoBar.Message = msg.Message;
            _infoBar.Severity = msg.Severity;

            // Süresiz mesajlar ZORUNLU cancellable
            _infoBar.IsClosable = msg.IsCancelable || msg.DurationMs <= 0;

            _infoBar.Opacity = 0;
            _infoBar.IsOpen = true;

            AnimateOpacity(0, 1);

            _infoBar.Closed += OnClosed;

            // ⛔ Süresiz ise burada dur
            if (msg.DurationMs <= 0)
                return;

            try
            {
                await Task.Delay(msg.DurationMs, _cts.Token);
            }
            catch { }

            CloseInfoBar();
        }


        private void OnClosed(InfoBar sender, InfoBarClosedEventArgs args)
        {
            _cts?.Cancel();
            sender.Closed -= OnClosed;
            Finish();
        }

        private async void CloseInfoBar()
        {
            AnimateOpacity(1, 0);
            await Task.Delay(200);
            Finish();
        }

        private void Finish()
        {
            if (_infoBar != null)
                _infoBar.IsOpen = false;

            _isShowing = false;
            TryShowNext();
        }

        private void AnimateOpacity(double from, double to)
        {
            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromMilliseconds(200)),
                EnableDependentAnimation = true
            };

            var storyboard = new Storyboard();
            Storyboard.SetTarget(animation, _infoBar);
            Storyboard.SetTargetProperty(animation, "Opacity");
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }
    }
}
