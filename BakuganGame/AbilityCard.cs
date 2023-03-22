using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BakuganGame
{
    internal class AbilityCard
    {
        uint teamID; // нужно чтобы карта отличала врагов от друзей
        uint brawlerID;// Ссылка на пользователя карты способности
        uint abilityID;// Нужно понимать порядок карты в инвентаре
        public uint abilityType { get; set; }

        public bool isActivated{ get; set; }
        public bool isUsed{ get; set; }

        Field field;//Логика карты опирается на ситуацию на поле боя

        public AbilityCard()
        {
            abilityType = 1;
            isActivated = false;
            isUsed = false;
}


        /// <summary>
        /// Определим карту, зная код: команды, игрока, бакугана, способности. И вид способности (см функцию activate)
        /// </summary>
        /// <param name="teamID">ID команды</param>
        /// <param name="brawlerID">ID бойца (Карта должна знать порядок своего владельца)</param>
        /// <param name="bakuganID">ID бакугана в инвентаре за которым закреплена способность</param>
        /// <param name="abilityID">ID способности в инвентаре (необходимо чтобы карта знала свой порядок в инвентаре)</param>
        /// <param name="abilityType">Тип карты способности (см функцию activate)</param>
        /// <returns>Возвращает true - если установить бакугана удалось</returns>
        public bool define(uint teamID, uint brawlerID, uint abilityID)
        {
            this.teamID = teamID;
            this.brawlerID = brawlerID;
            this.abilityID = abilityID;

            return true;
        }


        /// <summary>
        /// Активировать работу карты способности (Здесь описана основная логика всех карт)
        /// </summary>
        /// <returns>Возвращает true - если успешно завершилось</returns>
        public bool activate(uint bakuganID)
        {
            // В будущем важно проверять возможно ли впринципе активировать функцию. Вдруг на 
            // поле особые условия и нам нельзя активировать карту
            if (!isUsed && !isActivated && abilityType != 0)
            {
                switch (abilityType)
                {
                    case 1://Привабить 100 себе
                        field.brawler[brawlerID].bakugan[bakuganID].g += 100;
                        break;
                    case 2://Прибавить 100 себе и удалить 100 всем врагам
                        field.brawler[brawlerID].bakugan[bakuganID].g += 100;
                        for (int i = 0; i < field.NbrBraw; i++)
                            if (i != brawlerID && teamID != field.brawler[i].teamID)
                                field.brawler[i].bakugan[bakuganID].g -= 100;
                        break;
                }
                isActivated = true;
                isUsed = true;

                return true;
            }
            else
            {
                Console.WriteLine("Ability card message: ERROR in activate function");

                if (isActivated) 
                    Console.WriteLine("Why do you try to activate activated card?");
                if (isUsed)
                    Console.WriteLine("Why do you try to activate used card?");
                if (abilityType == 0)
                    Console.WriteLine("Your ability slot is empty");

                return false;
            }
        }


        /// <summary>
        /// Дективировать карту cпособности. Происходит при завершении очереди queueBattle
        /// </summary>
        /// <returns>Возвращает true - если успешно завершилось</returns>
        public void deactivate()
        {
            if (isActivated && isUsed)
            {
                isActivated = false;
                //isUsed = true;
            }
            else
            {
                Console.WriteLine("Ability card message: ERROR in deactivate function");
                
                if(!isActivated)
                    Console.WriteLine("Why do you try to deactivate deactivated card?");
                if(!isUsed)
                    Console.WriteLine("Why do you try to deactivate unused card?");
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

            return true;
        }
    }
}
