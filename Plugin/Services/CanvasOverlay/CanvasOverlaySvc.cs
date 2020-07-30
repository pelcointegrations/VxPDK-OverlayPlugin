using NLog;
using PluginNs.Services.CanvasOverlay.Types.Shapes;
using PluginNs.Utilities;
using PluginNs.ViewModels;
using PluginNs.Views;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PluginNs.Services.CanvasOverlay
{
    class CanvasOverlaySvc : ICanvasOverlaySvc
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private double RightSideUpWidth => _rotation / 90 % 2 == 0 ? Width : Height;
        private double RightSideUpHeight => _rotation / 90 % 2 == 0 ? Height : Width;
        private double Width => _canvasView.ActualWidth * _normalizedVideoWindow.Width;
        private double Height => _canvasView.ActualHeight * _normalizedVideoWindow.Height;

        private readonly CanvasView _canvasView;
        private readonly CanvasViewModel _canvasViewModel;
        private DebounceDispatcher _redrawDispatcher;
        private Task _redrawTask;
        private CancellationTokenSource _redrawCts;
        private bool _redraw;
        private ConcurrentDictionary<string, BaseShape> _shapes;
        private Rect _normalizedDptzWindow;
        private Rect _normalizedVideoWindow;
        private double _aspectRatio;
        private double _rotation;

        public CanvasOverlaySvc(CanvasView canvasView)
        {
            _canvasView = canvasView;
            _redrawDispatcher = new DebounceDispatcher();
            _shapes = new ConcurrentDictionary<string, BaseShape>();

            _canvasViewModel = _canvasView.DataContext as CanvasViewModel;

            _canvasView.SizeChanged += CanvasViewSizeChanged;

            _redrawCts = new CancellationTokenSource();
            _redrawTask = Task.Run(RedrawTask);
        }

        private void CanvasViewSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Redraw();
        }

        public FrameworkElement CreateVideoOverlay()
        {
            return _canvasView;
        }

        public void OnVideoViewDigitalPtzInfo(Rect normalizedDPTZWindow)
        {
            _normalizedDptzWindow = normalizedDPTZWindow;
            Redraw();
        }

        public void OnVideoViewStreamAspectRatio(double aspectRatio)
        {
            _aspectRatio = aspectRatio;
            Redraw();
        }

        public void OnVideoViewWindow(Rect normalizedVideoWindow, double rotation)
        {
            _normalizedVideoWindow = normalizedVideoWindow;
            _rotation = rotation;
            Redraw();
        }

        public void CreateShape(BaseShape shape)
        {
            _shapes.TryAdd(shape.Id, shape);
        }

        public void UpdateShape(BaseShape shape)
        {
            shape.IsDirty = true;
        }

        public void DeleteShape(BaseShape shape)
        {
            shape.Delete = true;
        }

        private void Dispatch(Action action)
        {
            if (!_canvasView.CheckAccess())
            {
                _canvasView.Dispatcher.BeginInvoke(new Action<Action>(Dispatch), action);
                return;
            }

            action();
        }

        private void Redraw()
        {
            _redrawDispatcher.Debounce(100, _ => _redraw = true, disp: _canvasView.Dispatcher);
        }

        private async Task RedrawTask()
        {
            while (!_redrawCts.IsCancellationRequested)
            {
                DateTime now = DateTime.Now;
                if (_redraw || _shapes.Values.Any(s => s.IsDirty))
                {
                    _redraw = false;

                    Dispatch(() => _canvasViewModel.CreateBitmap(RightSideUpWidth, RightSideUpHeight, _rotation));

                    foreach (var shape in _shapes.Values)
                    {
                        if (!shape.Delete)
                            Dispatch(() => Create(shape));
                        else
                            _shapes.TryRemove(shape.Id, out _);
                        shape.IsDirty = false;
                    }
                }

                double ellapsedMs = (DateTime.Now - now).TotalMilliseconds;
                double delay = Math.Max(0, 20 - ellapsedMs);
                await Task.Delay(TimeSpan.FromMilliseconds(delay));
            }
        }

        private void Create(BaseShape shape)
        {
            if (shape is RectangleShape rectangleShape)
                DrawRectangle(rectangleShape);
        }

        private void DrawRectangle(RectangleShape shape)
        {
            var topLeft = new Point(shape.TopLeft.X * RightSideUpWidth, shape.TopLeft.Y * RightSideUpHeight);
            var bottomRight = new Point(shape.BottomRight.X * RightSideUpWidth, shape.BottomRight.Y * RightSideUpHeight);

            _canvasViewModel.OverlayBitmap.DrawRectangle(
                (int)topLeft.X,
                (int)topLeft.Y,
                (int)bottomRight.X,
                (int)bottomRight.Y,
                shape.BorderColor);

            if (shape.FillColor != default)
            {
                _canvasViewModel.OverlayBitmap.FillRectangle(
                    (int)topLeft.X + 1,
                    (int)topLeft.Y + 1,
                    (int)bottomRight.X,
                    (int)bottomRight.Y,
                    shape.FillColor);
            }
        }
    }
}
