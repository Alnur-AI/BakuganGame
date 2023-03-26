using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using static System.Reflection.Metadata.BlobBuilder;
using System.Globalization;
using static System.Net.Mime.MediaTypeNames;
using System.ComponentModel.Design;

namespace BakuganGame
{
    /*
        Данный класс определяет поле, которое содержит в себе 
        Фиксированное количество ворот на время игры
        Менять количество ворот на поле запрещено 

        Так же поле содержит в себе информацию по бакуганам, их картам ворот и способностей    
    */
    internal class Field
    {

        public uint NbrBaku { get; private set; }
        public uint NbrTeam { get; private set; }
        public uint NbrBraw { get; private set; }

        public uint currGateX { get; private set; }//Осмотр ворот по Ох
        public uint currGateY { get; private set; }//Осмотр ворот по Оу

        public Brawler[] brawler;
        public Gate[,] gate;

        string appLog;// логирование процессов и ошибок всего приложения (Console Information)
        string battleLog;// логирование процессов битвы во время выполнения приложения (Console Information)

        int currBrawlerIndex = 0;
        public uint currBrawler{ get; private set; }//Текущий игрок по списку взял управление
        public List<Tuple<uint, uint>> queueGame { get; private set; } //Список игроков и их принадлежность к команде

        

        public List<List<Bakugan>> battleLink { get; set; }// Список бакуганов учавствующих в различных активных битвах. Заметим, что первый элемент каждого списка это инициатор боя

