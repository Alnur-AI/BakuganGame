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
using System.Xml.Linq;

namespace BakuganGame
{
    public class Menu
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public Menu Parent { get; set; }
        public List<Menu> Children { get; set; }

        public Menu(string name, int id, Menu parent = null)
        {
            Name = name;
            Id = id;
            Parent = parent;
            Children = new List<Menu>();
        }

        public void AddChild(Menu child)
        {
            child.Parent = this;
            Children.Add(child);
        }
    }
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

        // добавить константы ограничения бакуганов за один ход
        public uint MaxGateTurn { get; private set; }
        public uint MaxBakuTurn { get; private set; }
        public uint MaxAbilTurn { get; private set; }

        public uint currGateX { get; private set; }//Осмотр ворот по Ох
        public uint currGateY { get; private set; }//Осмотр ворот по Оу

        public Brawler[] brawler;
        public Gate[,] gate;

        string appLog;// логирование процессов и ошибок всего приложения (Console Information)
        string battleLog;// логирование процессов битвы во время выполнения приложения (Console Information)

        int currBrawlerIndex = 0;
        public uint currBrawler{ get; private set; }//Текущий игрок по списку взял управление
        public int currUsedBaku { get; private set; }
        public int currUsedGate { get; private set; }
        public int currUsedAbil { get; private set; }

        public List<Tuple<uint, uint>> queueGame { get; private set; } //Список игроков и их принадлежность к команде

        public Menu menu { get; set; }
        public Menu subMenu { get; set; }
        public int selectMenu { get; set; }
        public Stack <int> prevSelectMenu { get; set; }

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
                                if (cnstItem.Name == "MaxAbilTurn")
                                    MaxAbilTurn = uint.Parse(cnstItem.InnerText);
                                if (cnstItem.Name == "MaxBakuTurn")
                                    MaxBakuTurn = uint.Parse(cnstItem.InnerText);
                                if (cnstItem.Name == "MaxGateTurn")
                                    MaxGateTurn = uint.Parse(cnstItem.InnerText);
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
            prevSelectMenu = new Stack<int>();
            defineMenu();
            setAppLog($"Field initialized\n\n");
        }

        public void defineMenu()
        {
            int count = 1;

            menu = new Menu("Choice Action", 0);

            Menu grandpa;
            Menu parent = new Menu("Set Gate Card", count++, menu);
            menu.Children.Add(parent);

            
            for (int i = 0; i < NbrBaku; i++)
            {
                parent = new Menu("Gate " + (i + 1), count++, parent);
                menu.Children[0].Children.Add(parent);
                parent = parent.Parent;
                menu.Children[0].Children[i].Children.Add(new Menu("Yes", count++, parent));
                menu.Children[0].Children[i].Children.Add(new Menu("No", count++, parent));
            }
            

            parent = new Menu("Throw bakugan", count++, menu);
            menu.Children.Add(parent);
            
            for (int i = 0; i < NbrBaku; i++)
            {
                parent = new Menu("Bakugan " + (i + 1), count++, parent);
                menu.Children[1].Children.Add(parent);
                parent = parent.Parent;
                menu.Children[1].Children[i].Children.Add(new Menu("Yes", count++, parent));
                menu.Children[1].Children[i].Children.Add(new Menu("No", count++, parent));
            }
            

            parent = new Menu("Use Ability Card", count++, menu);
            menu.Children.Add(parent);
            
            for (int i = 0; i < NbrBaku; i++)
            {
                parent = new Menu("Ability Card " + (i + 1), count++, parent);
                menu.Children[2].Children.Add(parent);
                parent = parent.Parent;
                menu.Children[2].Children[i].Children.Add(new Menu("Yes", count++, parent));
                menu.Children[2].Children[i].Children.Add(new Menu("No", count++, parent));
            }
            

            parent = new Menu("Open Gate", count++, menu);
            menu.Children.Add(parent);

            //menu.Children[3].Children.Add(new Menu("You Sure?", count++, parent));
            menu.Children[3].Children.Add(new Menu("Yes", count++, parent));
            menu.Children[3].Children.Add(new Menu("No", count++, parent));


            parent = new Menu("Doom Card:", count++, menu);
            menu.Children.Add(parent);

            //menu.Children[3].Children.Add(new Menu("You Sure?", count++, parent));
            menu.Children[4].Children.Add(new Menu("Yes", count++, parent));
            menu.Children[4].Children.Add(new Menu("No", count++, parent));


            parent = new Menu("Skip turn", count++, menu);
            menu.Children.Add(parent);
                //menu.Children[5].Children.Add(new Menu("You Sure?", count++, parent));
            menu.Children[5].Children.Add(new Menu("Yes", count++, parent));
            menu.Children[5].Children.Add(new Menu("No", count++, parent));

            subMenu = menu;
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
            if (key.Key == ConsoleKey.A)
            {
                
                if (currGateX > 0)
                    currGateX--;
                else if (currGateX <= 0)
                    currGateX = 0;
            }
            if ( key.Key == ConsoleKey.D)
            {
                if(currGateX < NbrBraw-1)
                    currGateX++;
                else if(currGateX >= NbrBraw)
                    currGateX = NbrBraw-1;
            }
            if ( key.Key == ConsoleKey.W)
            {
                if (currGateY > 0)
                    currGateY--;
                else if (currGateY <= 0)
                    currGateY = 0;
            }
            if ( key.Key == ConsoleKey.S)
            {
                if (currGateY < NbrBaku - 1)
                    currGateY++;
                else if (currGateY >= NbrBaku)
                    currGateY = NbrBaku-1;
            }
            return true;
        }
        public bool controlInGame(ConsoleKeyInfo key)
        {

            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    selectMenu--;
                    if (selectMenu < 0)
                        selectMenu = subMenu.Children.Count-1;
                    //Console.WriteLine(subMenu.Children[selectMenu].Name);
                    break;

                case ConsoleKey.DownArrow:
                    selectMenu++;
                    if (selectMenu >= subMenu.Children.Count)
                        selectMenu = 0;
                    //Console.WriteLine(subMenu.Children[selectMenu].Name);
                    break;

                case ConsoleKey.Backspace:
                    if (subMenu.Parent != null)
                    {
                        subMenu = subMenu.Parent;
                        selectMenu = prevSelectMenu.Pop();
                    }
                    
                    break;

                case ConsoleKey.Enter:
                    if (subMenu.Children.Count > selectMenu)
                    {
                        if (subMenu.Children[selectMenu] != null && subMenu.Children[selectMenu].Children.Count != 0)
                        {
                            subMenu = subMenu.Children[selectMenu];
                            prevSelectMenu.Push(selectMenu);
                            selectMenu = 0;
                        }
                        else
                        {
                            selectControlLogic();
                        }
                    }
                    
                    break;
            }
            return true;
        }
        public void selectControlLogic()
        {
            // Если выбран вариант yes или no
            if (subMenu.Children[selectMenu].Name == "No")
            {
                subMenu = subMenu.Parent;
                selectMenu = prevSelectMenu.Pop();
                //selectMenu = 0;
                //prevSelectMenu.Clear();
            }              
            if(subMenu.Children[selectMenu].Name == "Yes")
            {

                if (subMenu.Name.Length >= 10)
                {
                    if (subMenu.Name.Substring(0, 10) == "Doom Card:")
                    {
                        if (brawler[currBrawler].setDoom)
                            brawler[currBrawler].removeDoomCard();
                        
                        else
                            brawler[currBrawler].setDoomCard();
                        
                    }
                }
                if (subMenu.Name.Length >= 9)
                {
                    if (subMenu.Name.Substring(0, 9) == "Skip turn")
                    {
                        nextPlayer();
                        checkBattleRes(this);

                        

                        subMenu = menu;
                        return;
                    }
                }


                // Облатсь выбора бакуганов карт ворот и способностей
                int tempChoice = prevSelectMenu.Pop();
                if (subMenu.Name.Length >= 7)
                {
                    if (subMenu.Name.Substring(0, 7) == "Bakugan")
                    {
                        if(currUsedBaku <= MaxBakuTurn)
                            brawler[currBrawler].ForceThrowBakugan((int)currGateX, (int)currGateY, tempChoice);
                        currUsedBaku++;
                    }
                        
                    
                    if (subMenu.Name.Substring(0, 7) == "Ability")
                    {
                        // Код появится когда доработается алгоритмы способностей
                        currUsedAbil++;
                    }
                }              
                if (subMenu.Name.Length >= 4)
                    if (subMenu.Name.Substring(0, 4) == "Gate")
                    {
                        if(currUsedGate <= MaxGateTurn)
                            brawler[currBrawler].setGate((int)currGateX, (int)currGateY, (uint)tempChoice);
                        currUsedGate++;
                    }
                prevSelectMenu.Push(tempChoice);


                subMenu = menu;
            }
            
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
        public bool drawControl()
        {
            printText(ConsoleColor.Red, 55, 0, "Control");
            printText(ConsoleColor.White, 40, 1, "---------------------------------------");
            printText(ConsoleColor.White, 53, 2, subMenu.Name);

            int index = 0;
            while (index < subMenu.Children.Count)
            {

                bool isDoomCard = false;
                bool isBakuganLabel = false;
                bool isGateCardLabel = false;
                bool isAbilityLabel = false;

                if (subMenu.Children[index].Name.Length >= 10)
                {
                    if (subMenu.Children[index].Name.Substring(0, 10) == "Doom Card:")
                    {
                        isDoomCard = true;

                        if (index == selectMenu)
                        {
                            printText(ConsoleColor.Black, ConsoleColor.White, 40, index + 3, subMenu.Children[index].Name);
                            if (brawler[currBrawler].setDoom)
                            {
                                printText(ConsoleColor.Green, ConsoleColor.White, 50, index + 3, "On");
                            }
                            else
                            {
                                printText(ConsoleColor.Red, ConsoleColor.White, 50, index + 3, "Off");
                            }
                        }
                        else
                        {
                            printText(ConsoleColor.White, ConsoleColor.Black, 40, index + 3, subMenu.Children[index].Name);
                            if (brawler[currBrawler].setDoom)
                            {
                                printText(ConsoleColor.Green, ConsoleColor.Black, 50, index + 3, "On");
                            }
                            else
                            {
                                printText(ConsoleColor.Red, ConsoleColor.Black, 50, index + 3, "Off");
                            }
                        }
                    }
                }

                if (subMenu.Children[index].Name.Length >= 7)
                {
                    if (subMenu.Children[index].Name.Substring(0, 7) == "Bakugan")
                    {
                        isBakuganLabel = true;
                        if (index == selectMenu)
                        {
                            printText(ConsoleColor.Black, ConsoleColor.White, 40, index + 3, brawler[currBrawler].bakugan[index].name);
                        }
                        else if (brawler[currBrawler].bakugan[index].state == BakuState.InInventory)
                        {
                            printBakuName((int)currBrawler, index, 40, index + 3, brawler[currBrawler].bakugan[index].name);
                        }
                        else if (brawler[currBrawler].bakugan[index].state == BakuState.OnGate)
                        {
                            printBakuName((int)currBrawler, index, 40, index + 3, $"({brawler[currBrawler].bakugan[index].x},{brawler[currBrawler].bakugan[index].y})" + brawler[currBrawler].bakugan[index].name);
                        }
                        else if (brawler[currBrawler].bakugan[index].state == BakuState.KnockedOut)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkRed;
                            printBakuName((int)currBrawler, index, 40, index + 3, brawler[currBrawler].bakugan[index].name);
                            Console.BackgroundColor = ConsoleColor.White;
                        }
                        else if (brawler[currBrawler].bakugan[index].state == BakuState.Killed)
                        {
                            printText(ConsoleColor.Black, ConsoleColor.White, 40, index + 3, "KILLED");
                        }
                    }
                    if (subMenu.Children[index].Name.Substring(0, 7) == "Ability")
                    {
                        isAbilityLabel = true;
                        
                    }
                }
                if (subMenu.Children[index].Name.Length >= 4)
                {
                    if (subMenu.Children[index].Name.Substring(0, 4) == "Gate")
                    {
                        isGateCardLabel = true;
                        if (currUsedGate >= MaxGateTurn) 
                        {
                            printText(ConsoleColor.Red, 40, index + 3, "UNAVAILABLE");
                        }
                        else if (index == selectMenu)
                        {
                            printText(ConsoleColor.Black, ConsoleColor.White, 40, index + 3, subMenu.Children[index].Name);
                        }
                        else if (brawler[currBrawler].gateCard[index].isPlaced)
                        {
                            printText(ConsoleColor.Red, 40, index + 3, subMenu.Children[index].Name);
                        }
                        else
                        {
                            printText(ConsoleColor.White, 40, index + 3, subMenu.Children[index].Name);
                        }
                    }
                }
                
                if (!isBakuganLabel && !isGateCardLabel && !isDoomCard)
                {
                    printText(ConsoleColor.White, ConsoleColor.Black, 40, index + 3, subMenu.Children[index].Name);
                   
                }
                if (index == selectMenu && (!isBakuganLabel && !isGateCardLabel && !isDoomCard))
                {
                    printText(ConsoleColor.Black, ConsoleColor.White, 40, index + 3, subMenu.Children[index].Name);
                }

                
                index++;
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
            int gTotal = 0;
            int tempTeamCnt = 0;


            foreach (List<Bakugan> innerList in battleLink)
            {
                printText(ConsoleColor.Black, ConsoleColor.Gray, 80, strCount++, "Battle " + "                     Steps Left ");
                printBrawName((int)innerList[0].owner, 80, strCount, brawler[innerList[0].owner].name + ":");
                printBakuName((int)innerList[0].owner, (int)innerList[0].bakuganID, 82 + brawler[innerList[0].owner].name.Length, strCount, innerList[0].name);

                uint tempqueue = (uint)queueGame.FindIndex(x=>x.Item1 == innerList[0].owner);
                uint tempcurr = (NbrBraw - (uint)currBrawlerIndex +tempqueue)% NbrBraw;
                if (tempcurr == 0)
                    tempcurr = NbrBraw;
                printText(ConsoleColor.White, 113, strCount++, "" + tempcurr  );

                List<Bakugan> tempList = new List<Bakugan>(innerList);
                tempList.Sort((x, y) => x.team.CompareTo(y.team));
                for (int j = 1; j <= NbrTeam; j++)
                {
                    printText(ConsoleColor.Black, ConsoleColor.Gray, 80, strCount++, "Team " + (j) + "                        G power  ");


                    for (int i = 0; i < tempList.Count; i++)
                    {
                        if (j == tempList[i].team)
                        {
                            uint bakuOwner = tempList[i].owner;
                            uint bakuID = tempList[i].bakuganID;
                            string brawName = brawler[bakuOwner].name;
                            printBrawName((int)bakuOwner, 80, strCount, brawName + ":");
                            printBakuName((int)bakuOwner, (int)bakuID, 82 + brawName.Length, strCount, tempList[i].name);
                            printText(ConsoleColor.White, 112, strCount++, "" + tempList[i].g);
                            gTotal += tempList[i].g;
                            tempTeamCnt++;
                        }
                    }

                    printText(ConsoleColor.White, 80, strCount, "Total G power: ");
                    printText(ConsoleColor.White, 112, strCount++, "" + gTotal);
                    tempTeamCnt++;
                    gTotal = 0;
                }
                strCount++;
            }
            /*
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
            */
            return true;
        }

        public void nextPlayer()
        {
            currUsedAbil = 0;
            currUsedBaku = 0;
            currUsedGate = 0;

            currBrawlerIndex++;
            if(currBrawlerIndex >= NbrBraw)
                currBrawlerIndex = 0;

            currBrawler = queueGame[currBrawlerIndex].Item1;

            prevSelectMenu.Clear();
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



        public static void checkBattleRes(Field field)
        {
            foreach (List<Bakugan> innerList in field.battleLink)
            {
                if (innerList.Count == 0)
                    continue;
                if (innerList[0].owner == field.currBrawler)
                {
                    // Создаем словарь для хранения сумм g для каждой команды
                    Dictionary<uint, int> teamG = new Dictionary<uint, int>();

                    foreach (Bakugan bakugan in innerList)
                    {
                        // Подсчет суммы g для каждой команды
                        if (teamG.ContainsKey(bakugan.team))
                        {
                            teamG[bakugan.team] += bakugan.g;
                        }
                        else
                        {
                            teamG.Add(bakugan.team, bakugan.g);
                        }
                    }

                    int maxG = 0;
                    List<uint> winnerTeams = new List<uint>();

                    // Поиск команды с наибольшей суммой g
                    foreach (KeyValuePair<uint, int> team in teamG)
                    {
                        if (team.Value > maxG)
                        {
                            maxG = team.Value;
                            winnerTeams.Clear();
                            winnerTeams.Add(team.Key);
                        }
                        else if (team.Value == maxG)
                        {
                            winnerTeams.Add(team.Key);
                        }
                    }

                    // Вывод сообщений о результатах боя и удаление побежденных бакуганов
                    foreach (Bakugan bakugan in innerList.ToList())
                    {
                        // Если карта опустела окончательно сделать правильную очистку карты ворот
                        // Нужна модийикация списка


                        field.gate[bakugan.x, bakugan.y].bakugan.RemoveAll(x => x == bakugan);

                        if (field.gate[bakugan.x, bakugan.y].bakugan.Count == 0)
                            field.gate[bakugan.x, bakugan.y].removePlayerGate();


                        if (winnerTeams.Contains(bakugan.team))
                            field.brawler[bakugan.owner].bakugan[bakugan.bakuganID].state = BakuState.InInventory;
                        else
                        {
                            field.brawler[bakugan.owner].loseBakugan++;
                            field.brawler[bakugan.owner].bakugan[bakugan.bakuganID].state = BakuState.KnockedOut;
                        }

                        // Поиск и удаление бакуганов из BattleLink
                        foreach (List<Bakugan> innerList2 in field.battleLink)
                            innerList2.RemoveAll(x => x == bakugan);

                    }
                }
            }
            field.battleLink.RemoveAll(list => list.Count == 0);
            foreach (List<Bakugan> innerList in field.battleLink)
                innerList.RemoveAll(x => x == null);

        }



    }
}
