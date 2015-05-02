using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class Startup
{
    private Func<object, Task<object>> eventHandler;
    private Func<object, Task<object>> checkMessage;
    private SysTrayApp app;

    public async Task<object> Invoke(dynamic input)
    {
        var name = (string)input.name;
        var items = ((object[])input.items).ToList().Cast<string>();

        this.eventHandler = (Func<object, Task<object>>)input.eventHandler;
        this.checkMessage = (Func<object, Task<object>>)input.checkMessage;

        
        
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
        var t = new System.Timers.Timer(20);
        t.AutoReset = true;
        t.Elapsed += onTick;
        t.Start();
        Application.Run(app);
        SendEvent("stop", null);
        
        return 0;
    }

    void onTick(object sender, EventArgs t)
    {
        Check();
    }

    private async void Check()
    {
        var m = await checkMessage(null);
        var message = (dynamic)m;
        try
        {
            var e = (string)message.e;
            var data = (object)message.data;
            ReceiveEvent(e, data);
        }
        catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
        {
            // no message
        }
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
        switch (name)
        {
            case "add:menuItem":
                addMenuItem((string) data);
                break;
            case "del:menuItem":
                delMenuItem((string)data);
                break;
            case "del:menuItem:at":
                delMenuItemAt((int)data);
                break;
            case "exit":
                Application.Exit();
                break;
            case "drop:menu":
                app.trayMenu.MenuItems.Clear();
                break;
            case "updateIcon":
                updateIcon((byte[])data);
                break;
        }
    }

    private void updateIcon(byte[] data)
    {
        var source = Image.FromStream(new System.IO.MemoryStream(data));

        var target = new Bitmap(40, 40, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        var g = Graphics.FromImage(target);
        g.DrawImage(source, 0, 0, 40, 40);
        var icon = Icon.FromHandle(target.GetHicon());

        app.trayIcon.Icon = icon;
    }

    private void addMenuItem(string item)
    {
        app.trayMenu.MenuItems.Add(app.CreateMenuItem(item));
    }

    private void delMenuItem(string item)
    {
        var menuItem = Tools.ToList<MenuItem>(app.trayMenu.MenuItems).Where(i => i.Text == item).FirstOrDefault();
        if (menuItem != null)
        {
            app.trayMenu.MenuItems.Remove(menuItem);
        }
       
    }
    private void delMenuItemAt(int index)
    {
        app.trayMenu.MenuItems.RemoveAt(index);
    }


}

public static class Tools
{
    public static List<T> ToList<T>(this IEnumerable source) where T : class
    {
        var list = new List<T>();
        var e = source.GetEnumerator();
        foreach (var i in source) {
            list.Add(i as T);
        }
        return list;
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

    public NotifyIcon trayIcon;
    public ContextMenu trayMenu;
    private Input input;

    public SysTrayApp(Input input)
    {
        this.input = input;
        // Create a simple tray menu with only one item.
        trayMenu = new ContextMenu();

        trayMenu.MenuItems.AddRange(input.Items.Select(CreateMenuItem).ToArray());

        trayIcon = new NotifyIcon();

        trayIcon.Text = input.Name;
        trayIcon.Icon = input.Icon ?? new Icon(SystemIcons.Application, 40, 40);

        // Add menu to tray icon and show it.
        trayIcon.ContextMenu = trayMenu;
        trayIcon.Visible = true;
        trayIcon.Click += OnIconClick;
    }

    public MenuItem CreateMenuItem(string text)
    {
        return new MenuItem(text, OnMenuItemClick);
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
