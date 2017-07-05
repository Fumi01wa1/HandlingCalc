using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Runtime.InteropServices;
using System.Diagnostics;

namespace HandlingCalc
{
    public partial class Form1 : Form
    {
        public const int WM_LBUTTONDOWN = 0x201;
        public const int WM_LBUTTONUP = 0x202;
        public const int MK_LBUTTON = 0x0001;
        public static int GWL_STYLE = -16;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_CHAR = 0x0102;

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hWnd, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 電卓のトップウィンドウのウィンドウハンドル（※見つかることを前提としている）
            var mainWindowHandle = Process.GetProcessesByName("notepad")[0].MainWindowHandle;

            // 対象のボタンを探す
            var hWnd = FindTargetButton(GetWindow(mainWindowHandle));

            // マウスを押して放す
            SendMessage(hWnd, WM_LBUTTONDOWN, MK_LBUTTON, 0x000A000A);
            SendMessage(hWnd, WM_LBUTTONUP, 0x00000000, 0x000A000A);
            //SendMessage(hWnd, WM_KEYDOWN, 0x00000041, 0x00000001);
            //SendMessage(hWnd, WM_KEYUP, 0x00000041, 0x00000001);
            SendMessage(hWnd, WM_CHAR, 'a', 9);
        }

        // 全てのボタンを列挙し、その10番目のボタンのウィンドウハンドルを返す
        public static IntPtr FindTargetButton(Window top)
        {
            var all = GetAllChildWindows(top, new List<Window>());
            return all.Where(x => x.ClassName == "Edit").Skip(0).First().hWnd;
        }

        // 指定したウィンドウの全ての子孫ウィンドウを取得し、リストに追加する
        public static List<Window> GetAllChildWindows(Window parent, List<Window> dest)
        {
            dest.Add(parent);
            EnumChildWindows(parent.hWnd).ToList().ForEach(x => GetAllChildWindows(x, dest));
            return dest;
        }

        // 与えた親ウィンドウの直下にある子ウィンドウを列挙する（孫ウィンドウは見つけてくれない）
        public static IEnumerable<Window> EnumChildWindows(IntPtr hParentWindow)
        {
            IntPtr hWnd = IntPtr.Zero;
            while ((hWnd = FindWindowEx(hParentWindow, hWnd, null, null)) != IntPtr.Zero) { yield return GetWindow(hWnd); }
        }

        // ウィンドウハンドルを渡すと、ウィンドウテキスト（ラベルなど）、クラス、スタイルを取得してWindowsクラスに格納して返す
        public static Window GetWindow(IntPtr hWnd)
        {
            int textLen = GetWindowTextLength(hWnd);
            string windowText = null;
            if (0 < textLen)
            {
                //ウィンドウのタイトルを取得する
                StringBuilder windowTextBuffer = new StringBuilder(textLen + 1);
                GetWindowText(hWnd, windowTextBuffer, windowTextBuffer.Capacity);
                windowText = windowTextBuffer.ToString();
            }

            //ウィンドウのクラス名を取得する
            StringBuilder classNameBuffer = new StringBuilder(256);
            GetClassName(hWnd, classNameBuffer, classNameBuffer.Capacity);

            // スタイルを取得する
            int style = GetWindowLong(hWnd, GWL_STYLE);
            return new Window() { hWnd = hWnd, Title = windowText, ClassName = classNameBuffer.ToString(), Style = style };
        }
    }
}

public class Window
{
    public string ClassName;
    public string Title;
    public IntPtr hWnd;
    public int Style;
}