        public Field()
        {
            currGateX = 0;
            currGateY = 0;

            battleLink = new List<List<Bakugan>>();

            setAppLog($"Connecting battle.xml");
            bool xmlFileFound = false;
            XmlDocument xmlDoc = new XmlDocument();
            while (!xmlFileFound)
            {
                try
                {
                    if (File.Exists("battle.xml"))
                    {
                        xmlDoc.Load("battle.xml");

                        setAppLog($"Connecting constants.xml");
                        XmlNodeList constants = xmlDoc.GetElementsByTagName("Constants");
                        foreach (XmlNode cnst in constants)
                        {
                            XmlNodeList cnstTmp = cnst.ChildNodes;
                            foreach (XmlNode cnstItem in cnstTmp)
                            {
                                if (cnstItem.Name == "NbrBaku")
                                    NbrBaku = uint.Parse(cnstItem.InnerText);
                                if (cnstItem.Name == "NbrTeam")
                                    NbrTeam = uint.Parse(cnstItem.InnerText);
                                if (cnstItem.Name == "NbrBraw")
                                    NbrBraw = uint.Parse(cnstItem.InnerText);
                            }
                        }

                        xmlFileFound = true;
                    }
                    else
                    {
                        setAppLog($"ERROR: File battle.xml does not exists in root directory.");
                        Console.WriteLine("ERROR: File battle.xml does not exists in root directory.");
                    }

                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occur during parsing
                    Console.WriteLine("Error parsing XML file: " + ex.Message);
                }
                Thread.Sleep(1000);
            }

            setAppLog($"Number of Bakugan {NbrBaku}");
            setAppLog($"Number of Team {NbrTeam}");
            setAppLog($"Number of Brawler {NbrBraw}\n\n");


            setAppLog($"Initializing field");

            brawler = new Brawler[NbrBraw];
            for (uint i = 0; i < NbrBraw; i++)
            {
                brawler[i] = new Brawler(NbrBaku, NbrTeam, NbrBraw, i,this);
                setAppLog($"Field message: Allocate memory to Brawler {i}");
            }

            gate = new Gate[NbrBraw, NbrBaku];
            for (int i = 0; i < NbrBraw; i++)
                for (int j = 0; j < NbrBaku; j++)
                {
                    gate[i, j] = new Gate(NbrBaku, NbrTeam, NbrBraw, i, j,this);
                    setAppLog($"Field message: Allocate memory to Gate ({i}, {j})");
                }
            setAppLog($"Connecting brawlers from battle.xml");
            XmlNodeList xmlBrawler = xmlDoc.GetElementsByTagName("Brawler");
            for (int i = 0; i < xmlBrawler.Count; i++)
            {
                int j = 0;//количество бакуганов в парсинге
                XmlNodeList brwlrTmp = xmlBrawler[i].ChildNodes;
                foreach (XmlNode brwlrItem in brwlrTmp)
                {
                    if (brwlrItem.Name == "TeamID")
                    {
                        brawler[i].teamID = uint.Parse(brwlrItem.InnerText);

                        for (uint k = 0; k < NbrBaku; k++)
                            brawler[i].bakugan[k].define(brawler[i].teamID, (uint)i, k);
                        
                        for (uint k = 0; k < 3 * NbrBaku; k++)
                            brawler[i].abilityCard[k].define(brawler[i].teamID, (uint)i, k);
                        
                        for (uint k = 0; k < NbrBaku; k++)
                            brawler[i].gateCard[k].define(brawler[i].teamID, (uint)i, k);
                        
                        Console.WriteLine("TeamID " + brawler[i].teamID);
                    }
                    if (brwlrItem.Name == "Name")
                    {
                        
                        brawler[i].name = brwlrItem.InnerText;
                        Console.WriteLine("Name " + brawler[i].name);
                    }
                    if (brwlrItem.Name == "BID")
                    {
                        brawler[i].BID = Convert.ToUInt64(brwlrItem.InnerText, 16);
                        Console.WriteLine("BID " + brawler[i].BID);
                    }
                    if (brwlrItem.Name == "Rank")
                    {
                        brawler[i].rank = ulong.Parse(brwlrItem.InnerText);
                        Console.WriteLine("Rank " + brawler[i].rank);
                    }
                    if (brwlrItem.Name == "Money")
                    {
                        brawler[i].money = double.Parse(brwlrItem.InnerText);
                        Console.WriteLine("Money " + brawler[i].money);
                    }
                    if (brwlrItem.Name == "DoomCard")
                    {
                        if (brwlrItem.InnerText == "Yes")
                            brawler[i].avDoom = true;
                        if (brwlrItem.InnerText == "No")
                            brawler[i].avDoom = false;
                        Console.WriteLine("DoomCard" + brawler[i].avDoom);
                    }

                    
                    if (brwlrItem.Name == "Bakugan")
                    {
                        brawler[i].bakugan[j].state = 0;
                        Console.WriteLine(j);
                        XmlNodeList xmlBakugan = brwlrItem.ChildNodes;
                        foreach (XmlNode bakuItem in xmlBakugan)
                        {
                            if (bakuItem.Name == "Name")
                            {
                                brawler[i].bakugan[j].name = bakuItem.InnerText;// +xmlBakugan[j].InnerText;
                                Console.WriteLine("Name " + brawler[i].bakugan[j].name);
                            }
                                
                            if (bakuItem.Name == "GPower")
                            {
                                brawler[i].bakugan[j].g = int.Parse(bakuItem.InnerText);
                                brawler[i].bakugan[j].gGame = int.Parse(bakuItem.InnerText);
                                brawler[i].bakugan[j].gGlobal = int.Parse(bakuItem.InnerText);
                                Console.WriteLine("GPower " + brawler[i].bakugan[j].g);
                            }
                            if (bakuItem.Name == "Attribute")
                            {
                                switch (bakuItem.InnerText)
                                {
                                    case "Pyrus":
                                        brawler[i].bakugan[j].isPyrus = true;
                                        break;
                                    case "Aquos":
                                        brawler[i].bakugan[j].isAquos = true;
                                        break;
                                    case "Darkus":
                                        brawler[i].bakugan[j].isDarkus = true;
                                        break;
                                    case "Ventus":
                                        brawler[i].bakugan[j].isVentus = true;
                                        break;
                                    case "Subterra":
                                        brawler[i].bakugan[j].isSubterra = true;
                                        break;
                                    case "Haos":
                                        brawler[i].bakugan[j].isHaos = true;
                                        break;
                                }
                            }
                            
                        }
                        j++;
                    }
                    if (brwlrItem.Name == "AbilityCard")
                    {
                        XmlNodeList xmlAbiCard = brwlrItem.ChildNodes;
                        for (int k = 0; k < xmlAbiCard.Count; k++)
                        {
                            if (xmlAbiCard[k].Name == "AbilityID")
                            {
                                brawler[i].abilityCard[k].abilityType = uint.Parse(xmlAbiCard[k].InnerText);
                                Console.WriteLine("AbilityID " + brawler[i].abilityCard[k].abilityType);
                            }
                        }
                    }
                    if (brwlrItem.Name == "GateCard")
                    {
                        XmlNodeList xmlGtCard = brwlrItem.ChildNodes;
                        for (int k = 0; k < xmlGtCard.Count; k++)
                        {
                            if (xmlGtCard[k].Name == "GateID")
                            {
                                brawler[i].gateCard[k].gateType = uint.Parse(xmlGtCard[k].InnerText);
                                Console.WriteLine("GateID " + brawler[i].gateCard[k].gateType);
                            }
                        }
                    }
                }
            }
            
            setAppLog($"Field initialized\n\n");
        }











