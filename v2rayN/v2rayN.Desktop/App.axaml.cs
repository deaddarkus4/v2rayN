using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.Timers;
using v2rayN.Desktop.ViewModels;
using v2rayN.Desktop.Views;

namespace v2rayN.Desktop;

public partial class App : Application
{
    //public static EventWaitHandle ProgramStarted;

    private static System.Timers.Timer updateTimer;

    public static void UpdateRuBlockList()
    {
        Logging.SaveLog("Start to update RU Block list..");

        var localSrss = Utils.GetBinPath("srss");

        Directory.CreateDirectory(localSrss);

        using (var client = new HttpClient())
        {
            using (var s = client.GetStreamAsync("https://github.com/deaddarkus4/ru-block-sing_box-rules/releases/latest/download/geoip-ru-block.srs"))
            {
                
                using (var fs = new FileStream($"{localSrss}/geoip-ru-block.srs", FileMode.OpenOrCreate))
                {
                    s.Result.CopyTo(fs);
                }
            }

            using (var s = client.GetStreamAsync("https://github.com/deaddarkus4/ru-block-sing_box-rules/releases/latest/download/geosite-ru-block.srs"))
            {
                using (var fs = new FileStream($"{localSrss}/geosite-ru-block.srs", FileMode.OpenOrCreate))
                {
                    s.Result.CopyTo(fs);
                }
            }
        }
        Logging.SaveLog("RU Block list updated.");
    }

    private static void OnTimedUpdateEvent(Object source, System.Timers.ElapsedEventArgs e)
    {

        UpdateRuBlockList();
    }

    public override void Initialize()
    {
        if (!AppHandler.Instance.InitApp())
        {
            Environment.Exit(0);
            return;
        }
        UpdateRuBlockList();

        updateTimer = new System.Timers.Timer();
        updateTimer.Interval = 1000 * 60 * 60 * 4;
        updateTimer.Elapsed += OnTimedUpdateEvent;
        updateTimer.AutoReset = true;
        updateTimer.Enabled = true;

        AvaloniaXamlLoader.Load(this);

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

        this.DataContext = new AppViewModel();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            OnStartup(desktop.Args);

            desktop.Exit += OnExit;
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnStartup(string[]? Args)
    {
        var exePathKey = Utils.GetMD5(Utils.GetExePath());

        var rebootas = (Args ?? new string[] { }).Any(t => t == Global.RebootAs);
        //ProgramStarted = new EventWaitHandle(false, EventResetMode.AutoReset, exePathKey, out bool bCreatedNew);
        //if (!rebootas && !bCreatedNew)
        //{
        //    ProgramStarted.Set();
        //    Environment.Exit(0);
        //    return;
        //}

        AppHandler.Instance.InitComponents();
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject != null)
        {
            Logging.SaveLog("CurrentDomain_UnhandledException", (Exception)e.ExceptionObject!);
        }
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Logging.SaveLog("TaskScheduler_UnobservedTaskException", e.Exception);
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
    }

    private void TrayIcon_Clicked(object? sender, EventArgs e)
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (desktop.MainWindow.IsVisible)
            {
                desktop.MainWindow?.Hide();
            }
            else
            {
                desktop.MainWindow?.Show();
            }
        }
    }
}