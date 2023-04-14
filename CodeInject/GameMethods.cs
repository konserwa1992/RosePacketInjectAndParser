using CodeInject.Packet;
using CodeInject.Packet.Packet_Events;
using EasyHook;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeInject
{
    class GameMethods
    {
        #region MemoryOperations
        [DllImport("ClrBootstrap.dll")]
        public static extern UInt64 GetBaseAdress();

        [DllImport("ClrBootstrap.dll")]
        public static extern UInt64 GetInt64(UInt64 Adress);
        [DllImport("ClrBootstrap.dll")]
        public static extern int GetInt32(UInt64 Adress);
        [DllImport("ClrBootstrap.dll")]
        public static extern float GetFloat(UInt64 Adress);
        [DllImport("ClrBootstrap.dll")]
        public static extern int SendPacketToServer(UInt64 deviceAddr, byte[] packet);

        [DllImport("ClrBootstrap.dll")]
        public static extern byte GetByte(UInt64 Adress);


        [DllImport("ClrBootstrap.dll")]
        public static extern short GetShort(UInt64 Adress);

        [DllImport("ClrBootstrap.dll")]
        public static extern void GetByteArray(UInt64 adress, byte[] outTable, int size);


        public static List<byte> IgnoreSendPacketOpCode = new List<byte>() { 0x71 };



        public static UInt64 GetInt64(UInt64 Adress, short[] offsets)
        {
            UInt64 finalAdress = Adress;

            foreach (short offset in offsets)
            {
                finalAdress = GetInt64(finalAdress)+ (ulong)offset;
            }

            return finalAdress;
        }
        #endregion


        #region MemoryHooks
        public unsafe delegate void SendFunc(IntPtr a, Int16* packet);

      //  00007ff7bdc9ec69 int64_t sub_7ff7bdc9ec69(int32_t* arg1, int64_t arg2)
        public unsafe delegate Int64 ReciveFunc(IntPtr a, IntPtr packet);

        public unsafe delegate Int64 ReciveFunc2(void* a, IntPtr packet,Int32 arg3, Int32 arg4);


        public static SendFunc hookSendFunction;
        public static ReciveFunc hookRecivFunction;
        public static ReciveFunc2 hookRecivFunction2;

        private static LocalHook hookSend;
        private static LocalHook hookRecive;

        /// <summary>
        /// Have to be initialized later after login in
        /// </summary>
        public unsafe static void AttachHookSendHook()
        {
            if (hookSend == null)
            {
                hookSendFunction = PacketSendHook;

                hookSend =
                EasyHook.LocalHook.Create(new IntPtr((long)(GameMethods.GetBaseAdress() + 0x268DC)), hookSendFunction, null);
                hookSend.ThreadACL.SetExclusiveACL(new int[] { Thread.CurrentThread.ManagedThreadId });
            }

        }

        public unsafe static void AttachHookReciveHook()
        {
            if (hookRecive == null)
            {
                hookRecivFunction = PacketReciveHook;
            //hookRecivFunction2 = PacketReciveHook2;

                hookRecive =
                EasyHook.LocalHook.Create(new IntPtr((long)(GameMethods.GetBaseAdress() + 0xEC69)), hookRecivFunction, null);
                hookRecive.ThreadACL.SetExclusiveACL(new int[] { Thread.CurrentThread.ManagedThreadId });
            }
            /*hookRecive =
            EasyHook.LocalHook.Create(new IntPtr((long)(GameMethods.GetBaseAdress() + 0x3B471)), hookRecivFunction2, null);
            hookRecive.ThreadACL.SetExclusiveACL(new int[] { Thread.CurrentThread.ManagedThreadId });*/
        }


        public unsafe static Int64 PacketReciveHook2(void* a, IntPtr packet, Int32 arg3, Int32 arg4)
        {

            StreamWriter logger = new StreamWriter("logger.txt", true);
            if (arg3 == 6)
                logger.WriteLine($"{DateTime.Now.ToString("yyyy/MM/dd HH:MM:ss")}{GameMethods.GetShort((ulong)packet.ToInt64())} |||| {packet.ToInt64().ToString("X2")} |||  {arg3} ||||| {arg4}");

            if (arg3 == 6)
            {
               // Console.WriteLine($"Size: {GameMethods.GetInt32(((ulong)a.ToInt64() - 0x68) + 0x22)} Packet Recive: {packet.ToInt64().ToString("X")}");

                byte[] PacketData = new byte[GameMethods.GetShort((ulong)packet.ToInt64())];

                GameMethods.GetByteArray((ulong)packet.ToInt64(), PacketData, PacketData.Length);
                for (int i = 0; i < PacketData.Length; i++)
                {
                    Console.Write(PacketData[i].ToString("X2") + " ");
                }
                Console.WriteLine();
                Console.WriteLine("------------------------------------------------------");

                PacketParserManager.Instance.NewPacketArrived(null, new PacketArgs { Packet = new RecivePacket(PacketData) });
            }


            return RecivePacketFromServer2(a, packet,arg3,arg4);
        }
        public unsafe static Int64 PacketReciveHook(IntPtr a, IntPtr packet)
        {
            Console.WriteLine($"Size: {GameMethods.GetInt32(((ulong)a.ToInt64() - 0x68) + 0x22)} Packet Recive: {packet.ToInt64().ToString("X")}");

            byte[] PacketData = new byte[GameMethods.GetShort((ulong)packet.ToInt64())];

            GameMethods.GetByteArray((ulong)packet.ToInt64(), PacketData, PacketData.Length);
            for(int i=0;i<PacketData.Length;i++)
            {
                Console.Write(PacketData[i].ToString("X2")+ " ");
            }
            Console.WriteLine();
            Console.WriteLine("------------------------------------------------------");

            PacketParserManager.Instance.NewPacketArrived(a, new PacketArgs { Packet = new RecivePacket(PacketData) });

  
            return RecivePacketFromServer(a, packet);

        }
        public unsafe static void PacketSendHook(IntPtr a, Int16* packet)
        {
            ulong packetAdr = (ulong)(new IntPtr((void*)packet).ToInt64());
            byte[] PacketData = new byte[GameMethods.GetByte(packetAdr)];
            GameMethods.GetByteArray(packetAdr, PacketData, PacketData.Length);

            PacketParserManager.Instance.NewPacketArrived(a, new PacketArgs { Packet = new SendPacket(PacketData) });


            if (!IgnoreSendPacketOpCode.Any(x=>x == PacketData[2]))
            {
                SendPacketToServer(packet);
            }
            Console.WriteLine($"Packet Send: {new IntPtr(packet).ToInt64().ToString("X")}");
        }


        public unsafe static Int64 RecivePacketFromServer2(void* a, IntPtr packet, Int32 arg3, Int32 arg4)
        {
            //3B471
            GameMethods.ReciveFunc2 delegateRecive = (GameMethods.ReciveFunc2)Marshal.GetDelegateForFunctionPointer(new IntPtr((long)(GameMethods.GetBaseAdress() + 0x3B471)), typeof(GameMethods.ReciveFunc2));
            return delegateRecive(a, packet,arg3,arg4);
        }

        public unsafe static Int64 RecivePacketFromServer(IntPtr a, IntPtr packet)
        {
            //3B471
            GameMethods.ReciveFunc delegateRecive = (GameMethods.ReciveFunc)Marshal.GetDelegateForFunctionPointer(new IntPtr((long)(GameMethods.GetBaseAdress() + 0xEC69)), typeof(GameMethods.ReciveFunc));
            return delegateRecive(a, packet);
        }

        public unsafe static void SendPacketToServer(Int16* packet)
        {
            GameMethods.SendFunc delegateSend = (GameMethods.SendFunc)Marshal.GetDelegateForFunctionPointer(new IntPtr((long)(GameMethods.GetBaseAdress() + 0x268DC)), typeof(GameMethods.SendFunc));
            delegateSend(new IntPtr((long)GameMethods.GetInt64((GameMethods.GetBaseAdress() + 0x1126948)) + 0x000016D8 + 0x320), packet);
        }
        #endregion
    }
}
