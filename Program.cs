using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace ImeTray {
    static class Program {
        static RegistryKey Subkey { get; }
        const string RegistryName = "Keyboard Layout Setting.New Phonetic";
        static NotifyIcon KeyboardLayout { get; } = new NotifyIcon();

        static bool IsBoPoMoFo { get; set; } = false;
        const string BoBoMoFoRegistryValue = "0x00020010";
        static bool IsPinyin { get; set; } = false;
        const string PinyinRegistryValue = "0x00100022";

        static class IconEnum {
            static string IconFont { get; } = "Segoe MDL2 Assets";
            public static Icon ChineseBoPoMoFo { get; } = CreateCharIcon(0xE989, IconFont);
            public static Icon ChinesePinyin { get; } = CreateCharIcon(0xE8C1, IconFont);
            public static Icon Unknown { get; } = CreateCharIcon(0xE844, IconFont);
        }

        static Program() {
            Subkey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\IME\\15.0\\IMETC", true);
        }


        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);


            //KeyboardLayout.Icon = IconEnum.ChineseBoPoMoFo;
            KeyboardLayout.Visible = true;
            KeyboardLayout.Click += KeyboardLayout_Click;

            var currentUser = WindowsIdentity.GetCurrent();

            var query = new WqlEventQuery(
                 //"SELECT * FROM RegistryValueChangeEvent "
                 "SELECT * FROM RegistryValueChangeEvent "
                 + "WHERE Hive = 'HKEY_USERS' "
                 + @$"AND KeyPath = '{currentUser.Owner.Value}\\SOFTWARE\\Microsoft\\IME\\15.0\\IMETC' AND ValueName='Keyboard Layout Setting.New Phonetic'"
            );


            var watcher = new ManagementEventWatcher(query);
            watcher.EventArrived += delegate { Render(); };
            watcher.Start();

            Render();

            Application.Run();
        }

        private static void KeyboardLayout_Click(object sender, EventArgs e) {
            if (IsBoPoMoFo) {
                IsBoPoMoFo = false;
                IsPinyin = true;
                Subkey.SetValue(RegistryName, PinyinRegistryValue);
                return;
            }

            IsBoPoMoFo = true;
            IsPinyin = false ;
            Subkey.SetValue(RegistryName, BoBoMoFoRegistryValue);
        }

        static void Render() {
            var setting = Subkey.GetValue(RegistryName) as string;

            IsBoPoMoFo = false;
            IsPinyin = false;

            if (setting is null) return;
            if (setting == BoBoMoFoRegistryValue) {
                IsBoPoMoFo = true;
                KeyboardLayout.Icon = IconEnum.ChineseBoPoMoFo;
                return;

            }
            if (setting == PinyinRegistryValue) {
                IsPinyin = true;
                KeyboardLayout.Icon = IconEnum.ChinesePinyin;
                return;
            }

            KeyboardLayout.Icon = IconEnum.Unknown;
        }

        static void QueryValueChanged(object sender, EventArrivedEventArgs e) {
            Console.WriteLine("Received an event.");
            Render();
            // RegistryKeyChangeEvent occurs here; do something.
        }

        static Icon CreateTextIcon(string str) {
            Font fontToUse = new Font("Microsoft Sans Serif", 16, FontStyle.Regular, GraphicsUnit.Pixel);
            Brush brushToUse = new SolidBrush(Color.White);
            Bitmap bitmapText = new Bitmap(16, 16);
            Graphics g = System.Drawing.Graphics.FromImage(bitmapText);

            IntPtr hIcon;

            g.Clear(Color.Transparent);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            g.DrawString(str, fontToUse, brushToUse, -4, -2);
            hIcon = (bitmapText.GetHicon());
            return System.Drawing.Icon.FromHandle(hIcon);
            //DestroyIcon(hIcon.ToInt32);
        }

        static Icon CreateCharIcon(int c, string font = "Segoe MDL2 Assets") {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append((char)c);
            return CreateFontIcon(stringBuilder.ToString(), font);
        }

        static Icon CreateFontIcon(string str, string font = "Segoe MDL2 Assets") {
            Font fontToUse = new Font(font, 16, FontStyle.Regular, GraphicsUnit.Pixel);
            Brush brushToUse = new SolidBrush(Color.White);
            Bitmap bitmapText = new Bitmap(16, 16);
            Graphics g = System.Drawing.Graphics.FromImage(bitmapText);

            IntPtr hIcon;

            g.Clear(Color.Transparent);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            g.DrawString(str, fontToUse, brushToUse, -4, -2);
            hIcon = (bitmapText.GetHicon());
            return System.Drawing.Icon.FromHandle(hIcon);
            //DestroyIcon(hIcon.ToInt32);
        }
    }
}
