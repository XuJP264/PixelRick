namespace PixelRick;
public partial class MainWindow
{
    private bool sleepRequested;
    private double cursorCooldown,nextDemo;
    private int demoStep;
    private readonly EffectController effects;
    private void UpdateBehavior(double dt,double now)
    {
        effects.Update(dt);cursorCooldown-=dt;
        switch(state.State)
        {
            case PetState.Walk:
                if(movement.Walk(dt,Config.MovementSpeed,Area,48*Scale))state.Enter(PetState.Idle);
                break;
            case PetState.Falling:
                if(physics.Update(movement,dt,Area,48*Scale,96*Scale)){state.Enter(PetState.Landing);effects.Play("dust",.6);}
                break;
            case PetState.Sit:
                if(animator.Finished)state.Enter(sleepRequested?PetState.Sleep:PetState.SittingIdle);
                break;
            case PetState.Landing:
                if(animator.Finished){state.Enter(hardLanding?PetState.Dizzy:PetState.Poked);hardLanding=false;}
                break;
            case PetState.SittingIdle:
                if(state.Elapsed>10){state.Enter(sleepRequested||interaction.InactiveSeconds>150?PetState.Sleep:PetState.WakeUp);}
                break;
            case PetState.Sleep:
                if(effects.Current!="zzz")effects.Play("zzz",3600);
                break;
            case PetState.PortalEnter:
                if(state.Elapsed>1.4){var areas=screens.Areas(Config.MultiMonitor);var a=areas[behavior.Index(areas.Length)];movement.X=behavior.Between(a.Left+100*Scale,a.Right-100*Scale);movement.Y=a.Bottom;state.Enter(PetState.PortalExit);effects.Play("portal",1.4);}
                break;
            case PetState.PortalExit:
                if(state.Elapsed>1.4){effects.Clear();state.Enter(PetState.Idle);}
                break;
            case PetState.Idle:
                if(interaction.InactiveSeconds>180 && state.Elapsed>10 && behavior.Between(0,1)<dt*.07){sleepRequested=true;state.Enter(PetState.Sit);}
                else if(state.Elapsed>nextBehavior){nextBehavior=behavior.Delay(Config.ActivityFrequency);var choice=behavior.Choose(Config.Wander,interaction.Irritation);if(choice==PetState.Walk)movement.Destination=behavior.Between(Area.Left+48*Scale,Area.Right-48*Scale);state.Enter(choice);}
                break;
            case PetState.Grabbed:break;
            default:if(animator.Finished){effects.Clear();state.Enter(PetState.Idle);}break;
        }
        if(!pressed&&!IsPortal&&state.State==PetState.Idle&&cursorCooldown<=0)
        {
            var p=ScreenService.Cursor();double distance=Math.Sqrt(Math.Pow(p.X-movement.X,2)+Math.Pow(p.Y-(movement.Y-55*Scale),2));
            if(distance<140*Scale){cursorCooldown=behavior.Between(8,16);if(behavior.Between(0,1)<Config.InteractionFrequency){movement.FaceToward(p.X);state.Enter(distance<35*Scale?PetState.Poked:PetState.LookAround);effects.Play(distance<35*Scale?"exclamation":"question",1);}}
        }
        Character.Opacity=1;
        if(state.State==PetState.PortalEnter)Character.Opacity=state.Elapsed<.35?1:Math.Clamp(1-(state.Elapsed-.35)/.6,0,1);
        if(state.State==PetState.PortalExit)Character.Opacity=Math.Clamp((state.Elapsed-.2)/.7,0,1);
        EffectBehind.Source=effects.Current=="portal"?effects.Frame:null;
        EffectFront.Source=effects.Current!="portal"?effects.Frame:null;
        if(args.Contains("--demo")&&now>=nextDemo)
        {
            var sequence=new[]{PetState.Idle,PetState.Walk,PetState.Blink,PetState.LookAround,PetState.Angry,PetState.Smirk,PetState.Sit,PetState.Sleep,PetState.WakeUp,PetState.Grabbed,PetState.Falling,PetState.PortalEnter};
            var s=sequence[demoStep++%sequence.Length];nextDemo=now+(s==PetState.Sleep?4:2.5);
            if(s==PetState.PortalEnter)Teleport();else {state.Enter(s);if(s==PetState.Walk)movement.Destination=movement.X-150;if(s==PetState.Grabbed)movement.Y-=100;if(s==PetState.Falling)physics.Release(130,-100,true);if(s==PetState.Angry)effects.Play("anger",1);}
        }
    }
}
