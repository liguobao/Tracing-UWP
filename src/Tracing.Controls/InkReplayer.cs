using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Tracing.Controls
{
    public class InkReplayer
    {
        private const int FPS = 60;

        DateTimeOffset beginTimeOfRecordedSession;
        DateTimeOffset endTimeOfRecordedSession;
        TimeSpan durationOfRecordedSession;
        DateTime beginTimeOfReplay;

        DispatcherTimer inkReplayTimer;

        InkStrokeBuilder strokeBuilder;
        IReadOnlyList<InkStroke> strokesToReplay;

        public ProgressBar ReplayProgress { get; set; }

        public InkCanvas Ink { get; set; }

        public Button BtnInkReplay { get; set; }

        public Button BtnStopReplay { get; set; }

        public InkReplayer(InkCanvas ink, Button btnInkReplay, Button btnStopReplay, ProgressBar replayProgress,
            Func<bool> canReplay, Action cantReplayAction)
        {
            Ink = ink;
            BtnInkReplay = btnInkReplay;
            ReplayProgress = replayProgress;
            BtnStopReplay = btnStopReplay;

            BtnStopReplay.Click += (sender, args) =>
            {
                StopReplay();
            };

            BtnInkReplay.Click += (sender, args) =>
            {
                var b = canReplay();
                if (b)
                {
                    Replay();
                }
                else
                {
                    cantReplayAction();
                }
            };
        }

        public void Replay()
        {
            if (strokeBuilder == null)
            {
                strokeBuilder = new InkStrokeBuilder();
                inkReplayTimer = new DispatcherTimer { Interval = new TimeSpan(TimeSpan.TicksPerSecond / FPS) };
                inkReplayTimer.Tick += InkReplayTimer_Tick;
            }

            strokesToReplay = Ink.InkPresenter.StrokeContainer.GetStrokes();

            BtnInkReplay.IsEnabled = false;
            Ink.InkPresenter.IsInputEnabled = false;

            // Calculate the beginning of the earliest stroke and the end of the latest stroke.
            // This establishes the time period during which the strokes were collected.
            beginTimeOfRecordedSession = DateTimeOffset.MaxValue;
            endTimeOfRecordedSession = DateTimeOffset.MinValue;

            for (int i = 0; i < strokesToReplay.Count; i++)
            {
                var previousStroke = i >= 1 ? strokesToReplay[i - 1] : null;
                var stroke = strokesToReplay[i];

                // Skip empty time between strokes
                if (null != previousStroke)
                {
                    stroke.StrokeStartedTime = previousStroke.StrokeStartedTime + previousStroke.StrokeDuration;
                }

                var startTime = stroke.StrokeStartedTime;
                var duration = stroke.StrokeDuration;
                if (startTime.HasValue && duration.HasValue)
                {
                    if (beginTimeOfRecordedSession > startTime.Value)
                    {
                        beginTimeOfRecordedSession = startTime.Value;
                    }
                    if (endTimeOfRecordedSession < startTime.Value + duration.Value)
                    {
                        endTimeOfRecordedSession = startTime.Value + duration.Value;
                    }
                }
            }

            //foreach (var stroke in strokesToReplay)
            //{
            //    var startTime = stroke.StrokeStartedTime;
            //    var duration = stroke.StrokeDuration;
            //    if (startTime.HasValue && duration.HasValue)
            //    {
            //        if (beginTimeOfRecordedSession > startTime.Value)
            //        {
            //            beginTimeOfRecordedSession = startTime.Value;
            //        }
            //        if (endTimeOfRecordedSession < startTime.Value + duration.Value)
            //        {
            //            endTimeOfRecordedSession = startTime.Value + duration.Value;
            //        }
            //    }
            //}

            // If we found at least one stroke with a timestamp, then we can replay.
            if (beginTimeOfRecordedSession != DateTimeOffset.MaxValue)
            {
                durationOfRecordedSession = endTimeOfRecordedSession - beginTimeOfRecordedSession;

                ReplayProgress.Maximum = durationOfRecordedSession.TotalMilliseconds;
                ReplayProgress.Value = 0.0;

                beginTimeOfReplay = DateTime.Now;
                inkReplayTimer.Start();
            }
            else
            {
                // There was nothing to replay. Either there were no strokes at all,
                // or none of the strokes had timestamps.
                StopReplay();
            }
        }

        private void InkReplayTimer_Tick(object sender, object e)
        {
            var currentTimeOfReplay = DateTimeOffset.Now;
            var timeElapsedInReplay = currentTimeOfReplay - beginTimeOfReplay;

            ReplayProgress.Value = timeElapsedInReplay.TotalMilliseconds;

            var timeEquivalentInRecordedSession = beginTimeOfRecordedSession + timeElapsedInReplay;
            Ink.InkPresenter.StrokeContainer = GetCurrentStrokesView(timeEquivalentInRecordedSession);

            if (timeElapsedInReplay > durationOfRecordedSession)
            {
                StopReplay();
            }
        }

        private InkStrokeContainer GetCurrentStrokesView(DateTimeOffset time)
        {
            var inkStrokeContainer = new InkStrokeContainer();

            // The purpose of this sample is to demonstrate the timestamp usage,
            // not the algorithm. (The time complexity of the code is O(N^2).)
            foreach (var stroke in strokesToReplay)
            {
                var s = GetPartialStroke(stroke, time);
                if (s != null)
                {
                    inkStrokeContainer.AddStroke(s);
                }
            }

            return inkStrokeContainer;
        }

        private InkStroke GetPartialStroke(InkStroke stroke, DateTimeOffset time)
        {
            var startTime = stroke.StrokeStartedTime;
            var duration = stroke.StrokeDuration;
            if (!startTime.HasValue || !duration.HasValue)
            {
                // If a stroke does not have valid timestamp, then treat it as
                // having been drawn before the playback started.
                // We must return a clone of the stroke, because a single stroke cannot
                // exist in more than one container.
                return stroke.Clone();
            }

            if (time < startTime.Value)
            {
                // Stroke has not started
                return null;
            }

            if (time >= startTime.Value + duration.Value)
            {
                // Stroke has already ended.
                // We must return a clone of the stroke, because a single stroke cannot exist in more than one container.
                return stroke.Clone();
            }

            // Stroke has started but not yet ended.
            // Create a partial stroke on the assumption that the ink points are evenly distributed in time.
            var points = stroke.GetInkPoints();
            var portion = (time - startTime.Value).TotalMilliseconds / duration.Value.TotalMilliseconds;
            var count = (int)((points.Count - 1) * portion) + 1;
            return strokeBuilder.CreateStrokeFromInkPoints(points.Take(count), Matrix3x2.Identity, startTime, time - startTime);
        }

        public void StopReplay()
        {
            inkReplayTimer?.Stop();
            BtnInkReplay.IsEnabled = true;
            Ink.InkPresenter.IsInputEnabled = true;
        }
    }
}
