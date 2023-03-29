using BakuganGame;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace BakuganGame
{
    public enum BakuState
    {
        DoesntExist  = -1,
        InInventory,
        OnGate,
        InBattle,
        KnockedOut,
        Killed
    }
    /*
     Цель: создать универсальный алгоритм ведения боя при любых условиях
     Входные данные (из конфига): 
          * NbrBaku - Количество доступных бакуганов
          * NbrTeam - Количество команд 
          * NbrBraw - Количество бойцов (независимо от команды)
          *           Бойцы с фиксированным арсеналом
          *           и принадлежность к команде
     Константы:
          * MaxAvGtInGame = NbrBaku - Количество доступных карт ворот у бойца за игру
          * MaxAvAbInGame = NbrBaku - Количество доступных карт способностей у бойца за игру
          * MaxAvCrd = 3*NbrBaku - Доступные в арсенале карты способностей (ссылаются по ID существующего справочника)
          * MaxFieldSize = NbrBaku*NbrBraw - Максимальный размер поля игры (количество ворот)
          * MaxGateSize = NbrBaku*NbrBraw - Максимальная вместимость карты ворот
     Выходные данные:
          * Список победивших игроков и начисляемых очков
          * Победившая команда
          * Количество оставшихся у победителей бакуганов 
    */
    class Program
    {

        public static void waitTillPressed(ref ConsoleKeyInfo key)
        {
            while (!Console.KeyAvailable)
            {
                // Ожидание нажатия клавиши
            }

            key = Console.ReadKey(true);
        }
        public static void Exit(Field field)
        {
            for (int i = 0; i < field.NbrBraw; i++)
                for (int j = 0; j < 3 * field.NbrBaku; j++)
                    if (field.brawler[i].abilityCard[j].isActivated)
                        field.brawler[i].abilityCard[j].deactivate();

            for (int i = 0; i < field.NbrBraw; i++)
                for (int j = 0; j < field.NbrBaku; j++)
                    if (field.gate[i, j].isBusy)
                        field.gate[i, j].removePlayerGate();

            field.printAppLog();

            Console.Clear();
            Console.WriteLine("Permanent exit thougth the ESC. All battle information saved in AppLot.txt");

        }


        static void Main(string[] args)
        {
            // Пункт 1: Вытащить из файла вводные данные
            //          Начать заполнять поле информацией по бойцам
            Field field = new Field();
            field.SortBrawlers();


            ConsoleKeyInfo key =  Console.ReadKey(true);
            while (true)// Пока не завершиться программа
            {
                waitTillPressed(ref key);

                // Управление картой
                field.controlField(key);//WASD 
                field.controlInGame(key); 

                // Выход из игры
                if (key.Key == ConsoleKey.Escape)
                {
                    Exit(field);

                    Console.Clear();
                    Console.WriteLine("Permanent exit thougth the ESC. All battle information saved in AppLot.txt");
                    break;
                }

                uint[] teamKilledCount = new uint[field.NbrTeam];
                int[] teamBrawCount = new int[field.NbrTeam];

                for (int i = 0; i < field.NbrBraw; i++)
                    for (int j = 1; j <= field.NbrTeam; j++)
                    {
                        if (field.brawler[i].teamID == j)
                        {
                            teamKilledCount[j - 1] += field.brawler[i].loseBakugan;
                            teamBrawCount[j - 1]++;
                        }
                        
                    }


                // ДОРАБОТАТЬ 
                int winTeam = 0;
                int winCont = 0;
                for (int i = 0; i < field.NbrTeam; i++)
                {
                    if (teamKilledCount[i] != field.NbrBaku*teamBrawCount[i])
                    {
                        winTeam = i;
                    }
                    else
                    {
                        winCont++;
                    }
                }


                if (winCont == field.NbrTeam - 1)
                {
                    Exit(field);
                    Console.WriteLine($"Winner team: {winTeam}");
                    break;
                }



                // Пункт 2.1: Запустить бесконечный цикл по каждому игроку (Очередь queueMain)
                /*
                if (key.Key == ConsoleKey.Q)
                    field.brawler[0].useAbility(0,0);
                */


                //field.brawler[0].useGate((int)field.currGateX, (int)field.currGateY);

                // Пункт 2.2: Если завершится локальный бой, вывести выходные данные



                // Пункт 2.3: Выводим изображение

                Console.Clear();

                field.drawField();
                field.drawFieldInfo();
                field.drawGateInfo();
                field.drawControl();

                field.drawBattleLink();

                /*
                Console.SetCursorPosition(0,23);
                for (int i = 0; i < field.NbrTeam; i++)
                    Console.Write($"{teamKilledCount[i]} ");

                Console.SetCursorPosition(0, 24);
                for (int i = 0; i < field.NbrTeam; i++)
                    Console.Write($"{teamBrawCount[i]*field.NbrBaku} ");

                Console.SetCursorPosition(0, 25);
                    Console.Write($"Curr Gate lim: {field.currUsedGate}  Curr Baku lim: {field.currUsedBaku}");
                */
            }
            
        }
    }
}





