namespace PixelRick;
public sealed class MovementController
{
    public double X {get;set;}
    public double Y {get;set;}
    public double Destination {get;set;}
    public bool FacingLeft {get;private set;}
    public void FaceToward(double x)=>FacingLeft=x<X;
    public bool Walk(double dt,double speed,WorkArea area,double halfWidth)
    {
        Destination=Math.Clamp(Destination,area.Left+halfWidth,area.Right-halfWidth);
        double delta=Destination-X;FacingLeft=delta<0;
        X+=Math.Sign(delta)*Math.Min(Math.Abs(delta),speed*dt);
        Y=area.Bottom;
        return Math.Abs(Destination-X)<1;
    }
    public void Clamp(WorkArea area,double halfWidth,double height)
    { X=Math.Clamp(X,area.Left+halfWidth,area.Right-halfWidth);Y=Math.Clamp(Y,area.Top+height,area.Bottom); }
}
