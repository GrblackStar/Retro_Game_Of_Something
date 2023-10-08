﻿#region Using

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
using Emotion.Platform.Input;
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
            DebugMode = true,
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

    public GameObject3D GhostObject;

    public override async Task LoadAsync()
    {
        var cam3D = new Camera3D(new Vector3(100));
        cam3D.LookAtPoint(Vector3.Zero);
        Engine.Renderer.Camera = cam3D;

        await ChangeMapAsync(new Take1Map(new Vector2(10, 10)));

        UI = new UIController();
        AddPlaceableObjectMenu(UI);

        Engine.Host.OnKey.AddListener(KeyHandler, KeyListenerType.Game);
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

    private bool KeyHandler(Key key, KeyStatus status)
    {
        if (key == Key.MouseKeyLeft && ObjectTypeToPlace != null)
        {
            if (status == KeyStatus.Down)
            {
                var thisTreeObject = (GameObject3D)Activator.CreateInstance(ObjectTypeToPlace);
                thisTreeObject.Position = GhostObject.Position;
                CurrentMap.AddObject(thisTreeObject);

                return false;
            }
        }
        return true;
    }

    private void UpdateGhost(GameObject3D ghost)
    {
        //currentObject.Position = Engine.Renderer.Camera.Position;  ------>>>>> moves with the camera
        Ray3D ray3D = (Engine.Renderer.Camera as Camera3D).GetCameraMouseRay();
        Vector3 position = new Vector3(0, 0, 0);

        var enumerator = CurrentMap.GetObjectsByType<GroundTile>();
        while (enumerator.MoveNext())
        {
            var obj = enumerator.Current;
            if (ray3D.IntersectWithObject(obj, out Mesh _, out position, out Vector3 _, out int _))
            {
                ghost.Position = CurrentMap.SnapToGrid(position);
                if (CurrentMap.IsValidPosition(ghost))
                {
                    // change color
                    ghost.Tint = Color.White.SetAlpha(150);
                }
                else
                {
                    ghost.Tint = Color.Red.SetAlpha(150);
                }
            }

        }
    }





    public override void Update()
    {
        base.Update();
        UI.Update();
        if (ObjectTypeToPlace != null) UpdateGhost(GhostObject);
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

            if (GhostObject != null)
            {
                CurrentMap.RemoveObject(GhostObject);
            }

            GhostObject = (GameObject3D)Activator.CreateInstance(ObjectTypeToPlace);
            GhostObject.Tint = GhostObject.Tint.SetAlpha(150);
            CurrentMap.AddObject(GhostObject);


        };

        ui.AddChild(barContainer);
    }
}