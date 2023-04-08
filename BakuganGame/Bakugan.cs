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
        public int bakuganInGateID { get; set; }//ID on the gate (-1 means that the bakugan is in inventory)

        public int x { get; set; }
        public int y { get; set; }

        public BakuState state { get; set; }
                      //-1 - does not exist
                      //0 - in pocket,
                      //1 - in card,
                      //2 - in battle,
                      //3 - knocked out
                      //4 - killed
        

        public uint team  { get; private set; }
                    // 0 - team of observers
                    // 1,2... - others
        public uint owner { get; private set; }//BrawlerID

        //G level
        public int g { get; set; }// G in local combat
        public int gGame { get; set; }// G changed initial level for the duration of the game
        public int gGlobal { get; set; }// G changed starting level permanently

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
            team = 0;
            gGlobal = gGame = this.g = 0;
        }


        /// <summary>
        /// Define Bakugan, knowing the code: team, player
        /// </summary>
        /// <param name="team">Team ID</param>
        /// <param name="brawlerID">Brawler ID</param>
        /// <returns>Returns true - if Bakugan was installed successfully</returns>
        public bool define(uint teamID,uint brawlerID,uint bakuganID)
        {
            owner = brawlerID;
            this.bakuganID = bakuganID;
            team = teamID;
            return true;
        }


        /// <summary>
        /// Set Bakugan properties: element type and amount of G
        /// </summary>
        /// <param name="team">Team ID</param>
        /// <param name="brawlerID">Brawler ID</param>
        /// <returns>Returns true - if Bakugan was installed successfully</returns>
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
