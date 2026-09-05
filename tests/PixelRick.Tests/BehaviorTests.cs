using Xunit;
namespace PixelRick.Tests;
public class BehaviorTests
{
    [Theory]
    [InlineData(-1920,0,0,1080)]
    [InlineData(0,0,1920,1040)]
    [InlineData(1920,-300,3840,1200)]
    public void ThrowAlwaysReturnsToWorkArea(double left,double top,double right,double bottom)
    {
        var area=new WorkArea(left,top,right,bottom);var m=new MovementController{X=left+200,Y=top+250};var p=new PhysicsController();p.Release(-900,-900,true);
        bool landed=false;for(int i=0;i<1000&&!landed;i++)landed=p.Update(m,1d/60,area,96,192);
        Assert.True(landed);Assert.InRange(m.X,left+96,right-96);Assert.Equal(bottom,m.Y);
    }
    [Fact] public void WalkerStopsAtValidDestination(){var a=new WorkArea(0,0,1920,1040);var m=new MovementController{X=300,Y=1040,Destination=3000};for(int i=0;i<2000;i++)m.Walk(.05,100,a,96);Assert.Equal(1824,m.X);}
    [Fact] public void StayNeverSchedulesWalk(){var b=new BehaviorScheduler(42);for(int i=0;i<10000;i++)Assert.NotEqual(PetState.Walk,b.Choose(false,0));}
    [Fact] public void IrritationRisesAndDecays(){var c=new InteractionController();var b=new BehaviorScheduler(42);for(int i=0;i<20;i++)c.Click(b);Assert.Equal(100,c.Irritation);c.Update(100);Assert.Equal(0,c.Irritation);}
    [Fact] public void InvalidSettingsRecover(){var s=new PetSettings{Scale=double.NaN,MovementSpeed=double.PositiveInfinity,X=double.NaN};s.Normalize();Assert.Equal(2,s.Scale);Assert.Equal(65,s.MovementSpeed);Assert.Null(s.X);}
    [Fact] public void StateTransitionsResetElapsed(){var s=new PetStateMachine();s.Update(10);s.Enter(PetState.Grabbed);Assert.Equal(0,s.Elapsed);Assert.Equal(PetState.Grabbed,s.State);}
    [Fact] public void CursorFacingUsesPhysicalCoordinates(){var m=new MovementController{X=-500};m.FaceToward(-600);Assert.True(m.FacingLeft);m.FaceToward(-300);Assert.False(m.FacingLeft);}
}
