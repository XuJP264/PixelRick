namespace PixelRick;
public sealed class PhysicsController
{
    public double Vx {get;private set;}
    public double Vy {get;private set;}
    public void Release(double vx,double vy,bool throwing){Vx=throwing?Math.Clamp(vx,-1200,1200):0;Vy=throwing?Math.Clamp(vy,-1000,1000):0;}
    public bool Update(MovementController m,double dt,WorkArea a,double halfWidth,double height)
    {
        Vy+=1800*dt;m.X+=Vx*dt;m.Y+=Vy*dt;
        if(m.X<a.Left+halfWidth || m.X>a.Right-halfWidth){m.X=Math.Clamp(m.X,a.Left+halfWidth,a.Right-halfWidth);Vx*=-.35;}
        if(m.Y<a.Top+height){m.Y=a.Top+height;Vy=Math.Max(0,Vy);}
        if(m.Y>=a.Bottom){m.Y=a.Bottom;Vx=0;Vy=0;return true;}
        return false;
    }
}
