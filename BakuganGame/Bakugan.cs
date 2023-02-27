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
        uint bakuganID;
        public int bakuganInGateID { get; set; }
        int x;
        int y;

        public int state { get; set; }
                      //0 - in pocket,
                      //1 - in card,
                      //2 - in battle,
                      //3 - knocked out
                      //4 - killed
        

        public uint team  { get; private set; }
        public uint owner { get; private set; }//BrawlerID

        //G level
        public int g { get; set; }
        public int gGame { get; set; }
        public int gGlobal { get; set; }

        //Atribute
        public bool isPyrus { get; private set; }
        public bool isAquos { get; private set; }
        public bool isDarkus { get; private set; }
        public bool isVentus { get; private set; }
        public bool isSubterra { get; private set; }
        public bool isHaos { get; private set; }

        public Bakugan()
        {
        }


        /// <summary>
        /// Определим карту, зная код: команды, игрока
        /// </summary>
        /// <param name="team">ID команды</param>
        /// <param name="brawlerID">ID бойца</param>
        /// <returns>Возвращает true - если установить бакугана удалось</returns>
        public bool define(uint brawlerID, uint team)
        {
            owner = brawlerID;
            this.team = team;

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
