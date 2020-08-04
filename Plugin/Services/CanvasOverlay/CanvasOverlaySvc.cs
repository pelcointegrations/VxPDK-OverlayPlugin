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
            // Get coord values for the shape request
            var reqTopLeft = new Point(shape.TopLeft.X * RightSideUpWidth, shape.TopLeft.Y * RightSideUpHeight);
            var reqBottomRight = new Point(shape.BottomRight.X * RightSideUpWidth, shape.BottomRight.Y * RightSideUpHeight);
            var reqRect = new Rect(reqTopLeft, reqBottomRight);

            // Get coord values for dptz
            var dptzTopLeft = new Point(_normalizedDptzWindow.TopLeft.X * RightSideUpWidth, _normalizedDptzWindow.TopLeft.Y * RightSideUpHeight);
            var dptzBottomRight = new Point(_normalizedDptzWindow.BottomRight.X * RightSideUpWidth, _normalizedDptzWindow.BottomRight.Y * RightSideUpHeight);
            var dptzRect = new Rect(dptzTopLeft, dptzBottomRight);

            // If no intersection, no need to draw
            Rect rect = Rect.Intersect(dptzRect, reqRect);
            if (rect == Rect.Empty)
                return;

            // Adjust the overlays to the dptz area
            rect.X = rect.X - dptzRect.X;
            rect.Y = rect.Y - dptzRect.Y;

            // Determine multiplier (when dptz is being used)
            //double normal = RightSideUpWidth * RightSideUpHeight;
            //double dptz = dptzRect.Width * dptzRect.Height;
            //double multiplier = normal / dptz;

            // it seems as though the multiplier above should be able to calculate how much larger the shape should be, however it makes it slightly
            // bigger and bigger and bigger the more you zoom in.

            double multiplier = 1;

            int x1 = (int)(rect.TopLeft.X * multiplier);
            int y1 = (int)(rect.TopLeft.Y * multiplier);
            int x2 = (int)(rect.BottomRight.X * multiplier);
            int y2 = (int)(rect.BottomRight.Y * multiplier);

            if (x2 > x1 && y2 > y1)
            {
                _canvasViewModel.OverlayBitmap.DrawRectangle(x1, y1, x2, y2, shape.BorderColor);

                if (shape.FillColor != default)
                {
                    x1 += 1;
                    y1 += 1;

                    if (x2 > x1 && y2 > y1)
                        _canvasViewModel.OverlayBitmap.FillRectangle(x1, y1, x2, y2, shape.FillColor);
                }
            }
        }
    }
}
