#region Using

using System.Numerics;
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
    public Type? QuadObjectsForType;
    public List<Quad3D>? QuadObjects;

    public override async Task LoadAsync()
    {
        var cam3D = new Camera3D(new Vector3(100));
        cam3D.LookAtPoint(Vector3.Zero);
        Engine.Renderer.Camera = cam3D;

        await ChangeMapAsync(new Take1Map(new Vector2(10, 10)));

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
        UI.Render(composer);
    }

    private bool KeyHandler(Key key, KeyStatus status)
    {
        if (GhostObject != null && !GhostObject.CanPlaceObject) return true;

        if (key == Key.MouseKeyLeft && ObjectTypeToPlace != null)
        {
            if (status == KeyStatus.Down)
            {
                var thisTreeObject = (GameObject3D)Activator.CreateInstance(ObjectTypeToPlace);
                thisTreeObject.Position = GhostObject.Position;
                CurrentMap.AddObject(thisTreeObject);

                ObjectTypeToPlace = null;
                QuadObjectsForType = null;
                RemovePlanesAndGhost();

                var buildingBarUI = UI.GetWindowById("BuildingsBar") as UICallbackListNavigator;
                buildingBarUI.ResetSelection(true);
                buildingBarUI.OnChoiceConfirmed?.Invoke(null, -1);

                return false;
            }
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
    
    // check if the ghostObject is loded and it has bounds
    public void UpdatePlanesForGhost()
    {
        var ghostObjectType = GhostObject?.GetType();
        if (ghostObjectType != QuadObjectsForType && GhostObject.ObjectState == ObjectState.Alive)
        {
            QuadObjects?.Clear();
            AddPlanesToList(GhostObject);
            foreach (var item in QuadObjects)
            {
                CurrentMap.AddObject(item);
            }
            QuadObjectsForType = ghostObjectType;
        }
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
        });
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
        UpdatePlanesForGhost();

        // if we have a ghost object, initialise the Quad3Ds. find it in the currentmap
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

            var nuAs = nu as UIObject3DWindow;
            if (nuAs == null) return;

            nuAs.Selected = true;
            ObjectTypeToPlace = nuAs.Type;
            oldSel = nuAs;

            if (GhostObject != null) RemovePlanesAndGhost();
            
            GhostObject = Activator.CreateInstance(ObjectTypeToPlace) as BaseTakeOneObject;
            GhostObject.ShouldApplyToGrid = false;
            GhostObject.Tint = GhostObject.Tint.SetAlpha(150);
            GhostObject.ObjectFlags = ObjectFlags.Map3DDontThrowShadow;
            CurrentMap.AddObject(GhostObject);
        };

        UI.AddChild(barContainer);
    }
}