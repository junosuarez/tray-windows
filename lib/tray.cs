using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class Startup
{
    private Func<object, Task<object>> eventHandler;
    private SysTrayApp app;

    public async Task<object> Invoke(dynamic input)
    {
        var name = (string)input.name;
        var items = ((object[])input.items).ToList().Cast<string>();

        this.eventHandler = (Func<object, Task<object>>)input.eventHandler;


        var i = new Input
        {
            Name = name,
            Items = items,
            TriggerEvent = this.SendEvent
        };

        try
        {
            var bytes = (byte[])input.icon;
            var source = Image.FromStream(new System.IO.MemoryStream(bytes));

            var target = new Bitmap(40, 40, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var g = Graphics.FromImage(target);
            g.DrawImage(source, 0, 0, 40, 40);
            i.Icon = Icon.FromHandle(target.GetHicon());

        }
        catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
        {
            // no icon specified
        }

        // for some reason, have to call with await first, then it works for app lifetime
        //await eventHandler(new { e = "start", data = false });
        Func<object, Task<object>> receive = (x) =>
        {
            ReceiveEvent("ev", x);
            return Task.FromResult<object>(true);
        };
        await eventHandler(new { e = "start", data =  receive});
        

        app = new SysTrayApp(i);
        Application.Run(app);
        SendEvent("stop", null);
        
        return 0;
    }

    public void SendEvent (string e, object data) {
        var message = new
            {
                e = e,
                data = data ?? false
            };

            eventHandler(message).ContinueWith(x =>
            {
                // required to invoke node fn
            });
    }

    public void ReceiveEvent(string name, dynamic data)
    {
        SendEvent("receive", new { name = name, data = data });
        app.Menu.MenuItems.Add(name);
    }


}

public class Input
{
    public string Name { get; set; }
    public IEnumerable<string> Items { get; set; }
    public Icon Icon { get; set; }
    public Action<string, object> TriggerEvent { get; set; }

    public Input()
    {

    }
}

public class SysTrayApp : Form
{

    private NotifyIcon trayIcon;
    private ContextMenu trayMenu;
    private Input input;

    public SysTrayApp(Input input)
    {
        this.input = input;
        // Create a simple tray menu with only one item.
        trayMenu = new ContextMenu();

        trayMenu.MenuItems.AddRange(input.Items.Select(i =>
        {
            return new MenuItem(i, OnMenuItemClick);
        }).ToArray());

        trayMenu.MenuItems.Add("x", (object s, EventArgs e) =>
        {
            Application.Exit();
        });

        trayIcon = new NotifyIcon();

        trayIcon.Text = input.Name;
        trayIcon.Icon = input.Icon ?? new Icon(SystemIcons.Application, 40, 40);

        // Add menu to tray icon and show it.
        trayIcon.ContextMenu = trayMenu;
        trayIcon.Visible = true;
        trayIcon.Click += OnIconClick;
    }


    private void OnIconClick(object sender, EventArgs e)
    {

        var show = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
        show.Invoke(trayIcon, null);
    }

    private void OnMenuItemClick(object sender, EventArgs e)
    {
        var item = (MenuItem)sender;
        var data = new
        {
            index = item.Index,
            text = item.Text
        };

        this.input.TriggerEvent("click:menuItem", data);
    }

    protected override void OnLoad(EventArgs e)
    {
        Visible = false; // Hide form window.
        ShowInTaskbar = false; // Remove from taskbar.

        base.OnLoad(e);
    }

    private void OnExit(object sender, EventArgs e)
    {
        Application.Exit();
    }

    protected override void Dispose(bool isDisposing)
    {
        if (isDisposing)
        {
            // Release the icon resource.
            trayIcon.Dispose();
        }

        base.Dispose(isDisposing);
    }
}
