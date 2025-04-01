using System;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AutoClicker
{
    public partial class Form1 : Form
    {
        private Thread clickThread;
        private bool clicking = false;
        private IntPtr targetWindowHandle = IntPtr.Zero;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            NativeMethods.RegisterHotKey(Handle, HotkeyConstants.HOTKEY_ID, HotkeyConstants.MOD_NONE, HotkeyConstants.VK_F8);
            LoadOpenWindows();
        }

        private void LoadOpenWindows()
        {
            windowComboBox.Items.Clear();
            NativeMethods.EnumWindows((hWnd, lParam) =>
            {
                AddWindowToComboBox(hWnd);
                NativeMethods.EnumChildWindows(hWnd, (childHWnd, childLParam) =>
                {
                    AddWindowToComboBox(childHWnd, hWnd);
                    return true;
                }, IntPtr.Zero);
                return true;
            }, IntPtr.Zero);

            if (windowComboBox.Items.Count > 0)
            {
                windowComboBox.SelectedIndex = 0;
            }
        }

        private void AddWindowToComboBox(IntPtr hWnd, IntPtr parentHWnd = default)
        {
            int length = NativeMethods.GetWindowTextLength(hWnd);
            if (length == 0) return;

            StringBuilder builder = new StringBuilder(length);
            NativeMethods.GetWindowText(hWnd, builder, length + 1);

            string title = builder.ToString();
            if (parentHWnd != default)
            {
                int parentLength = NativeMethods.GetWindowTextLength(parentHWnd);
                StringBuilder parentBuilder = new StringBuilder(parentLength);
                NativeMethods.GetWindowText(parentHWnd, parentBuilder, parentLength + 1);
                title = $"{parentBuilder} - {title}";
            }

            windowComboBox.Items.Add(new WindowItem { Handle = hWnd, Title = title });
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            if (!clicking && windowComboBox.SelectedItem != null)
            {
                clicking = true;
                targetWindowHandle = ((WindowItem)windowComboBox.SelectedItem).Handle;
                clickThread = new Thread(AutoClick)
                {
                    IsBackground = true
                };
                clickThread.Start();
            }
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            clicking = false;
            if (clickThread != null && clickThread.IsAlive)
            {
                clickThread.Join();
            }
        }

        private void AutoClick()
        {
            NativeMethods.SetForegroundWindow(targetWindowHandle);
            Thread.Sleep(1000);

            while (clicking)
            {
                NativeMethods.mouse_event(HotkeyConstants.MOUSEEVENTF_LEFTDOWN | HotkeyConstants.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                Thread.Sleep((int)intervalUpDown.Value);
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HotkeyConstants.HOTKEY_ID)
            {
                if (clicking)
                {
                    stopButton_Click(null, null);
                }
                else
                {
                    startButton_Click(null, null);
                }
            }
            base.WndProc(ref m);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            NativeMethods.UnregisterHotKey(Handle, HotkeyConstants.HOTKEY_ID);
            base.OnFormClosing(e);
        }
    }
}
