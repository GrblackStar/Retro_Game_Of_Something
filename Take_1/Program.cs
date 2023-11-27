#region Using

using System.Numerics;
using Emotion.Audio;
using Emotion.Common;
using Emotion.Editor.EditorWindows.DataEditorUtil;
using Emotion.Game.World;
using Emotion.Game.World2D;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Game.World3D;
using Emotion.Game.World3D.Objects;
using Emotion.Game.World3D.SceneControl;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Standard.XML;
using Emotion.Testing;
using Emotion.UI;
using Take_1;
using Take_1.Data;

#endregion
namespace Emotion.ExecTest;

public class Program
{
    private static void Main(string[] args)
    {
        var config = new Configurator
        {
            DebugMode = true,
            MasterVolume = 0f
        };

        Engine.Setup(config);

        Engine.SceneManager.SetScene(new TakeOneGame());

        Engine.Run();
    }
}


public class TakeOneGame : World3DBaseScene<Take1Map>
{
    public UIController? UI;
    public Type? ObjectTypeToPlace;
    public BaseTakeOneObject? GhostObject;
    public List<Quad3D>? QuadObjects;

    public override async Task LoadAsync()
    {
        Engine.Audio.CreateLayer("FX");

        var cam3D = new Camera3D(new Vector3(100));
        cam3D.LookAtPoint(Vector3.Zero);
        cam3D.Position = new Vector3(922.2831f, 344.7375f, 391);
        cam3D.LookAt = new Vector3(-0.677202f, 0.04379144f, -0.7344929f);
        Engine.Renderer.Camera = cam3D;

        var mapAsset = Engine.AssetLoader.Get<XMLAsset<Take1Map>>("Maps/sofia.xml");
        Take1Map map;
        if (mapAsset?.Content == null)
            map = new Take1Map(new Vector2(10, 10));
        else
            map = mapAsset.Content;

        await ChangeMapAsync(map);

        UI = new UIController();
        AddPlaceableObjectMenu();

        Engine.Host.OnKey.AddListener(KeyHandler, KeyListenerType.Game);

        QuadObjects = new List<Quad3D>();
    }

    public override void Draw(RenderComposer composer)
    {
        composer.SetUseViewMatrix(false);
        composer.RenderSprite(Vector3.Zero, composer.CurrentTarget.Size, Color.CornflowerBlue);
        composer.ClearDepth();
        composer.SetUseViewMatrix(true);

        base.Draw(composer);

        composer.SetUseViewMatrix(false);
        composer.ClearDepth();
        UI.Render(composer);
    }

    private bool KeyHandler(Key key, KeyStatus status)
    {
        if (key == Key.MouseKeyLeft && ObjectTypeToPlace != null && status == KeyStatus.Up)
        {
            var fxLayer = Engine.Audio.GetLayer("FX");

            if (GhostObject != null && !GhostObject.CanPlaceObject)
            {
                var cantPlaceAudio = Engine.AssetLoader.Get<AudioAsset>("sound_fx/197566__username12125__nuh-uh.wav");
                fxLayer.QuickPlay(cantPlaceAudio);

                return false;
            }

            var thisTreeObject = (GameObject3D)Activator.CreateInstance(ObjectTypeToPlace);
            thisTreeObject.Position = GhostObject.Position;
            thisTreeObject.Rotation = GhostObject.Rotation;
            CurrentMap.AddObject(thisTreeObject);

            var buildingBarUI = UI.GetWindowById("BuildingsBar") as UICallbackListNavigator;
            buildingBarUI.ResetSelection(true);
            buildingBarUI.OnChoiceConfirmed?.Invoke(null, -1);

            var placeAudio = Engine.AssetLoader.Get<AudioAsset>("sound_fx/390355__josethehedgehog__deslizamiento-bajo-tronco.wav");
            fxLayer.QuickPlay(placeAudio);

            return false;
        }

        return true;
    }

    private void UpdateGhost(GameObject3D ghost)
    {
        if (CurrentMap.EditorMode) return;

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
                    GhostObject.CanPlaceObject = true;
                }
                else
                {
                    ghost.Tint = Color.Red.SetAlpha(255);
                    GhostObject.CanPlaceObject = false;
                }

                //if (CurrentMap.PlayAreaCenter != null)
                //{
                //    ghost.RotateZToFacePoint(CurrentMap.PlayAreaCenter.Position);
                //    var deg = ghost.RotationDeg.Z;
                //    var snapped = MathF.Round(deg / 90f) * 90f;
                //    ghost.RotationDeg = new Vector3(0, 0, snapped);
                //}

