using Emotion.Editor.EditorWindows.DataEditorUtil;

namespace Take_1.Data
{
    public class GameDataReferenceCost : GameDataReference<ResourceGameData>
    {
        public int Amount;
    }

    public class BuildingGameData : GameDataObject
    {
        public string Name { get; set; }

        public string ObjectClass { get; set; }

        public GameDataReferenceCost[] Costs { get; set; }

        public GameDataReference<NeedGameData>[] Provides { get; set; }
    }
}