        /// <summary>
        /// Записываем строку str в AppLog
        /// </summary>
        /// <returns>Возвращает true - если удалось заполнить </returns>
        public bool setAppLog(string str)
        {
            appLog = appLog + str + '\n';
            return true;
        }

        /// <summary>
        /// Записываем строку str в BatteLog
        /// </summary>
        /// <returns>Возвращает true - если удалось заполнить </returns>
        public bool setBattleLog(string str)
        {
            battleLog = battleLog + str + '\n';
            return true;
        }
        
        /// <summary>
        /// Записываем строку appLog в файл
        /// </summary>
        /// <returns>Возвращает true - если удалось заполнить </returns>
        public bool printAppLog()
        {
            try
            {
                //string path = @"C:\Users\Hikari\Desktop\bakugan\newproject\BakuganGame\BakuganGame\AppLog.txt";
                string path = "Applog.txt";
                StreamWriter sw = new StreamWriter(path);

                sw.Write(appLog);
                sw.Close();
                
            }
            catch(Exception e) 
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            return true;
        }

        public bool controlField(ConsoleKeyInfo key)
        {
            if (key.Key == ConsoleKey.LeftArrow || key.Key == ConsoleKey.A)
            {
                
                if (currGateX > 0)
                    currGateX--;
                else if (currGateX <= 0)
                    currGateX = 0;
            }
            if (key.Key == ConsoleKey.RightArrow || key.Key == ConsoleKey.D)
            {
                if(currGateX < NbrBraw-1)
                    currGateX++;
                else if(currGateX >= NbrBraw)
                    currGateX = NbrBraw-1;
            }
            if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.W)
            {
                if (currGateY > 0)
                    currGateY--;
                else if (currGateY <= 0)
                    currGateY = 0;
            }
            if (key.Key == ConsoleKey.DownArrow || key.Key == ConsoleKey.S)
            {
                if (currGateY < NbrBaku - 1)
                    currGateY++;
                else if (currGateY >= NbrBaku)
                    currGateY = NbrBaku-1;
            }
            return true;
        }

        public void printBakuName(int brawlerID, int bakuganID, int x, int y, string str)
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.SetCursorPosition(x, y);

            int len = str.Length;
            int[] attrCount = new int[6];
            attrCount[0] = 0;
            attrCount[1] = 0;
            attrCount[2] = 0;
            attrCount[3] = 0;
            attrCount[4] = 0;
            attrCount[5] = 0;