                //break;
            }

        }
        UpdatePlanesPosition(ghost);
    }

    private void UpdatePlanesPosition(GameObject3D gameObject)
    {
        if (QuadObjects.Count == 0) return;

        int i = 0;
        CurrentMap.ProcessGridTiles(gameObject, (x, y) =>
        {
            var myquad = QuadObjects[i];
            myquad.Position = CurrentMap.GridToWorld(new Vector2(x, y));
            myquad.Z = 5.1f;

            if (CurrentMap.IsValidPosition(myquad))
                myquad.Tint = Color.PrettyBlue;
            else
                myquad.Tint = Color.PrettyRed.SetAlpha(230);

            i++;
        }, false);
    }

    public void AddPlanesToList(GameObject3D gameObject)
    {
        CurrentMap.ProcessGridTiles(gameObject, (x, y) =>
        {
            Quad3D quadPiece = new Quad3D();
            quadPiece.Width = 25;
            quadPiece.Depth = 25;
            quadPiece.Position = CurrentMap.GridToWorld(new Vector2(x, y));
            quadPiece.Z = 5.1f;

            QuadObjects.Add(quadPiece);
        }, false);
    }

    public void CreatePlanesForGhost()
    {
        QuadObjects?.Clear();
        AddPlanesToList(GhostObject);
        foreach (var item in QuadObjects)
        {
            CurrentMap.AddObject(item);
        }
    }

    public void RemovePlanesAndGhost()
    {
        CurrentMap.RemoveObject(GhostObject);
        foreach (var item in QuadObjects)
        {
            CurrentMap.RemoveObject(item);
        }
        QuadObjects.Clear();
    }

    public override void Update()
    {
        base.Update();
        UI.Update();
        if (ObjectTypeToPlace != null) UpdateGhost(GhostObject);
    }

    protected void AddPlaceableObjectMenu()
    {
        var barContainer = new UISolidColor();
        barContainer.StretchX = true;
        barContainer.StretchY = true;
        barContainer.WindowColor = Color.Black * 0.75f;
        barContainer.Paddings = new Rectangle(2, 2, 2, 2);
        barContainer.Anchor = UIAnchor.BottomCenter;
        barContainer.ParentAnchor = UIAnchor.BottomCenter;
        barContainer.Margins = new Rectangle(0, 0, 0, 5);
        barContainer.MinSize = new Vector2(32, 32);

        var bar = new UICallbackListNavigator();
        bar.StretchX = true;
        bar.StretchY = true;
        bar.IgnoreParentColor = true;
        bar.LayoutMode = LayoutMode.HorizontalList;
        bar.Id = "BuildingsBar";
        barContainer.AddChild(bar);

        var buildings = GameDataDatabase.GetObjectsOfType<BuildingGameData>() ?? Array.Empty<BuildingGameData>();
        for (int i = 0; i < buildings.Length; i++)
        {
            var building = buildings[i];
            var type = XMLHelpers.GetTypeByNameWithTypeHint(typeof(BaseTakeOneObject), building.ObjectClass);
            if (type == null) continue;

            UIObject3DWindow thisObjectWin = new UIObject3DWindow(type);
            thisObjectWin.MaxSize = new Vector2(32);
            thisObjectWin.MinSize = new Vector2(32);

            bar.AddChild(thisObjectWin);
        }
        bar.SetupMouseSelection();

        UIObject3DWindow? oldSel = null;
        bar.OnChoiceConfirmed = (nu, _) =>
        {
            if (oldSel != null) oldSel.Selected = false;

            if (GhostObject != null) RemovePlanesAndGhost();

            var nuAs = nu as UIObject3DWindow;
            ObjectTypeToPlace = nuAs?.Type;
            oldSel = nuAs;
            if (nuAs == null) return;

            nuAs.Selected = true;

            GhostObject = Activator.CreateInstance(ObjectTypeToPlace) as BaseTakeOneObject;
            GhostObject.ShouldApplyToGrid = false;
            GhostObject.Tint = GhostObject.Tint.SetAlpha(150);
            GhostObject.ObjectFlags = ObjectFlags.Map3DDontThrowShadow;
            GhostObject.Position = CurrentMap.SnapToGrid(Vector3.Zero);
            CurrentMap.AddObject(GhostObject);
            CreatePlanesForGhost();
            UpdateGhost(GhostObject);
        };

        UI.AddChild(barContainer);
    }
}