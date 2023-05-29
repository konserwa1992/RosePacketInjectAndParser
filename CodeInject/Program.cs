using CodeInject;
using CodeInject.NPC;
using CodeInject.Packet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ISpace
{
    public static class IClass
    {
        static Form1 form;

        //public static int IMain(string args)
        public static unsafe int IMain(string msg)
        {
            GameMethods.AttachHookReciveHook();
            PacketParserManager.Instance.Record = !PacketParserManager.Instance.Record;
            MessageBox.Show(CharacterData.Instance.PlayerPosition->y.ToString());
            form = new Form1();
             form.ShowDialog();
            AllocConsole();
            return 0;
        }


        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("kernel32")]
        static extern bool AllocConsole();

        [DllImport("ClrBootstrap.dll")]
        static extern int AttackTarget(uint skill, uint monsterIndex);
    }
}