            attrCount[0] += Convert.ToInt32(brawler[brawlerID].bakugan[bakuganID].isAquos);
            attrCount[1] += Convert.ToInt32(brawler[brawlerID].bakugan[bakuganID].isDarkus);
            attrCount[2] += Convert.ToInt32(brawler[brawlerID].bakugan[bakuganID].isHaos);
            attrCount[3] += Convert.ToInt32(brawler[brawlerID].bakugan[bakuganID].isPyrus);
            attrCount[4] += Convert.ToInt32(brawler[brawlerID].bakugan[bakuganID].isSubterra);
            attrCount[5] += Convert.ToInt32(brawler[brawlerID].bakugan[bakuganID].isVentus);

            // Calculate the length of each part based on the percentages
            double tempp;
            int[] partLengths = new int[6];
            int numParts = Math.Min(6, attrCount.Length);
            for (int i = 0; i < numParts; i++)
            {
                tempp = Convert.ToDouble(attrCount[i]) / Convert.ToDouble(attrCount.Sum());
                partLengths[i] = (int)Math.Round(tempp * len);
            }

            // Add any remaining characters to the last part
            int remainingChars = len - partLengths.Sum();
            partLengths[numParts - 1] += remainingChars;

            // Split the string into 6 parts
            int startPosition = 0;
            string part;
            for (int i = 0; i < numParts; i++)
            {
                switch (i)
                {
                    case 0:
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        break;
                    case 1:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;
                    case 2:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case 3:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case 4:
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        break;
                    case 5:
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                }
                if (startPosition + partLengths[i] >= len)
                {
                    part = str.Substring(startPosition);
                    Console.Write(part);
                    break;
                }
                else
                {
                    part = str.Substring(startPosition, partLengths[i]);
                }
                Console.Write(part);
                startPosition += partLengths[i];
            }
            Console.ForegroundColor = old;
        }
        public void printBrawName(int brawlerID,int x, int y, string str)
        {
            ConsoleColor old = Console.ForegroundColor;
            Console.SetCursorPosition(x, y);

            int len = str.Length;
            int[] attrCount = new int[6];
            attrCount[0] = 0;
            attrCount[1] = 0;
            attrCount[2] = 0;
            attrCount[3] = 0;
            attrCount[4] = 0;
            attrCount[5] = 0;

            for (int i = 0; i < NbrBaku; i++)
            {
                attrCount[0] += Convert.ToInt32(brawler[brawlerID].bakugan[i].isAquos);
                attrCount[1] += Convert.ToInt32(brawler[brawlerID].bakugan[i].isDarkus);
                attrCount[2] += Convert.ToInt32(brawler[brawlerID].bakugan[i].isHaos);
                attrCount[3] += Convert.ToInt32(brawler[brawlerID].bakugan[i].isPyrus);
                attrCount[4] += Convert.ToInt32(brawler[brawlerID].bakugan[i].isSubterra);
                attrCount[5] += Convert.ToInt32(brawler[brawlerID].bakugan[i].isVentus);
            }


            // Calculate the length of each part based on the percentages
            double tempp;
            int[] partLengths = new int[6];
            int numParts = Math.Min(6, attrCount.Length);
            for (int i = 0; i < numParts; i++)
            {
                tempp = Convert.ToDouble(attrCount[i]) / Convert.ToDouble(attrCount.Sum());
                partLengths[i] = (int)Math.Round(tempp * len) ; 
            }

            // Add any remaining characters to the last part
            int remainingChars = len - partLengths.Sum();
            partLengths[numParts-1] += remainingChars;

            // Split the string into 6 parts
            int startPosition = 0;
            string part;
            for (int i = 0; i < numParts; i++)
            {
                switch (i)
                {
                    case 0:
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        break;
                    case 1:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;
                    case 2:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case 3:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case 4:
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        break;
                    case 5:
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                }
                if(startPosition + partLengths[i] >= len)
                {
                    part = str.Substring(startPosition);
                    Console.Write(part);
                    break;
                }
                else
                {
                    part = str.Substring(startPosition, partLengths[i]);
                }
                Console.Write(part);
                startPosition += partLengths[i];
            }
            Console.ForegroundColor = old;
        }

        public void printText(ConsoleColor clr,int x,int y, string str)
        {
            ConsoleColor old = Console.ForegroundColor;

            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = clr;
            Console.Write(str);

            Console.ForegroundColor = old;
        }
        public void printText(ConsoleColor clrTex,ConsoleColor clrBgr, int x, int y, string str)
        {
            ConsoleColor oldTex = Console.ForegroundColor;
            ConsoleColor oldBgr = Console.BackgroundColor;

            Console.SetCursorPosition(x, y);
            Console.ForegroundColor = clrTex;
            Console.BackgroundColor = clrBgr;
            Console.Write(str);

            Console.ForegroundColor = oldTex;
            Console.BackgroundColor = oldBgr;
        }
        
        public bool drawField()
        {
            // Field
            printText(ConsoleColor.Red,13,0,"Field");
            for (int j = 0; j < NbrBaku; j++)
                for (int i = 0; i < NbrBraw; i++)
                {
                    if (!gate[i, j].isBusy)
                        printText(ConsoleColor.White, 3 + i, 2 + j, ".");
                    else
                        printText(ConsoleColor.Green, 3 + i, 2 + j, "#");
                    if (i == currGateX && j == currGateY)
                        printText(ConsoleColor.Red, 3 + i, 2 + j, "*");


                }
            
            return true;
        }
        public bool drawFieldInfo()
        {
            //Field Information
            printText(ConsoleColor.Red, 13, 10, "Field Info");
            printText(ConsoleColor.White, 0, 11, "---------------------------------------");

            string bakuLeft;
            int tempTeamCnt = 0;
            for (int j = 0; j <= NbrTeam; j++)
            {
                printText(ConsoleColor.Black, ConsoleColor.Gray, 0, 12 + tempTeamCnt, "Team "+(j)+"    Bakugan     Ability     Gate ");
                tempTeamCnt++;
                for (int i = 0; i < NbrBraw; i++)
                {
                    if(j == brawler[i].teamID)
                    {
                        if (i == currBrawler)
                            Console.BackgroundColor = ConsoleColor.DarkGray;
                        bakuLeft = new string('*', (int)(NbrBaku - brawler[i].loseBakugan));
                        printBrawName(i, 0, 12 + tempTeamCnt, brawler[i].name + "");
                        if (i == currBrawler)
                            Console.BackgroundColor = ConsoleColor.Black;


                        printText(ConsoleColor.Yellow, 12, 12 + tempTeamCnt, bakuLeft);
                        printText(ConsoleColor.White, 25, 12 + tempTeamCnt, "" + (NbrBaku - brawler[i].usedAbilityCard));
                        printText(ConsoleColor.White, 35, 12 + tempTeamCnt, "" + (NbrBaku - brawler[i].usedGateCard));
                        tempTeamCnt++;
                        
                    }
                    //12 + (j)* ((int)NbrBraw+1)
                    //12 + (j) * ((int)NbrBraw+1) + i + 1
                }
            }

            return true;
        }
        public bool drawGateInfo()
        {
            //Gate Info
            printText(ConsoleColor.Red, 55, 10, "Gate Info");
            printText(ConsoleColor.White, 40, 11, "---------------------------------------");

            int gTotal = 0;
            int tempTeamCnt = 0;
            for (int j = 1; j <= NbrTeam; j++)
            {
                printText(ConsoleColor.Black,ConsoleColor.Gray, 40, 12 + tempTeamCnt, "Team " + (j) + "                        G power  ");
                tempTeamCnt++;

                for (int i = 0; i < gate[currGateX, currGateY].bakugan.Count; i++)
                {
                    if (j == gate[currGateX, currGateY].bakugan[i].team)
                    {
                        uint bakuOwner = gate[currGateX, currGateY].bakugan[i].owner;
                        uint bakuID = gate[currGateX, currGateY].bakugan[i].bakuganID;
                        string brawName = brawler[bakuOwner].name;
                        printBrawName((int)bakuOwner, 40, 12 + tempTeamCnt, brawName + ":" );
                        printBakuName((int)bakuOwner,(int)bakuID, 42 + brawName.Length, 12 + tempTeamCnt, gate[currGateX, currGateY].bakugan[i].name);
                        printText(ConsoleColor.White, 72, 12 + tempTeamCnt, "" + gate[currGateX, currGateY].bakugan[i].g);
                        gTotal += gate[currGateX, currGateY].bakugan[i].g;
                        tempTeamCnt++;
                    }
                }
                printText(ConsoleColor.White, 40, 12 + tempTeamCnt, "Total G power: ");
                printText(ConsoleColor.White, 72, 12 + tempTeamCnt, "" + gTotal);
                tempTeamCnt++;
                gTotal = 0;
                
            }
            
            return true;
        }
        public bool drawBattleLink()
        {

            printText(ConsoleColor.Red, 95,0,"Queue Game: ");
            printText(ConsoleColor.White, 80, 1, "---------------------------------------");

            int strCount = 2;
            foreach (Tuple<uint, uint> queueBrawler in queueGame)
            {
                if ( queueBrawler.Item1 == currBrawler )
                    Console.BackgroundColor = ConsoleColor.DarkGray;
                printText(ConsoleColor.White,80,strCount, brawler[queueBrawler.Item1].teamID + ": ");
                printBrawName((int)queueBrawler.Item1, 83, strCount, brawler[queueBrawler.Item1].name);
                Console.BackgroundColor = ConsoleColor.Black;
                strCount ++;
            }

            printText(ConsoleColor.Red, 95, strCount++, "BattleLink: ");
            printText(ConsoleColor.White, 80, strCount++, "---------------------------------------");

            foreach (List<Bakugan> innerList in battleLink)
            {
                printText(ConsoleColor.White, 80, strCount, innerList.Count + ":  ");
                foreach (Bakugan obj in innerList)
                {
                    if (obj.owner == currBrawler)
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                    if (obj.owner == getNextPlayer())
                        Console.BackgroundColor = ConsoleColor.Red;
                    Console.Write(obj.name + " -> " );

                    Console.BackgroundColor = ConsoleColor.Black;
                }
                strCount++;
            }
            return true;
        }

        public void nextPlayer()
        {
            currBrawlerIndex++;
            if(currBrawlerIndex >= NbrBraw)
                currBrawlerIndex = 0;

            currBrawler = queueGame[currBrawlerIndex].Item1;
        }
        public uint getNextPlayer()
        {
            int tmp = currBrawlerIndex;
            tmp++;
            if (tmp >= NbrBraw)
                tmp = 0;
            return queueGame[tmp].Item1;
        }

        public void SortBrawlers()
        {
            queueGame = new List<Tuple<uint, uint>>();

            List<Tuple<uint, uint>> tempLst = brawler.Select(x => Tuple.Create(x.brawlerID, x.teamID)).ToList();
            uint minTeam = tempLst.Min(x => x.Item2);
            uint maxTeam = tempLst.Max(x => x.Item2);

            uint currPos = 0;
            uint currTeam = minTeam ;
            while (tempLst.Count > 0)
            {
                
                int i = 0;
                while (i < tempLst.Count)
                {
                    if (tempLst[i].Item2 == currTeam)
                    {
                        queueGame.Add(new Tuple<uint, uint>(tempLst[i].Item1, tempLst[i].Item2));
                        tempLst.RemoveAt(i);
                        currPos++;
                        break;
                    }
                    else
                        i++;

                }
                currTeam++;
                if (currTeam > maxTeam)
                    currTeam = minTeam;
                
            }
            tempLst = queueGame.GetRange(0, queueGame.Count);
            for (int i = 0; i < NbrBraw; i++)
            {
                brawler[i].queueID = tempLst[i].Item1;
                //Console.WriteLine(brawler[i].queueID);
            }
            //queueGame.ForEach(Console.WriteLine);
        }
    }
}
