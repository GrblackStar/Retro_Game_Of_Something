namespace Take_1
{
    public class TreeOak : BaseTakeOneObject
    {
        public TreeOak()
        {
            this.ShouldApplyToGrid = true;
            EntityPath = "NatureKit/tree_oak.fbx";
            Size3D = new System.Numerics.Vector3(10, 10, 10);
        }
    }
}
