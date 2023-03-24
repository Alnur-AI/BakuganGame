//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

using System.Reflection.Metadata.Ecma335;

namespace BakuganGame
{
    internal class Bakugan
    {
        //ID in fiel
        public string name { get; set; }
        public uint bakuganID { get; set; }
        public int bakuganInGateID { get; set; }//ID на вороте (-1 значит что бакуган в инвентаре)
        int x;
        int y;

        public BakuState state { get; set; }
                      //-1 - does not exist
                      //0 - in pocket,
                      //1 - in card,
                      //2 - in battle,
                      //3 - knocked out
                      //4 - killed
        

        public uint team  { get; private set; }
        public uint owner { get; private set; }//BrawlerID

        //G level
        public int g { get; set; }// в локальном бою
        public int gGame { get; set; }// изменен начальный уровень на время игры
        public int gGlobal { get; set; }// изменен начальный уровень навсегда

        //Atribute
        public bool isPyrus { get;  set; }
        public bool isAquos { get;  set; }
        public bool isDarkus { get;  set; }
        public bool isVentus { get;  set; }
        public bool isSubterra { get;  set; }
        public bool isHaos { get;  set; }

        public Bakugan()    
        {
            state = BakuState.DoesntExist;
            bakuganInGateID = -1;
            gGlobal = gGame = this.g = 0;
        }


        /// <summary>
        /// Определим бакугана, зная код: команды, игрока
        /// </summary>
        /// <param name="team">ID команды</param>
        /// <param name="brawlerID">ID бойца</param>
        /// <returns>Возвращает true - если установить бакугана удалось</returns>
        public bool define(uint teamID,uint brawlerID,uint bakuganID)
        {
            owner = brawlerID;
            this.bakuganID = bakuganID;
            team = teamID;
            return true;
        }


        /// <summary>
        /// Задать свойства бакугана: тип стихии и количество G
        /// </summary>
        /// <param name="team">ID команды</param>
        /// <param name="brawlerID">ID бойца</param>
        /// <returns>Возвращает true - если установить бакугана удалось</returns>
        public void setBakugan(bool isPyrus, bool isHaos, bool isAquos,
                                bool isDarkus, bool isSubterra, bool isVentus, int g)
        {
            state = 0;
            bakuganInGateID = -1;
            gGlobal = gGame = this.g = g;
            this.isPyrus = isPyrus;
            this.isHaos = isHaos;
            this.isAquos = isAquos;
            this.isDarkus = isDarkus;
            this.isSubterra = isSubterra;
            this.isVentus = isVentus;
        }
    }
}
