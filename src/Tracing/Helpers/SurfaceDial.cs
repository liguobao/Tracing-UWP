using System;
using Windows.UI.Input;
using Tracing.Configuration;
using Tracing.Core;

namespace Tracing.Helpers
{
    public class SurfaceDial
    {
        private AppSettings Settings { get; set; }

        private RadialController _diaoController;
        public RadialControllerMenuItem DiaoToolOpacity { get; set; }
        public RadialControllerMenuItem DiaoToolUndoRedo { get; set; }
        public RadialControllerMenuItem DiaoToolZoom { get; set; }

        public RadialControllerMenuItem DiaoToolAlignmentGrid { get; set; }

        public RadialControllerConfiguration DiaoConfig { get; private set; }

        public event ZoomingEventHandler Zooming;
        public delegate void ZoomingEventHandler(RadialController sender, RadialControllerRotationChangedEventArgs e);

        public event UndoRedoEventHandler UndoRedo;
        public delegate void UndoRedoEventHandler(RadialController sender, RadialControllerRotationChangedEventArgs e);

        public event OpacityChangingEventHandler OpacityChanging;
        public delegate void OpacityChangingEventHandler(RadialController sender, RadialControllerRotationChangedEventArgs e);

        public event AlignmentGridChangingEventHandler AlignmentGridChanging;
        public delegate void AlignmentGridChangingEventHandler(RadialController sender, RadialControllerRotationChangedEventArgs e);

        public event OpacityInvokedEventHandler OpacityInvoked;
        public delegate void OpacityInvokedEventHandler(RadialControllerMenuItem sender, object args);

        public event UndoRedoInvokedEventHandler UndoRedoInvoked;
        public delegate void UndoRedoInvokedEventHandler(RadialControllerMenuItem sender, object args);

        public event ZoomInvokedEventHandler ZoomInvoked;
        public delegate void ZoomInvokedEventHandler(RadialControllerMenuItem sender, object args);

        public event AlignmentGridInvokedEventHandler AlignmentGridInvoked;
        public delegate void AlignmentGridInvokedEventHandler(RadialControllerMenuItem sender, object args);

        public SurfaceDial()
        {
            Settings = new AppSettings();
            RegisterSurfaceDial();
        }

        public void RegisterSurfaceDial()
        {

            if (RadialController.IsSupported())
            {
                _diaoController = RadialController.CreateForCurrentView();
                _diaoController.UseAutomaticHapticFeedback = Settings.EnableVibrateForSurfaceDial;
                _diaoController.RotationResolutionInDegrees = 1;

                // Opacity Tool
                DiaoToolOpacity = RadialControllerMenuItem.CreateFromFontGlyph(Edi.UWP.Helpers.Utils.GetResource("Tracing.Core/Resources/SurfaceDialMenu/Opacity"), "\xE71C", "Segoe MDL2 Assets");
                _diaoController.Menu.Items.Add(DiaoToolOpacity);
                DiaoToolOpacity.Invoked += (sender, args) =>
                {
                    OpacityInvoked?.Invoke(sender, args);
                };

                // Undo Tool
                DiaoToolUndoRedo = RadialControllerMenuItem.CreateFromFontGlyph(Edi.UWP.Helpers.Utils.GetResource("Tracing.Core/Resources/SurfaceDialMenu/Undo"), "\xE10E", "Segoe MDL2 Assets");
                _diaoController.Menu.Items.Add(DiaoToolUndoRedo);
                DiaoToolUndoRedo.Invoked += (sender, args) =>
                {
                    UndoRedoInvoked?.Invoke(sender, args);
                };

                // Zoom Tool
                DiaoToolZoom = RadialControllerMenuItem.CreateFromKnownIcon(Edi.UWP.Helpers.Utils.GetResource("Tracing.Core/Resources/SurfaceDialMenu/Zoom"), RadialControllerMenuKnownIcon.Zoom);
                _diaoController.Menu.Items.Add(DiaoToolZoom);
                DiaoToolZoom.Invoked += (sender, args) =>
                {
                    ZoomInvoked?.Invoke(sender, args);
                };

                // AlignmentGrid Tool
                DiaoToolAlignmentGrid = RadialControllerMenuItem.CreateFromFontGlyph(Edi.UWP.Helpers.Utils.GetResource("Tracing.Core/Resources/SurfaceDialMenu/AlignmentGrid"), "\xE80A", "Segoe MDL2 Assets");
                _diaoController.Menu.Items.Add(DiaoToolAlignmentGrid);
                DiaoToolAlignmentGrid.Invoked += (sender, args) =>
                {
                    AlignmentGridInvoked?.Invoke(sender, args);
                };

                _diaoController.RotationChanged += DiaoController_RotationChanged;

                DiaoConfig = RadialControllerConfiguration.GetForCurrentView();
                DiaoConfig.SetDefaultMenuItems(new RadialControllerSystemMenuItemKind[] { });
            }

        }

        private void DiaoController_RotationChanged(RadialController sender, RadialControllerRotationChangedEventArgs args)
        {
            var selectedTool = _diaoController.Menu.GetSelectedMenuItem();
            if (selectedTool == DiaoToolOpacity)
            {
                OpacityChanging?.Invoke(sender, args);
            }
            if (selectedTool == DiaoToolUndoRedo)
            {
                UndoRedo?.Invoke(sender, args);
            }
            if (selectedTool == DiaoToolZoom)
            {
                Zooming?.Invoke(sender, args);
            }
            if (selectedTool == DiaoToolAlignmentGrid)
            {
                AlignmentGridChanging?.Invoke(sender, args);
            }
        }
    }
}
