using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
namespace PixelRick;
public partial class MainWindow : Window
{
    private readonly SettingsService settings=new();
    private readonly ScreenService screens=new();
    private readonly MovementController movement=new();
    private readonly PhysicsController physics=new();
    private readonly PetStateMachine state=new();
    private readonly BehaviorScheduler behavior=new();
    private readonly InteractionController interaction=new();
    private readonly SpriteAnimator animator;
    private readonly DispatcherTimer timer=new(){Interval=TimeSpan.FromMilliseconds(1000d/30)};
    private readonly Stopwatch clock=Stopwatch.StartNew();
    private double last,nextBehavior=5;
    private bool pressed,dragging;
    private (double X,double Y) press,lastCursor;
    private double offsetX,offsetY,vx,vy;
    private double lastMoveTime,pendingClick=-1;
    private bool captured;
    private bool hardLanding;
    private IntPtr hwnd;
    private readonly string[] args;
    private PetSettings Config=>settings.Value;
    private double Scale=>Config.Scale;
    public MainWindow(string[] args)
    {
        this.args=args;InitializeComponent();Diagnostics.Enabled=args.Contains("--diagnostics")||args.Contains("--smoke-test");
        var definition=new CharacterDefinition();animator=new(definition);effects=new(definition);
        SetupControls();
        state.Changed+=s=>animator.Play(PetStateMachine.Animation(s));
        SourceInitialized+=(_,_)=>{hwnd=new WindowInteropHelper(this).Handle;HwndSource.FromHwnd(hwnd).AddHook(Hook);ResetPosition();};
        MouseLeftButtonDown+=OnDown;MouseMove+=OnMove;MouseLeftButtonUp+=OnUp;
        LostMouseCapture+=(_,_)=>{if(pressed){pressed=false;if(dragging){dragging=false;physics.Release(0,0,false);state.Enter(PetState.Falling);}}};
        timer.Tick+=(_,_)=>Tick();Loaded+=(_,_)=>timer.Start();
        Closed+=(_,_)=>{timer.Stop();Config.X=movement.X;Config.Y=movement.Y;settings.Save();System.Windows.Application.Current.Shutdown();};
    }
    private IntPtr Hook(IntPtr h,int msg,IntPtr w,IntPtr l,ref bool handled)
    {
        if(msg==NativeMethods.ShowMessage){Show();handled=true;}
        if(msg==0x84){var p=PointFromScreen(new System.Windows.Point((short)(l.ToInt64()&0xffff),(short)((l.ToInt64()>>16)&0xffff)));int x=(int)(p.X/ActualWidth*128),y=(int)(p.Y/ActualHeight*128);if(x>=0&&x<128&&y>=0&&y<128){if(movement.FacingLeft&&state.State is PetState.Walk or PetState.LookAround)x=127-x;byte[] pixel=new byte[4];animator.Frame.CopyPixels(new Int32Rect(x,y,1,1),pixel,4,0);if(pixel[3]==0){handled=true;return new IntPtr(-1);}}}
        return IntPtr.Zero;
    }
    private void ResetPosition()
    {var a=screens.Areas(Config.MultiMonitor)[0];movement.X=Config.RememberPosition&&Config.X.HasValue?Config.X.Value:a.Right-180;movement.Y=Config.RememberPosition&&Config.Y.HasValue?Config.Y.Value:a.Bottom;KeepVisible();if(movement.Y<Area.Bottom)state.Enter(PetState.Falling);Draw();}
    private WorkArea Area=>screens.Nearest(movement.X,movement.Y-1,Config.MultiMonitor);
    private void KeepVisible()=>movement.Clamp(Area,48*Scale,96*Scale);
    private void Draw()
    {
        Topmost=Config.AlwaysOnTop;
        NativeMethods.SetWindowPos(hwnd,IntPtr.Zero,(int)Math.Round(movement.X-64*Scale),(int)Math.Round(movement.Y-120*Scale),(int)(128*Scale),(int)(128*Scale),0x0014);
        Character.Source=animator.Frame;Character.RenderTransform=new ScaleTransform(movement.FacingLeft&&state.State is PetState.Walk or PetState.LookAround?-1:1,1);
    }
    private void Tick()
    {
        double now=clock.Elapsed.TotalSeconds,dt=Math.Clamp(now-last,0,.05);last=now;animator.Update(dt);state.Update(dt);interaction.Update(dt);
        if(!IsVisible)return;
        if(pendingClick>=0&&now>=pendingClick&&!pressed){pendingClick=-1;if(!IsPortal){state.Enter(interaction.Click(behavior));effects.Play(state.State==PetState.Angry?"anger":"impact",.6);PlaySound();}}
        UpdateBehavior(dt,now);
        if(!dragging)KeepVisible();Draw();
        if(!captured&&now>2&&args.Contains("--capture")){int i=Array.IndexOf(args,"--capture");if(i+1<args.Length){var bitmap=new System.Windows.Media.Imaging.RenderTargetBitmap((int)ActualWidth,(int)ActualHeight,96,96,PixelFormats.Pbgra32);bitmap.Render(this);var encoder=new System.Windows.Media.Imaging.PngBitmapEncoder();encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));using var stream=System.IO.File.Create(args[i+1]);encoder.Save(stream);}captured=true;}
        if(args.Contains("--smoke-test")&&now>8){Diagnostics.Log("SMOKE PASS");Close();}
    }
    private void OnDown(object sender,MouseButtonEventArgs e)
    {if(IsPortal)return;if(e.ClickCount==2){pendingClick=-1;Teleport();e.Handled=true;return;}Wake();pressed=true;press=lastCursor=ScreenService.Cursor();offsetX=movement.X-press.X;offsetY=movement.Y-press.Y;vx=vy=0;lastMoveTime=clock.Elapsed.TotalSeconds;interaction.Touch();Character.CaptureMouse();e.Handled=true;}
    private void OnMove(object sender,System.Windows.Input.MouseEventArgs e)
    {if(!pressed)return;var p=ScreenService.Cursor();double now=clock.Elapsed.TotalSeconds,dt=Math.Max(.008,now-lastMoveTime);if(!dragging&&Math.Abs(p.X-press.X)+Math.Abs(p.Y-press.Y)>6){pendingClick=-1;dragging=true;state.Enter(PetState.Grabbed);}if(dragging){vx=.4*vx+.6*(p.X-lastCursor.X)/dt;vy=.4*vy+.6*(p.Y-lastCursor.Y)/dt;movement.X=p.X+offsetX;movement.Y=p.Y+offsetY;Draw();}lastCursor=p;lastMoveTime=now;}
    private void OnUp(object sender,MouseButtonEventArgs e)
    {if(!pressed)return;pressed=false;Character.ReleaseMouseCapture();if(dragging){dragging=false;if(clock.Elapsed.TotalSeconds-lastMoveTime>.12)vx=vy=0;hardLanding=Config.ThrowingPhysics&&(Math.Abs(vx)>500||Math.Abs(vy)>500);physics.Release(vx,vy,Config.ThrowingPhysics);state.Enter(PetState.Falling);}else pendingClick=clock.Elapsed.TotalSeconds+System.Windows.Forms.SystemInformation.DoubleClickTime/1000d;}
}
