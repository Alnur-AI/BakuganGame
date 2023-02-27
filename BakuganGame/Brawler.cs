using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BakuganGame
{
    internal class Brawler
    {   
        public uint team { get; set; }   // 0 1 2 3 4
        public uint brawlerID { get; set; }   // уникальный номер в списке игроков
        public uint queueID { get; set; }// 0 1 2 3 4 5 6 очередь в игре

        bool inGame = false;
	    bool isActive = false;

        int  score;
        uint winBakugan;
        uint loseBakugan;

        public uint usedGateCard{ get; private set; }  // max NbrBaku
        public uint usedAbilityCard{ get; private set; }  // max NbrBaku


        Field field;//Боец ориентируется в пространстве
        public Bakugan[] bakugan;
        public AbilityCard[] abilityCard;// в будущем должен быть приватный
        public GateCard[] gateCard;// в будущем должен быть приватный

        public Brawler (uint NbrBaku, uint NbrTeam, uint NbrBraw, uint brawlerID)
        {
            bakugan = new Bakugan[NbrBaku];
            abilityCard = new AbilityCard[3*NbrBaku];
            gateCard = new GateCard[NbrBaku];

            usedGateCard = 0;
            usedAbilityCard = 0;

            this.brawlerID = brawlerID;
        }
        
        /// <summary>
        /// Боец инициирует бросок бакугана bakuganID на ворота (x,y)
        /// </summary>
        /// <param name="x">Координата ворот по x.</param>
        /// <param name="y">Координата ворот по y.</param>
        /// <param name="bakuganID">ID бакугана из арсенала, которого хотим отправить.</param>
        /// <returns>Возвращает true - если установить бакугана удалось</returns>
        public bool throwBakugan(int x, int y, int bakuganID)
        {
            if (0 <= x && x < field.NbrBraw && 
                0 <= y && y < field.NbrBaku &&
                field.gate[x, y].isBusy &&
                bakugan[bakuganID].state == 0)
            {
                uint gateBakuID = field.gate[x, y].bakuganCount;
                if (gateBakuID < field.NbrBraw * field.NbrBaku)
                {
                    field.gate[x, y].bakugan[gateBakuID] = bakugan[bakuganID];

                    bakugan[bakuganID].bakuganInGateID = (int)gateBakuID;
                    bakugan[bakuganID].state = 1;

                    field.gate[x, y].bakuganCount++;
                    
                    return true;
                }
                else
                {
                    Console.WriteLine("Brawler message: ERROR in throwBakugan function");
                    Console.WriteLine("Gate is overloaded");

                    return false;
                }
            }
            else
            {
                Console.WriteLine("Brawler message: ERROR in throwBakugan function");

                if (0 > x)
                    Console.WriteLine("x less that 0");
                
                if (x >= field.NbrBraw)
                    Console.WriteLine("x bigger that field.NbrBraw");
                
                if (0 > y)
                    Console.WriteLine("y less that 0");
                
                if (y >= field.NbrBaku)
                    Console.WriteLine("y bigger that field.NbrBaku");

                if (!field.gate[x, y].isBusy)
                    Console.WriteLine("Brawler is trying to brawl in empty gate");

                if (bakugan[bakuganID].state != 0)
                    Console.WriteLine("bakugan is not in pocket");

                return false;
            }
                        
        }
        

        /// <summary>
        /// Боец активирует карту способности из арсенала под номером abilityID
        /// </summary>
        /// <param name="abilityID">Номер карты в арсенале</param>
        /// <returns>Возвращает true - если удалось активировать споосбность</returns>
        public bool useAbility(uint abilityID)
        {
            
            if (field.NbrBaku >= usedAbilityCard)
            {
                bool ableActivate = abilityCard[abilityID].activate();
                
                if (ableActivate)
                {
                    usedAbilityCard += 1;
                    return true;
                }
                return false;
            }
            else
            {
                Console.WriteLine("Brawler message: ERROR in useAbility function");
                Console.WriteLine("Used maximum of available cards");

                return false;
            }
            
        }
        

        /// <summary>
        /// Активирует карту с позицией (x,y)
        /// </summary>
        /// <param name="x">Координата ворот по x.</param>
        /// <param name="y">Координата ворот по y.</param>
        /// <returns>Возвращает true - если удалось активировать карту</returns>
        public bool useGate(int x, int y)
        {
            if (0 <= x && x < field.NbrBraw &&
                0 <= y && y < field.NbrBaku &&
                field.gate[x, y].isBusy &&
                field.gate[x,y].gateOwner == this.brawlerID)
            {
                if(field.NbrBaku >= usedGateCard)
                    return field.gate[x, y].gateCard.activate();
                
                else
                {
                    Console.WriteLine("Brawler message: ERROR in useGate function");
                    Console.WriteLine($"Number of used cards: {usedGateCard}");
                    Console.WriteLine("Used maximum of available cards");

                    return false;
                }
            }
            else
            {
                Console.WriteLine("Brawler message: ERROR in throwBakugan function");

                if (0 > x)
                    Console.WriteLine("x less that 0");

                if (x >= field.NbrBraw)
                    Console.WriteLine("x bigger that field.NbrBraw");

                if (0 > y)
                    Console.WriteLine("y less that 0");

                if (y >= field.NbrBaku)
                    Console.WriteLine("y bigger that field.NbrBaku");

                if (!field.gate[x, y].isBusy)
                    Console.WriteLine("Brawler is trying to brawl in empty gate");

                if(field.gate[x, y].gateOwner != this.brawlerID)
                    Console.WriteLine("this brawler is not the owner of selected gate");

                return false;
            }
        }


        /// <summary>
        /// Размещает карту ворот gateID с координатами (x,y)
        /// </summary>
        /// <param name="x">Координата ворот по x.</param>
        /// <param name="y">Координата ворот по y.</param>
        /// <param name="gateID">Номер карты ворот в инвентаре.</param>
        /// <returns>Возвращает true - если удалось разместить карту</returns>
        public bool setGate(int x, int y, uint gateID)
        {
            //Console.WriteLine(field.gate[x, y].isBusy);
            if (0 <= x && x < field.NbrBraw && 
                0 <= y && y < field.NbrBaku &&
                !field.gate[x, y].isBusy)
            {
                
                bool ableActivate = field.gate[x, y].placePlayerGate(brawlerID, gateID);

                if (ableActivate)
                {
                    usedGateCard += 1;
                    return true;
                }
                return false;
            }
            //return field.NbrBaku > (usedGateCard += 1) ? true : false;
            else
            {
                Console.WriteLine("Brawler message: ERROR in setGate function");

                if (0 > x)
                    Console.WriteLine("x less that 0");

                if (x >= field.NbrBraw)
                    Console.WriteLine("x bigger that field.NbrBraw");

                if (0 > y)
                    Console.WriteLine("y less that 0");

                if (y >= field.NbrBaku)
                    Console.WriteLine("y bigger that field.NbrBaku");

                if (field.gate[x, y].isBusy)
                    Console.WriteLine("Brawler is trying to set on non empty gate");

                return false;
            }
        }


        /// <summary>
        /// Бойцу важно знать обстановку на поле. Добавляем ссылку field на всё поле
        /// </summary>
        /// <param name="field">Ссылка на игровое поле</param>
        /// <returns>Возвращает true - если удалось правильно заполнить поле</returns>
        public bool setField(Field field)
        {
            this.field = field;

            for (int i = 0; i < 3 * field.NbrBaku; i++)
            {
                abilityCard[i] = new AbilityCard();
                abilityCard[i].setField(field);
            }
            for (int i = 0; i < field.NbrBaku; i++)
            {
                gateCard[i] = new GateCard();
                gateCard[i].setField(field);
            }
            for (int i = 0; i < field.NbrBaku; i++)
            {
                bakugan[i] = new Bakugan();
                bakugan[i].define(queueID, team);
            }

            return true;
        }

    }
}
