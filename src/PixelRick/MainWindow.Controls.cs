using System.Windows;
using System.Windows.Controls;
using Forms=System.Windows.Forms;
namespace PixelRick;
public partial class MainWindow
{
    private Forms.NotifyIcon? tray;
    private SettingsWindow? settingsWindow;
    private void SetupControls()
    {
        var menu=new ContextMenu();
        void Item(string label,Action action){var item=new MenuItem{Header=label};item.Click+=(_,_)=>action();menu.Items.Add(item);}
        Item("Stay Here",()=>SetWander(false));Item("Wander",()=>SetWander(true));Item("Sleep",Sleep);Item("Wake Up",Wake);Item("Teleport",Teleport);Item("Reset Position",()=>{Config.X=null;Config.Y=null;ResetPosition();engine.State.Enter(PetState.Idle);});
        var top=new MenuItem{Header="Always on Top",IsCheckable=true,IsChecked=Config.AlwaysOnTop};top.Click+=(_,_)=>{Config.AlwaysOnTop=top.IsChecked;settings.Save();};menu.Items.Add(top);menu.Opened+=(_,_)=>top.IsChecked=Config.AlwaysOnTop;
        menu.Items.Add(new Separator());Item("Settings",OpenSettings);Item("Exit",Close);Character.ContextMenu=menu;
        var trayMenu=new Forms.ContextMenuStrip();
        void TrayItem(string label,Action action)=>trayMenu.Items.Add(label,null,(_,_)=>Dispatcher.Invoke(action));
        TrayItem("Show",()=>{Show();KeepVisible();});TrayItem("Hide",Hide);TrayItem("Wander",()=>SetWander(true));TrayItem("Stay",()=>SetWander(false));TrayItem("Sleep",Sleep);TrayItem("Settings",OpenSettings);trayMenu.Items.Add(new Forms.ToolStripSeparator());TrayItem("Exit",Close);
        string icon=System.IO.Path.Combine(AppContext.BaseDirectory,"Assets/app.ico");
        tray=new Forms.NotifyIcon{Text="PixelRick · Right-click for controls",Icon=System.IO.File.Exists(icon)?new System.Drawing.Icon(icon):System.Drawing.SystemIcons.Application,ContextMenuStrip=trayMenu,Visible=true};
        tray.DoubleClick+=(_,_)=>Dispatcher.Invoke(()=>Show());
        Closed+=(_,_)=>{tray.Visible=false;tray.Dispose();settingsWindow?.Close();};
    }
    private void OpenSettings(){if(settingsWindow is not null){settingsWindow.Activate();return;}settingsWindow=new SettingsWindow(settings,()=>{KeepVisible();Draw();});settingsWindow.Closed+=(_,_)=>settingsWindow=null;settingsWindow.Show();}
    private void SetWander(bool wander){engine.SetWander(wander);settings.Save();}
    private void Sleep()=>engine.Sleep();
    private void Wake()=>engine.Wake();
    private void Teleport()=>engine.Teleport();
    private void PlaySound(){if(Config.Sound)System.Media.SystemSounds.Asterisk.Play();}
}
