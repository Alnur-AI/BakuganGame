﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace BakuganGame
{

    internal class GateCard
    {
        uint teamID; // нужно чтобы карта отличала врагов от друзей
        uint brawlerID;// Ссылка на пользователя карты способности
        public uint gateID{ get; set; }// Нужно понимать порядок карты в инвентаре
        public uint gateType { get; set; }// 0 - пустая, остальное ссылка на базу

        bool isActivated = false;
        bool isUsed = false;
        public bool isPlaced{ get; set; }//true - in gate,
                                //false - in pocket

        Field field;//Логика карты опирается на ситуацию на поле боя

        public GateCard(Field field)
        {
            gateType = 1;
            this.field = field;
            isPlaced = false;
        }


        /// <summary>
        /// Определим карту, зная код: команды, игрока, бакугана, карт ворот. И вид ворот (см функцию activate)
        /// </summary>
        /// <param name="teamID">ID команды</param>
        /// <param name="brawlerID">ID бойца</param>
        /// <param name="gateID">ID карт ворот в инвентаре</param>
        /// <param name="gateType">Тип карты ворот (см функцию activate)</param>
        /// <returns>Возвращает true - если установить бакугана удалось</returns>
        public bool define(uint teamID, uint brawlerID, uint gateID)
        {
            this.teamID = teamID;
            this.brawlerID = brawlerID;
            this.gateID = gateID;

            return true;
        }



        /// <summary>
        /// Пометить карту как убранную с поля боя
        /// </summary>
        /// <returns>Возвращает true - если удалось успешно завершить</returns>
        public bool removeFromField()
        {
            if(isPlaced)
            {
                isPlaced = false;

                return true;
            }
            else
            {
                Console.WriteLine("Gate card message: ERROR in removeFromField function");
                Console.WriteLine("Can't remove card that placed in pocket");

                return false;
            }
        }


        /// <summary>
        /// Активировать работу карты ворот (Здесь описана основная логика всех карт)
        /// </summary>
        /// <returns>Возвращает true - если успешно завершилось</returns>
        public bool activate(int x, int y)
        {
            // В будущем важно проверять возможно ли впринципе активировать функцию. Вдруг на 
            // поле особые условия и нам нельзя активировать карту
            if (!isUsed && !isActivated && gateType != 0)
            {
                switch (gateType)
                {
                    case 1://Привабить 500 себе
                        field.brawler[brawlerID].bakugan[0].g += 500;
                        break;
                    case 2://Прибавить 500 себе и удалить 100 всем врагам
                        field.brawler[brawlerID].bakugan[0].g += 500;
                        for (int i = 0; i < field.NbrBraw; i++)
                            if (i != brawlerID && teamID != field.brawler[i].teamID)
                                field.brawler[i].bakugan[0].g -= 100;
                        break;
                }
                isActivated = true;
                isUsed = true;

                return true;
            }
            else
            {
                Console.WriteLine("Gate card message: ERROR in activate function");

                if (isActivated)
                    Console.WriteLine("Why do you try to activate activated card?");
                if (isUsed)
                    Console.WriteLine("Why do you try to activate used card?");
                if (gateType == 0)
                    Console.WriteLine("Your gate slot is empty");

                return false;
            }
        }


        /// <summary>
        /// Дективировать карту ворот. Происходит при завершении очереди queueBattle
        /// </summary>
        /// <returns>Возвращает true - если успешно завершилось</returns>
        public bool deactivate()
        {
            if (isActivated && isUsed)
            {
                isActivated = false;
                //isUsed = true;

                return true;
            }
            else
            {
                Console.WriteLine("Gate card message: ERROR in deactivate function");

                if (!isActivated)
                    Console.WriteLine("Why do you try to deactivate deactivated card?");
                if (!isUsed)
                    Console.WriteLine("Why do you try to deactivate unused card?");

                return false;
            }

        }

    }

}
