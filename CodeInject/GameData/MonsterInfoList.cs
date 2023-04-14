using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeInject.GameData
{
    class MonsterInfoList
    {
        public static MonstersData MonsterInfo = new MonstersData();

        public static string GetNameByID(int id)
        {
            MonsterData monsterData = MonsterInfo.MonsterDataList.FirstOrDefault(x => x.Id == id);
           
            return monsterData!=null? monsterData.Name: "unknow "+id.ToString();
        }

        private MonsterInfoList() {

           
        }
    }

    public class MonsterData
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class MonstersData
    {
        public List<MonsterData> MonsterDataList { get; set; } = new List<MonsterData>();

        public MonstersData()
        {
            StreamReader monsterDataFile = new StreamReader("monsterList.txt");

            int id;
            string name;
            string line;
            while (!monsterDataFile.EndOfStream)
            {
                line = monsterDataFile.ReadLine();

                string[] splitedData = line.Split(';');

                  if (splitedData.Length == 2 && splitedData[0]!="" && splitedData[1]!="")
                {
                    MonsterDataList.Add(new MonsterData()
                    {
                        Id = int.Parse(splitedData[0]),
                        Name = splitedData[1]
                    });
                }
            }
        }
    }

    public static class JsonHandler
    {
        public static MonstersData ParseJson(string json)
        {
            return JsonConvert.DeserializeObject<MonstersData>(json);
        }
    }
}
