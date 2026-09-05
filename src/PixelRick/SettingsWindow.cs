using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Reflection;
namespace PixelRick;
public sealed class SettingsWindow : Window
{
    public SettingsWindow(SettingsService service,Action applied)
    {
        Title="PixelRick · Settings";Width=430;Height=680;ResizeMode=ResizeMode.NoResize;WindowStartupLocation=WindowStartupLocation.CenterScreen;Background=new SolidColorBrush(Color.FromRgb(24,33,48));Foreground=Brushes.White;
        var root=new StackPanel{Margin=new Thickness(26)};Content=new ScrollViewer{Content=root,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
        root.Children.Add(new TextBlock{Text="A tiny genius. Your rules.",FontSize=24,FontWeight=FontWeights.SemiBold,Margin=new Thickness(0,0,0,6)});
        var version=Assembly.GetExecutingAssembly().GetName().Version!;
        root.Children.Add(new TextBlock{Text=$"PixelRick {version.Major}.{version.Minor}.{version.Build}",Foreground=Brushes.LightSkyBlue,Margin=new Thickness(0,0,0,20)});
        var c=service.Value;
        Slider AddSlider(string label,double min,double max,double val,double step){var text=new TextBlock{Margin=new Thickness(0,10,0,4)};var slider=new Slider{Minimum=min,Maximum=max,Value=val,TickFrequency=step,IsSnapToTickEnabled=true};void Update()=>text.Text=$"{label}  {slider.Value:0.##}";slider.ValueChanged+=(_,_)=>Update();Update();root.Children.Add(text);root.Children.Add(slider);return slider;}
        CheckBox AddCheck(string label,bool val){var check=new CheckBox{Content=label,IsChecked=val,Foreground=Brushes.White,Margin=new Thickness(0,9,0,0)};root.Children.Add(check);return check;}
        var scale=AddSlider("Size",1,4,c.Scale,.5);
        var speed=AddSlider("Walking speed",20,180,c.MovementSpeed,5);
        var activity=AddSlider("Activity",.25,2,c.ActivityFrequency,.25);
        var interactions=AddSlider("Cursor curiosity",0,1,c.InteractionFrequency,.1);
        var top=AddCheck("Always on top",c.AlwaysOnTop);var sound=AddCheck("Soft interaction sounds",c.Sound);
        var startup=AddCheck("Start with Windows",c.StartupWithWindows);var remember=AddCheck("Remember position",c.RememberPosition);
        var throwing=AddCheck("Allow playful throws",c.ThrowingPhysics);var multi=AddCheck("Use all monitors",c.MultiMonitor);
        var status=new TextBlock{TextWrapping=TextWrapping.Wrap,Foreground=Brushes.LightSalmon,Margin=new Thickness(0,10,0,0)};root.Children.Add(status);
        var save=new Button{Content="Save settings",Padding=new Thickness(16,10,16,10),Margin=new Thickness(0,14,0,0),Background=Brushes.LightSkyBlue,Foreground=Brushes.Black};root.Children.Add(save);
        save.Click+=(_,_)=>{try{service.SetStartup(startup.IsChecked==true);c.Scale=scale.Value;c.MovementSpeed=speed.Value;c.ActivityFrequency=activity.Value;c.InteractionFrequency=interactions.Value;c.AlwaysOnTop=top.IsChecked==true;c.Sound=sound.IsChecked==true;c.RememberPosition=remember.IsChecked==true;c.ThrowingPhysics=throwing.IsChecked==true;c.MultiMonitor=multi.IsChecked==true;c.Normalize();service.Save();applied();Close();}catch(Exception ex)when(ex is System.IO.IOException or UnauthorizedAccessException or System.Security.SecurityException){status.Text="Could not save settings: "+ex.Message;}};
        root.Children.Add(new TextBlock{Text="Drag to pick up · double-click to teleport\nRight-click for more controls\nUnofficial fan project. No chat or network connection.",Foreground=Brushes.LightSlateGray,FontSize=12,Margin=new Thickness(0,18,0,0),TextWrapping=TextWrapping.Wrap});
    }
}
