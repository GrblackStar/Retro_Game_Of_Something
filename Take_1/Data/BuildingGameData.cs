using Emotion.Editor.EditorWindows.DataEditorUtil;

namespace Take_1.Data
{
    public class BuildingGameData : GameDataObject
    {
        public string Name { get; set; }

        public string ObjectClass { get; set; }

        public GameDataReference<ResourceGameData>[] Costs { get; set; }

        public GameDataReference<NeedGameData>[] Provides { get; set; }
    }
}
