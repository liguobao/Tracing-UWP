using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Input.Inking;
using Windows.UI.Input.Inking.Analysis;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace Tracing.Core
{
    public class InkToShapeAssKicker
    {
        public InkAnalyzer AnalyzerShape { get; } = new InkAnalyzer();

        public InkCanvas CurrentInkCanvas { get; }

        public InkDrawingAttributes CurrentDrawingAttributes { get; private set; }

        public DispatcherTimer DispatcherTimer { get; set; }

        public int RecognitionInterval { get; set; } = 200;

        public InkToShapeAssKicker(InkCanvas inkCanvas)
        {
            CurrentInkCanvas = inkCanvas;

            DispatcherTimer = new DispatcherTimer();
            DispatcherTimer.Tick += DispatcherTimer_Tick;
            DispatcherTimer.Interval = TimeSpan.FromMilliseconds(RecognitionInterval);
        }

        private async void DispatcherTimer_Tick(object sender, object e)
        {
            if (IsOn)
            {
                DispatcherTimer.Stop();
                if (!AnalyzerShape.IsAnalyzing)
                {
                    InkAnalysisResult results = await AnalyzerShape.AnalyzeAsync();
                    if (results.Status == InkAnalysisStatus.Updated)
                    {
                        ConvertShapes();
                    }
                }
                else
                {
                    // Ink analyzer is busy. Wait a while and try again.
                    DispatcherTimer.Start();
                }
            }
        }

        public bool IsOn { get; set; }

        public void CollectStrokes(IReadOnlyList<InkStroke> strokes)
        {
            if (IsOn)
            {
                DispatcherTimer.Stop();
                AnalyzerShape.AddDataForStrokes(strokes);
                CurrentDrawingAttributes = strokes[0].DrawingAttributes;
                DispatcherTimer.Start();
            }
        }

        public void EraseStrokes(IReadOnlyList<InkStroke> strokes)
        {
            if (IsOn)
            {
                DispatcherTimer.Stop();
                foreach (var stroke in strokes)
                {
                    AnalyzerShape.RemoveDataForStroke(stroke.Id);
                }
                DispatcherTimer.Start();
            }
        }

        private void ConvertShapes()
        {
            IReadOnlyList<IInkAnalysisNode> drawings = AnalyzerShape.AnalysisRoot.FindNodes(InkAnalysisNodeKind.InkDrawing);
            foreach (IInkAnalysisNode drawing in drawings)
            {
                var shape = (InkAnalysisInkDrawing)drawing;
                if (shape.DrawingKind == InkAnalysisDrawingKind.Drawing)
                {
                    continue;
                }

                if (shape.DrawingKind == InkAnalysisDrawingKind.Circle || shape.DrawingKind == InkAnalysisDrawingKind.Ellipse)
                {
                    continue;
                    //DrawEllipseTest(shape);
                }
                else
                {
                    DrawPolygon(shape);
                }

                foreach (var strokeId in shape.GetStrokeIds())
                {
                    InkStroke stroke = CurrentInkCanvas.InkPresenter.StrokeContainer.GetStrokeById(strokeId);
                    stroke.Selected = true;
                }

                AnalyzerShape.RemoveDataForStrokes(shape.GetStrokeIds());
            }
            CurrentInkCanvas.InkPresenter.StrokeContainer.DeleteSelected();
        }

        private void DrawEllipse(InkAnalysisInkDrawing shape)
        {
            Ellipse ellipse = new Ellipse();

            // Ellipses and circles are reported as four points
            // in clockwise orientation.
            // Points 0 and 2 are the extrema of one axis,
            // and points 1 and 3 are the extrema of the other axis.
            // See Ellipse.svg for a diagram.
            IReadOnlyList<Point> points = shape.Points;

            // Calculate the geometric center of the ellipse.
            var center = new Point((points[0].X + points[2].X) / 2.0, (points[0].Y + points[2].Y) / 2.0);

            // Calculate the length of one axis.
            ellipse.Width = Distance(points[0], points[2]);

            var compositeTransform = new CompositeTransform();
            if (shape.DrawingKind == InkAnalysisDrawingKind.Circle)
            {
                ellipse.Height = ellipse.Width;
            }
            else
            {
                // Calculate the length of the other axis.
                ellipse.Height = Distance(points[1], points[3]);

                // Calculate the amount by which the ellipse has been rotated
                // by looking at the angle our "width" axis has been rotated.
                // Since the Y coordinate is inverted, this calculates the amount
                // by which the ellipse has been rotated clockwise.
                double rotationAngle = Math.Atan2(points[2].Y - points[0].Y, points[2].X - points[0].X);

                RotateTransform rotateTransform = new RotateTransform();
                // Convert radians to degrees.
                compositeTransform.Rotation = rotationAngle * 180.0 / Math.PI;
                compositeTransform.CenterX = ellipse.Width / 2.0;
                compositeTransform.CenterY = ellipse.Height / 2.0;
            }

            compositeTransform.TranslateX = center.X - ellipse.Width / 2.0;
            compositeTransform.TranslateY = center.Y - ellipse.Height / 2.0;

            ellipse.RenderTransform = compositeTransform;

            //canvas.Children.Add(ellipse);
        }

        private double Distance(Point p0, Point p1)
        {
            double dX = p1.X - p0.X;
            double dY = p1.Y - p0.Y;
            return Math.Sqrt(dX * dX + dY * dY);
        }

        private void DrawPolygon(InkAnalysisInkDrawing shape)
        {
            var x = shape.Points.First().X;
            var y = shape.Points.First().Y;

            var newPoint = new Point(x, y);

            var newPoints = new List<Point>();
            newPoints.AddRange(shape.Points);
            newPoints.Add(newPoint);

            ApplyInkFromPoints(newPoints);
        }

        private void ApplyInkFromPoints(List<Point> points)
        {
            var builder = new InkStrokeBuilder();
            builder.SetDefaultDrawingAttributes(CurrentDrawingAttributes);
            for (int i = 0; i < points.Count; i++)
            {
                var strk = builder.CreateStroke(i
                    + 1 < points.Count ? 
                    new[] { points[i], points[i + 1] } : 
                    new[] { points[i], points[0] });

                CurrentInkCanvas.InkPresenter.StrokeContainer.AddStroke(strk);
            }
            //var stroke = builder.CreateStroke(points);
            //CurrentInkCanvas.InkPresenter.StrokeContainer.AddStroke(stroke);
        }
    }
}
