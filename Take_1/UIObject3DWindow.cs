using Emotion.Game.World2D;
using Emotion.Game.World3D;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Objects;
using Emotion.Graphics;
using Emotion.Platform.Debugger;
using Emotion.Primitives;
using Emotion.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Take_1
{

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
                _previewObject.ObjectState = ObjectState.Alive;
                _previewObject.Init();
                var boundSphere = _previewObject.BoundingSphere;

                previewCamera = new Camera3D(boundSphere.Origin + new Vector3(0, boundSphere.Radius * 2.5f, boundSphere.Radius));
                previewCamera.LookAtPoint(boundSphere.Origin);
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
