using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LechYTDLP.Services
{

    public class InfoBarHyperlinkButton
    {
        public required string Content { get; init; }
        public required Uri NavigateUri { get; init; }
    }

    public class InfoBarMessage
    {
        // Required properties
        public required string Title { get; init; }
        public required string Message { get; init; }
        public required InfoBarSeverity Severity { get; init; }
        // Optional action button
        public InfoBarHyperlinkButton? HyperlinkButton { get; init; } = null;
        // Optional properties with default values
        public int DurationMs { get; init; } = 5000;
        public bool IsCancelable { get; init; } = false;
    }

    public class InfoBarService
    {
        private readonly Queue<InfoBarMessage> _queue = new();
        private InfoBar? _infoBar;
        private bool _isShowing;
        private CancellationTokenSource? _cts;
        private DispatcherQueue? _dispatcher;

        public void Register(InfoBar infoBar)
        {
            _infoBar = infoBar;
            _dispatcher = infoBar.DispatcherQueue;
        }

        public void Show(InfoBarMessage msg)
        {
            _queue.Enqueue(msg);
            ProcessQueue();
        }

        private void ProcessQueue()
        {
            if (_dispatcher == null)
                return;

            _dispatcher.TryEnqueue(() =>
            {
                TryShowNext();
            });
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

            // Making background more transparent for success messages to make it less intrusive
            //if (msg.Severity == InfoBarSeverity.Success)
            //{
            //    _infoBar.Background = blabla
            //}

            if (msg.HyperlinkButton != null)
            {
                _infoBar.ActionButton = new HyperlinkButton
                {
                    Content = msg.HyperlinkButton.Content,
                    NavigateUri = msg.HyperlinkButton.NavigateUri
                };
            }

            _infoBar.IsClosable = msg.IsCancelable || msg.DurationMs <= 0;

            _infoBar.Opacity = 0;
            _infoBar.IsOpen = true;

            AnimateOpacity(0, 1);

            _infoBar.Closed += OnClosed;

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
            ProcessQueue();
        }

        private void AnimateOpacity(double from, double to)
        {
            if (_infoBar == null)
                return;

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
