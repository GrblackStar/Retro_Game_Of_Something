#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Game.World2D;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Game.World2D.SceneControl;
using Emotion.Game.World3D;
using Emotion.Game.World3D.SceneControl;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Shading;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using Emotion.Platform.Debugger;
using Emotion.Primitives;
using Emotion.Testing;
using Emotion.UI;
using Emotion.Utility;
using Microsoft.VisualBasic.FileIO;
using OpenGL;
using Take_1;
using WinApi.User32;

#endregion
namespace Emotion.ExecTest;

public class Program
{
    private static void Main(string[] args)
    {
        var config = new Configurator
        {
            DebugMode = true
        };

        Engine.Setup(config);

        Engine.SceneManager.SetScene(new TakeOneGame());

        Engine.Run();
    }
}


public class TakeOneGame : World3DBaseScene<Take1Map>
{
    public UIController UI;

    public Type ObjectTypeToPlace;

    public override async Task LoadAsync()
    {
        var cam3D = new Camera3D(new Vector3(100));
        cam3D.LookAtPoint(Vector3.Zero);
        Engine.Renderer.Camera = cam3D;

        await ChangeMapAsync(new Take1Map(new Vector2(10, 10)));

        UI = new UIController();
        AddPlaceableObjectMenu(UI);
    }

    public override void Draw(RenderComposer composer)
    {
        composer.SetUseViewMatrix(false);
        composer.RenderSprite(Vector3.Zero, composer.CurrentTarget.Size, Color.CornflowerBlue);
        composer.ClearDepth();
        composer.SetUseViewMatrix(true);

        base.Draw(composer);

        composer.SetUseViewMatrix(false);
        UI.Render(composer);
    }

    public override void Update()
    {
        if (Engine.Host.IsMouseKeyDown(Platform.Input.MouseKey.Left))
        {
            Ray3D ray3D = (Engine.Renderer.Camera as Camera3D).GetCameraMouseRay();
            Vector3 position = new Vector3(0, 0, 0);

            foreach (var obj in CurrentMap.GetObjectsByType<GroundTile>())
            {
                if(ray3D.IntersectWithObject(obj, out Mesh _, out position, out Vector3 _, out int _))
                {
                    (CurrentMap as Take1Map).AddTreeObjects(position);
                }
                
            }
        }

        base.Update();
        UI.Update();
    }

    protected void AddPlaceableObjectMenu(UIController ui)
    {
        UISolidColor barContainer = new UISolidColor();
        barContainer.StretchX = true;
        barContainer.StretchY = true;
        barContainer.WindowColor = Color.Black * 0.75f;
        barContainer.Paddings = new Rectangle(2, 2, 2, 2);
        barContainer.Anchor = UIAnchor.BottomCenter;
        barContainer.ParentAnchor = UIAnchor.BottomCenter;
        barContainer.Margins = new Rectangle(0, 0, 0, 5);

        var bar = new UICallbackListNavigator();
        bar.StretchX = true;
        bar.StretchY = true;
        bar.IgnoreParentColor = true;
        bar.LayoutMode = LayoutMode.HorizontalList;
        barContainer.AddChild(bar);

        var types = EditorUtility.GetTypesWhichInherit<GameObject3D>();
        for (int i = 0; i < types.Count; i++)
        {
            var type = types[i];
            if (type.Assembly == typeof(Take1Map).Assembly)
            {
                UIObject3DWindow thisObjectWin = new UIObject3DWindow(type);
                thisObjectWin.MaxSize = new Vector2(32);
                thisObjectWin.MinSize = new Vector2(32);
                
                bar.AddChild(thisObjectWin);
            }
        }
        bar.SetupMouseSelection();

        UIObject3DWindow? oldSel = null;
        bar.OnChoiceConfirmed = (nu, _) =>
        {
            if (oldSel != null) oldSel.Selected = false;

            var nuAs = nu as UIObject3DWindow;
            Assert.NotNull(nuAs);

            nuAs.Selected = true;
            ObjectTypeToPlace = nuAs.Type;
            oldSel = nuAs;
        };

        ui.AddChild(barContainer);
    }

    public class UIObject3DWindow : UICallbackButton
    {
        public Type Type;
        public bool Selected;

        protected FrameBuffer? _previewImage;
        protected GameObject3D? _previewObject;
        protected Task _previewObjectLoading;
        protected Camera3D previewCamera;

        public UIObject3DWindow(Type type)
        {
            Type = type;
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            if (_previewObject == null)
            {
                _previewObject = (GameObject3D?)Activator.CreateInstance(Type);
                if (_previewObject == null) // Failed to create instance of.
                {
                    Visible = false;
                    return false;
                }

                _previewObjectLoading = Task.Run(_previewObject.LoadAssetsAsync);
            }

            if (_previewImage == null && _previewObjectLoading.Status == TaskStatus.RanToCompletion)
            {
                previewCamera = new Camera3D(new Vector3(100, 200, 100));
                previewCamera.LookAtPoint(Vector3.Zero + new Vector3(0, 0, previewCamera.Z / 2f));
                previewCamera.Update();

                _previewImage = new FrameBuffer(new Vector2(64, 64)).WithColor();
                RenderDoc.StartCapture();

                CameraBase oldCamera = c.Camera;
                c.RenderToAndClear(_previewImage);
                c.SetUseViewMatrix(true);
                c.Camera = previewCamera;

                _previewObject.Update(0);
                _previewObject.Render(c);
                c.RenderTo(null);
                c.SetUseViewMatrix(false);
                c.Camera = oldCamera;

                RenderDoc.EndCapture();
            }

            c.RenderSprite(Position, Size, new Color(53, 53, 53) * 0.50f);

            if (_previewImage != null)
            {
                c.RenderSprite(Position, Size, _previewImage.ColorAttachment);
            }

            if (Selected)
            {
                c.RenderOutline(Position, Size, Color.PrettyOrange, 2);
            }

            return base.RenderInternal(c);
        }
    }
}