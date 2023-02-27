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
        
        static void Main(string[] args)
        {
            // Пункт 1: Вытащить из файла вводные данные
            uint NbrBaku = 1;/// - Количество доступных бакуганов
            uint NbrTeam = 2;/// - Количество команд
            uint NbrBraw = 2;/// - Количество бойцов

            // Пункт 2: Начать заполнять поле информацией по бойцам
            Field field = new Field(NbrBaku, NbrTeam, NbrBraw);
            field.setReferenceField();

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


            // Пункт 3: Запустить бесконечный цикл по каждому игроку (Очередь queueMain)

            // Начало игры (все ставят карту ворот)
            field.brawler[0].setGate(0, 0, 0);
            field.brawler[1].setGate(1, 0, 0);

            // Цикл боя типо
            field.brawler[0].throwBakugan(1, 0, 0);
            field.brawler[1].throwBakugan(1, 0, 0);

            field.brawler[0].useAbility(0);
            field.brawler[1].useAbility(0);

            field.brawler[0].useGate(0,0);

            // Результат битвы и закрытие битвы
            field.gate[1, 0].removePlayerGate();
            field.brawler[0].abilityCard[0].deactivate();
            field.brawler[1].abilityCard[0].deactivate();
            //field.gate[0, 0].removePlayerGate();

            Console.WriteLine(field.brawler[0].bakugan[0].g);
            Console.WriteLine(field.brawler[1].bakugan[0].g);



            // Пункт 4: Когда завершится бой, вывести выходные данные

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


/*
 //Удобные цвета
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Hello World!");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Hello World!");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Hello World!");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Hello World!");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Hello World!");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Hello World!");
            System.Threading.Thread.Sleep(2000);
 */