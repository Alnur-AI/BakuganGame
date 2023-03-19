using System;
using System.Net;
using System.Runtime.InteropServices;

namespace BakuganGame
{
    /*
     Цель: создать универсальный алгоритм ведения боя при любых условиях
     Входные данные (из конфига): 
          * NbrBaku - Количество доступных бакуганов
          * NbrTeam - Количество команд 
          * NbrBraw - Количество бойцов
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
        public static bool defineBrawlers(Field field)
        {
            field.brawler[0].team = 1;
            field.brawler[0].bakugan[0].setBakugan(true, false, false, false, false, false, 300);
            field.brawler[0].abilityCard[0].define(1, 0, 0, 0, 1);
            field.brawler[0].abilityCard[1].define(1, 0, 1, 1, 2);
            field.brawler[0].abilityCard[2].define(1, 0, 2, 2, 3);
            field.brawler[0].gateCard[0].define(1, 0, 0, 0, 1);

            field.brawler[1].team = 2;
            field.brawler[1].bakugan[0].setBakugan(false, false, true, false, false, false, 330);
            field.brawler[1].abilityCard[0].define(2, 1, 0, 0, 1);
            field.brawler[1].abilityCard[1].define(2, 1, 1, 1, 2);
            field.brawler[1].abilityCard[2].define(2, 1, 2, 2, 3);
            field.brawler[1].gateCard[0].define(2, 1, 0, 0, 2);

            // Завершаем формировать бойцов
            return true;
        }

        public static bool battle(Field field)
        {
            // Начало игры (все ставят карту ворот)
            field.brawler[0].setGate(0, 0, 0);
            field.brawler[1].setGate(1, 0, 0);


            // Цикл боя типо
            field.brawler[0].throwBakugan(1, 0, 0);
            field.brawler[1].throwBakugan(1, 0, 0);

            field.brawler[0].useAbility(0);
            field.brawler[1].useAbility(0);

            field.brawler[0].useGate(0, 0);

            // Выход из боя
            return true;
        }

        public static bool endBattle(Field field)
        {
            // Результат битвы и закрытие битвы
            field.gate[1, 0].removePlayerGate();
            field.brawler[0].abilityCard[0].deactivate();
            field.brawler[1].abilityCard[0].deactivate();
            //field.gate[0, 0].removePlayerGate();

            return true;
        }

        public static bool drawDemo(Field field)
        {
            /*
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
            */


            // Field
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


            //Field Information
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


            //Gate Info
            //Field Information
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
        
        static void Main(string[] args)
        {
            // Пункт 1: Вытащить из файла вводные данные
            uint NbrBaku = 3;/// - Количество доступных бакуганов
            uint NbrTeam = 2;/// - Количество команд
            uint NbrBraw = 2;/// - Количество бойцов (независимо от команды)


            // Пункт 2: Начать заполнять поле информацией по бойцам
            Field field = new Field(NbrBaku, NbrTeam, NbrBraw);
            field.setReferenceField();

            defineBrawlers(field);


            // Пункт 3.1: Запустить бесконечный цикл по каждому игроку (Очередь queueMain)
            battle(field);


            // Пункт 3.2: Когда завершится бой, вывести выходные данные
            endBattle(field);

            // Пункт 3.3: Выводим изображение
            drawDemo(field);


        }

        // перед удалением изменить опцию "Allow unsafe code"
        /*
        static unsafe void a(Field field)
        {
            Field* a = &field;
            Console.WriteLine("Address field: {0}", (int)a);
        }
        */
    }
}
