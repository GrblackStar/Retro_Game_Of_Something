using Emotion.Game.World3D.Objects;

namespace Take_1
{
    public class BaseTakeOneObject : GenericObject3D
    {
        public bool ShouldApplyToGrid = false;
        public bool CanPlaceObject = true;

        public BaseTakeOneObject() { }

        // !!!!!!!!!!!! INIT IS CALLED WHEN THE OBJECT AS ADDED TO THE MAP
        public override void Init()
        {
            base.Init();
            if (this.ShouldApplyToGrid)
            {
                var t1Map = (Map as Take1Map);
                t1Map?.ApplyObjectToGrid(this);
            }
        }

    }
}
