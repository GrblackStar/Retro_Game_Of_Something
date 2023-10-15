using Emotion.Editor.EditorWindows.DataEditorUtil;

namespace Take_1.Data
{
    public class BuildingGameData : GameDataObject
    {
        public string Name { get; set; }

        public string ObjectClass { get; set; }

        // [GameDataReference<ResourceGameData>]
        public string[] Costs { get; set; }

        // [GameDataReference<NeedGameData>]
        public string[] Provides { get; set; }
    }
}
