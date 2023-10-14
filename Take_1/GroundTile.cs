namespace Take_1
{
    public class GroundTile : BaseTakeOneObject
    {
        public GroundTile()
        {
            this.ShouldApplyToGrid = true;
            EntityPath = "NatureKit/ground_grass.fbx";
            Size3D = new System.Numerics.Vector3(10, 10, 10);
        }
    }
}