/*
 * 
 * Секция устаревшего кода 

for (uint i = 0; i < field.NbrBraw; i++)
                {
                    for (uint j = 0; j < field.NbrBaku; j++)
                    {
                        field.setAppLog(field.brawler[i].brawlerID + " " +
                            field.brawler[i].teamID +
                            field.brawler[i].bakugan[j].bakuganID);
                    }
                    for (uint j = 0; j < 3*field.NbrBaku; j++)
                    {
                        field.setAppLog(field.brawler[i].brawlerID + " " +
                            field.brawler[i].abilityCard[j].abilityID);
                    }
                    for (uint j = 0; j < field.NbrBaku; j++)
                    {
                        field.setAppLog(field.brawler[i].brawlerID + " " +
                            field.brawler[i].gateCard[j].gateID);
                    }

                }
  
public static bool drawDemo(Field field)
{
    
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Pyrus");
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Haos");
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine("Aquos");
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine("Subterra");
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.WriteLine("Darkus");
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("Ventus");
    Console.ForegroundColor = ConsoleColor.White;
    


    // Field (done)
    Console.SetCursorPosition(13, 0);
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write("Field");

    Console.SetCursorPosition(13, 2);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("###");
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("***");

    Console.SetCursorPosition(13, 3);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("#");
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write("#");
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("#");
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("***");

    Console.SetCursorPosition(13, 4);
    Console.Write("******");

    Console.SetCursorPosition(13, 5);
    Console.Write("******");


    //Field Information (done)
    Console.SetCursorPosition(13, 10);
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write("Field Info");

    Console.SetCursorPosition(0, 11);
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("---------------------------------------");

    Console.SetCursorPosition(0, 12);
    Console.Write("Team 1    Bakugan     Ability     Gate");

    Console.SetCursorPosition(0, 13);
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write("Dan");
    Console.SetCursorPosition(12, 13);
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("***          ");
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("3");
    Console.Write("         3");

    Console.SetCursorPosition(0, 14);
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.Write("Marucho");
    Console.SetCursorPosition(12, 14);
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("***          ");
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("3");
    Console.Write("         3");

    Console.SetCursorPosition(0, 15);
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("Runo");
    Console.SetCursorPosition(12, 15);
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("***          ");
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("3");
    Console.Write("         3");


    Console.SetCursorPosition(0, 17);
    Console.Write("Team 2    Bakugan     Ability     Gate");

    Console.SetCursorPosition(0, 18);
    Console.ForegroundColor = ConsoleColor.DarkMagenta;
    Console.Write("Masquerade");
    Console.SetCursorPosition(12, 18);
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("***          ");
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("3");
    Console.Write("         3");

    Console.SetCursorPosition(0, 19);
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.Write("Julie");
    Console.SetCursorPosition(12, 19);
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("***          ");
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("3");
    Console.Write("         3");

    Console.SetCursorPosition(0, 20);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("Shuun");
    Console.SetCursorPosition(12, 20);
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("***          ");
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("3");
    Console.Write("         3");


    //Gate Info (done)
    Console.SetCursorPosition(55, 10);
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write("Gate Info");

    Console.SetCursorPosition(40, 11);
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("---------------------------------------");

    Console.SetCursorPosition(40, 12);
    Console.Write("Team 1                        G power         ");

    Console.SetCursorPosition(40, 13);
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write("Dan: Dragonoid");
    Console.SetCursorPosition(72, 13);
    Console.Write("340G");

    Console.SetCursorPosition(40, 14);
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write("Dan: Reaper");
    Console.SetCursorPosition(72, 14);
    Console.Write("300G");

    Console.SetCursorPosition(40, 15);
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("Runo: Reaper");
    Console.SetCursorPosition(72, 15);
    Console.Write("300G");

    Console.SetCursorPosition(40, 16);
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("Total G power: ");
    Console.SetCursorPosition(72, 16);
    Console.Write("940G");

    Console.SetCursorPosition(40, 18);
    Console.Write("Team 2                        G power         ");

    Console.SetCursorPosition(40, 19);
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.Write("Masquerade: Hydranoid");
    Console.SetCursorPosition(72, 19);
    Console.Write("340G");

    Console.SetCursorPosition(40, 20);
    Console.ForegroundColor = ConsoleColor.Magenta;
    Console.Write("Masquerade: Reaper");
    Console.SetCursorPosition(72, 20);
    Console.Write("300G");

    Console.SetCursorPosition(40, 21);
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("Shuun: Skyress");
    Console.SetCursorPosition(72, 21);
    Console.Write("300G");

    Console.SetCursorPosition(40, 22);
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("Total G power: ");
    Console.SetCursorPosition(72, 22);
    Console.Write("940G");

    //Control
    Console.SetCursorPosition(56, 0);
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write("Control");

    Console.SetCursorPosition(40, 1);
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("1) Set Gate Card");

    Console.SetCursorPosition(40, 2);
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("2) Set Bakugan");

    Console.SetCursorPosition(40, 3);
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("3) Activate Ability Card");

    Console.SetCursorPosition(40, 4);
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("4) Open Gate Card");

    Console.SetCursorPosition(40, 5);
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("5) Change Bakugan attribute");

    Console.SetCursorPosition(40, 6);
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("6) Remove Doom Card");

    Console.SetCursorPosition(40, 7);
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("7) Miss turn");




    // Console Information
    Console.SetCursorPosition(90, 0);
    Console.ForegroundColor = ConsoleColor.Red;
    Console.Write("Console Information");

    Console.SetCursorPosition(83, 1);
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("1: Dan SET GATE ON (x,y)");

    Console.SetCursorPosition(83, 2);
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("2: Dan SET BAKUGAN Dragonoid ON (x,y)");

    Console.SetCursorPosition(83, 3);
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("3: Dan OPEN GATE (x,y)");

    Console.SetCursorPosition(83, 4);
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("4: Dan OPEN ABILITY 1");

    Console.SetCursorPosition(83, 5);
    Console.ForegroundColor = ConsoleColor.White;
    Console.Write("5: Dan END TURN");


    Console.ForegroundColor = ConsoleColor.White;
    Console.SetCursorPosition(0, 23);

    return true;
}
*/