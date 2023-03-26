using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BakuganGame
{
    /*
    Ворота являются исчисляемой единицей поля. Каждая карта ворот:
        * Имеет координаты на поле
        * Количество бакуганов на ней
        * Ссылается на карту ворот (конкретный тип)
    */
    internal class Gate
    {
        
        public int x { get; private set; }
        public int y { get; private set; }

        public bool isBusy = false;// стоит чья-то карта
        public uint bakuganCount { get; set; }

        public uint gateOwner { get; private set; }// 0 - ничья, иначе айди бойца

        Field field; // Воротам важно знать что происходит вокруг
        public Bakugan[] bakugan; //ссылка на бакуганов установленных на карте
        public GateCard gateCard; //установленная карта ворот

        public Gate(uint NbrBaku, uint NbrTeam, uint NbrBraw, int x, int y, Field field) 
        {
            this.x = x;
            this.y = y;

            bakuganCount = 0;
            this.field = field;

            bakugan = new Bakugan[NbrBaku * NbrBraw];
            gateCard = new GateCard(field);



        }

        /// <summary>
        /// Принудительно поместить карту gateID от бойца brawlerID 
        /// </summary>
        /// <param name="brawlerID">ID игрока</param>
        /// <param name="gateID">ID карты ворот у игрока</param>
        /// <returns>Возвращает true - если установить бакугана удалось</returns>
        public bool placePlayerGate(uint brawlerID, uint gateID)
        {
            isBusy = true;
            gateOwner = brawlerID;
            gateCard = field.brawler[brawlerID].gateCard[gateID];
            field.brawler[brawlerID].gateCard[gateID].isPlaced = true;

            field.setAppLog($"Gate ({x},{y}) message: brawler {brawlerID} stand gate {gateID}");

            return true;
        }


        /// <summary>
        /// Удалить с ворот уже установленную карту
        /// </summary>
        /// <returns>Возвращает true - если удалось удалить карту</returns>
        public bool removePlayerGate()
        {
            if (isBusy)
            {
                field.setAppLog($"Gate ({x},{y}) message: gate erased, removed card from gate ");
                gateCard.removeFromField();//нужна причина почему мы удаляем: конец битвы, конец игры
                isBusy = false;
                bakuganCount = 0;

                gateCard = new GateCard(field);

                return true;
            }
            else
            {
                field.setAppLog($"Gate ({x},{y}) Error message: Error in removePlayerGate function");
                field.setAppLog($"Gate ({x},{y}) Error message: Why do you try to erase an empty gate?");

                return false;
            }
        }



        // Дебаг
        public void printInfo()
        {
            Console.WriteLine($"x position: {x}");
            Console.WriteLine($"y position: {y}");
            Console.WriteLine($"is busy: {isBusy}"); 
            Console.WriteLine($"count of bakugan in gate: {bakuganCount}");
        }

    }
}
