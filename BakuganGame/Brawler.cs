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
        public string name { get; set; }
        public ulong BID{ get; set; }
        public ulong rank { get; set; }
        public double money { get; set; }
        public uint teamID { get; set; }   // 0 1 2 3 4
        public uint brawlerID { get; set; }   // уникальный номер в списке игроков
        public uint queueID { get; set; }// 0 1 2 3 4 5 6 очередь в игре

        bool inGame = false;// Все еще играет?

        public bool avDoom { get; set; }// Карта смерти в наличии?
        public bool setDoom { get; set; }// Карта смерти установлена?

        uint winBakugan = 0;// выиграл бакугана
        public uint loseBakugan{ get; set; }// потерял своих бакуганов

        public uint usedGateCard{ get; private set; }  // max NbrBaku
        public uint usedAbilityCard{ get; private set; }  // max NbrBaku


        Field field;//Боец ориентируется в пространстве
        public Bakugan[] bakugan;
        public AbilityCard[] abilityCard { get; private set; } // в будущем должен быть приватный
        public GateCard[] gateCard { get; private set; } // в будущем должен быть приватный

        public Brawler (uint NbrBaku, uint NbrTeam, uint NbrBraw, uint brawlerID, Field field)
        {
            setDoom = false;
            this.field = field;

            bakugan = new Bakugan[NbrBaku];
            for (int i = 0; i < NbrBaku; i++)
                bakugan[i] = new Bakugan();

            abilityCard = new AbilityCard[3*NbrBaku];
            for (int i = 0; i < 3*NbrBaku; i++)
                abilityCard[i] = new AbilityCard(field);

            gateCard = new GateCard[NbrBaku];
            for (int i = 0; i < NbrBaku; i++)
                gateCard[i] = new GateCard(field);

            usedGateCard = 0;
            usedAbilityCard = 0;

            this.brawlerID = brawlerID;
        }
        
        /// <summary>
        /// Боец насильно инициирует бросок бакугана bakuganID на ворота (x,y)
        /// </summary>
        /// <param name="x">Координата ворот по x.</param>
        /// <param name="y">Координата ворот по y.</param>
        /// <param name="bakuganID">ID бакугана из арсенала, которого хотим отправить.</param>
        /// <returns>Возвращает true - если установить бакугана удалось</returns>
        public bool ForceThrowBakugan(int x, int y, int bakuganID)
        {
            int gateBakuID = field.gate[x, y].bakugan.Count;
            if 
            (
                0 <= x && x < field.NbrBraw && 
                0 <= y && y < field.NbrBaku &&
                field.gate[x, y].isBusy     && 
                bakugan[bakuganID].state == BakuState.InInventory
            )
            {
                if (gateBakuID < field.NbrBraw * field.NbrBaku)
                {

                    // BattleLink
                    bool isYourTeam = false;// Есть ли бакуганы из твоей команды
                    int enemyCount = 0;// Количество вражеских бакуганов
                    int friendCount = 0;// Количество дружеского бакугана
                    bool bakuInBattleLink = false;// Есть ли бакуган в списке battleLink

                    // Проверка является ли карта дружеской или вражеской
                    for (int i = 0; i < gateBakuID; i++)
                    {
                        if (field.gate[x, y].bakugan[i].team != bakugan[bakuganID].team)
                            enemyCount++;
                        if (field.gate[x, y].bakugan[i].team == bakugan[bakuganID].team)
                            friendCount++;

                        isYourTeam = isYourTeam || field.gate[x, y].bakugan[i].team == bakugan[bakuganID].team;
                    }

                    // Ведем поиск каждого бакугана в списке BattleLink 
                    // Когда будет готов код для работы карт (изменения battleLink)
                    // Частично изменим код в случае если будут одни союзники на карте
                    // НЕ ПРОДУМАНО ЕСЛИ НА КАРТЕ ОДИН ИЛИ НЕСКОЛЬКО СОЮЗНИКОВ 

                    bool myBakuInList = false;
                    bool enBakuFound = false;
                    // Для всех бакуганов на непустой карте
                    for (int i = 0; i < gateBakuID; i++)
                    {
                        int countBL = field.battleLink.Count;

                        // Для всех вражеских бакуганов
                        if (field.gate[x, y].bakugan[i].team != bakugan[bakuganID].team && field.gate[x, y].bakugan[i].state != BakuState.DoesntExist)
                        {

                            // Ищем вражеского бакугана по всему BattleLink
                            foreach (List<Bakugan> innerList in field.battleLink)
                            {
                                foreach (Bakugan obj in Enumerable.Reverse(innerList))
                                //for(int obj =0; obj < innerList.Count; obj++)
                                {
                                    // Мы уже есть в текущем подсписке. Дубликаты нашего бакугана нам не нужны 
                                    if (obj == bakugan[bakuganID])
                                    {
                                        myBakuInList = true;
                                        //break;
                                    }
                                    // Вражеский бакуган уже учавствует в битве, присоединимся к нему
                                    if (obj == field.gate[x, y].bakugan[i] )
                                    {
                                        enBakuFound = true;

                                        if (!myBakuInList && obj.state != BakuState.DoesntExist)
                                            innerList.Add(bakugan[bakuganID]);
                                        
                                        break;
                                    }
                                }
                            }

                            // Для пустого battleLink
                            // При отсутсвии бакугана в списке battleLink сначала добавляем себя, затем другого бакугана
                            if ((!enBakuFound || countBL == 0) && !myBakuInList)
                            {
                                field.battleLink.Add(new List<Bakugan>());
                                countBL = field.battleLink.Count;
                                field.battleLink[countBL - 1].Add(bakugan[bakuganID]);
                                field.battleLink[countBL - 1].Add(field.gate[x, y].bakugan[i]);
                            }
                            // Если мы уже нашли своего бакугана но не добавили нашего врага в список
                            if (myBakuInList && !enBakuFound)
                            {
                                field.battleLink[countBL - 1].Add(field.gate[x, y].bakugan[i]);
                            }
                        }
                    }


                    // Добавляем нашего бакугана на поле
                    field.gate[x, y].bakugan.Add(bakugan[bakuganID]);
                    //field.gate[x, y].bakugan[(int)gateBakuID] = bakugan[bakuganID];

                    bakugan[bakuganID].x = x;
                    bakugan[bakuganID].y = y;
                    bakugan[bakuganID].bakuganInGateID = (int)gateBakuID;
                    bakugan[bakuganID].state = BakuState.OnGate;

                    //field.gate[x, y].bakuganCount++;

                    field.setAppLog($"Brawler {brawlerID} message: bakugan {bakuganID} placed in gate ({x}, {y})");


                    return true;
                }
                else
                {
                    field.setAppLog($"Brawler {brawlerID} Error message: ERROR in throwBakugan function");
                    field.setAppLog($"Brawler {brawlerID} Error message: Gate is overloaded");

                    return false;
                }
            }
            else
            {
                field.setAppLog($"Brawler {brawlerID} Error message: ERROR in throwBakugan function");

                if (0 > x)
                    field.setAppLog($"Brawler {brawlerID} Error message: x = {x} less that 0");
                
                if (x >= field.NbrBraw)
                    field.setAppLog($"Brawler {brawlerID} Error message: x = {x} bigger that field.NbrBraw");
                
                if (0 > y)
                    field.setAppLog($"Brawler {brawlerID} Error message: y = {y} less that 0");
                
                if (y >= field.NbrBaku)
                    field.setAppLog($"Brawler {brawlerID} Error message: y = {y} bigger that field.NbrBaku");

                if (!field.gate[x, y].isBusy)
                    field.setAppLog($"Brawler {brawlerID} Error message: Brawler is trying to brawl in empty gate ({x},{y})");

                if (bakugan[bakuganID].state == BakuState.OnGate)
                    field.setAppLog($"Brawler {brawlerID} Error message: bakugan is already on other gate");

                if (bakugan[bakuganID].state == BakuState.InBattle)
                    field.setAppLog($"Brawler {brawlerID} Error message: bakugan is already in battle");

                if (bakugan[bakuganID].state == BakuState.DoesntExist)
                    field.setAppLog($"Brawler {brawlerID} Error message: bakugan does not exists");

                if (bakugan[bakuganID].state == BakuState.KnockedOut)
                    field.setAppLog($"Brawler {brawlerID} Error message: bakugan is knocked out");

                if (bakugan[bakuganID].state == BakuState.Killed)
                    field.setAppLog($"Brawler {brawlerID} Error message: bakugan is killed");

                return false;
            }
                        
        }


        /// <summary>
        /// Боец активирует карту способности из арсенала под номером abilityID
        /// </summary>
        /// <param name="abilityID">Номер карты в арсенале</param>
        /// <param name="bakuganID">Бакуган, на котором будет использована карта способности</param>
        /// <returns>Возвращает true - если удалось активировать споосбность</returns>
        public bool useAbility(uint abilityID,uint bakuganID)
        {
            if (field.NbrBaku >= usedAbilityCard)
            {
                bool ableActivate = abilityCard[abilityID].activate(bakuganID);
                
                if (ableActivate)
                {
                    usedAbilityCard += 1;
                    return true;
                }
                field.setAppLog($"Brawler {brawlerID} message: ability {abilityID} activated");

                return false;
            }
            else
            {
                field.setAppLog($"Brawler {brawlerID} Error message: ERROR in useAbility function");
                field.setAppLog($"Brawler {brawlerID} Error message: Used maximum of available cards");

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
                {
                    field.setAppLog($"Brawler {brawlerID} message: gate ({x},{y}) is activated");
                    return field.gate[x, y].gateCard.activate(x,y);
                }
                    
                
                else
                {
                    field.setAppLog($"Brawler {brawlerID} Error message: ERROR in useGate function");
                    field.setAppLog($"Brawler {brawlerID} Error message: Number of used cards: {usedGateCard}");
                    field.setAppLog($"Brawler {brawlerID} Error message: Used maximum of available cards");

                    return false;
                }
            }
            else
            {
                field.setAppLog($"Brawler {brawlerID} Error message: ERROR in useGate function");

                if (0 > x)
                    field.setAppLog($"Brawler {brawlerID} Error message: x = {x} less that 0");

                if (x >= field.NbrBraw)
                    field.setAppLog($"Brawler {brawlerID} Error message: x = {x} bigger that field.NbrBraw");

                if (0 > y)
                    field.setAppLog($"Brawler {brawlerID} Error message: y = {y} less that 0");

                if (y >= field.NbrBaku)
                    field.setAppLog($"Brawler {brawlerID} Error message: y = {y} bigger that field.NbrBaku");

                if (!field.gate[x, y].isBusy)
                    field.setAppLog($"Brawler {brawlerID} Error message: Brawler {brawlerID} is trying to brawl on empty gate ({x},{y})");

                if(field.gate[x, y].gateOwner != this.brawlerID)
                    field.setAppLog($"Brawler {brawlerID} Error message: this brawler {brawlerID} is not the owner of selected gate (true owner is {field.gate[x, y].gateOwner})");

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
                !field.gate[x, y].isBusy && !gateCard[gateID].isPlaced)
            {
                
                bool ableActivate = field.gate[x, y].placePlayerGate(brawlerID, gateID);

                if (ableActivate)
                {
                    field.setAppLog($"Brawler {brawlerID} message: gate card {gateID} is placed on gate ({x},{y})");
                    usedGateCard += 1;
                }
                return ableActivate;
            }
            //return field.NbrBaku > (usedGateCard += 1) ? true : false;
            else
            {
                field.setAppLog($"Brawler {brawlerID} Error message: ERROR in setGate function");

                if (0 > x)
                    field.setAppLog($"Brawler {brawlerID} Error message: x = {x} less that 0");

                if (x >= field.NbrBraw)
                    field.setAppLog($"Brawler {brawlerID} Error message: x = {x} bigger that field.NbrBraw");

                if (0 > y)
                    field.setAppLog($"Brawler {brawlerID} Error message: y = {y} less that 0");

                if (y >= field.NbrBaku)
                    field.setAppLog($"Brawler {brawlerID} Error message: y = {y} bigger that field.NbrBaku");

                if (field.gate[x, y].isBusy)
                    field.setAppLog($"Brawler {brawlerID} Error message: Brawler is trying to set on non empty gate");

                if (gateCard[gateID].isPlaced)
                    field.setAppLog($"Brawler {brawlerID} Error message: Brawler trying to set used gateCard");
                

                return false;
            }
        }

        /// <summary>
        /// Установить карту смерти
        /// </summary>
        /// <returns>Возвращает true - если удалось правильно заполнить поле</returns>
        public bool setDoomCard()
        {
            if (avDoom && !setDoom )
            {
                setDoom = true;
                field.setAppLog($"Brawler {brawlerID} message: brawler set doom card");
                return true;
            }
            else 
            {
                field.setAppLog($"Brawler {brawlerID} Error message: ERROR in setDoomCard");
                if(!avDoom)
                    field.setAppLog($"Brawler {brawlerID} Error message: player don't have a doom card");
                if(setDoom)
                    field.setAppLog($"Brawler {brawlerID} Error message: player set doom card already");
                return false; 
            }
        }
        
        /// <summary>
        /// Убрать карту смерти
        /// </summary>
        /// <returns>Возвращает true - если удалось правильно заполнить поле</returns>
        public bool removeDoomCard()
        {
            if (avDoom && setDoom)
            {
                setDoom = false;
                field.setAppLog($"Brawler {brawlerID} message: brawler remove doom card");
                return true;
            }
            else
            {
                field.setAppLog($"Brawler {brawlerID} Error message: ERROR in setDoomCard");
                if (!avDoom)
                    field.setAppLog($"Brawler {brawlerID} Error message: player don't have a doom card");
                if (!setDoom)
                    field.setAppLog($"Brawler {brawlerID} Error message: player removed doom card already");
                return false;
            }
        }

    }
}
