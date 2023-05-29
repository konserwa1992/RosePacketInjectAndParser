using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeInject
{
    public unsafe sealed  class CharacterData
    {
        public unsafe struct Position
        {
            public float x, y;
        }


        private static CharacterData _instance = null;
        private static readonly object _lock = new object();

        public unsafe Position* PlayerPosition { get; private set; }


        CharacterData()
        {
            PlayerPosition = (CharacterData.Position*)GameMethods.GetInt64(GameMethods.GetBaseAdress() + 0x01125900, new short[] { 0x258, 0x1C4 });
            Console.WriteLine("Inject");
            Console.WriteLine($"{PlayerPosition->x}");
        }

        public static CharacterData Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new CharacterData();
                    }
                    return _instance;
                }
            }
        }
    }
}
