using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BakuganGame
{
    /*
    Gates are the countable unit of the field. Each Gate Card:
        * Has coordinates on the field
        * Number of Bakugan on it
        * Refers to gate card (specific type)
    */
    internal class Gate
    {
        //Goal coordinates on the field
        public int x { get; private set; }
        public int y { get; private set; }

       
        public bool isBusy = false; // there is someone's card on the gate


        public int gateOwner { get; private set; }// ] Brawler ID


        public List<Bakugan> bakugan { get; set; }//link to bakugan installed on the map
        public GateCard gateCard; //link to an installed gate card owned by an individual fighter
        Field field; // It is important for the gate to know what is happening around, so a link to the field is needed

        public Gate(uint NbrBaku, uint NbrTeam, uint NbrBraw, int x, int y, Field field) 
        {
            this.x = x;
            this.y = y;
            this.field = field;
            gateOwner = -1;

            bakugan = new List<Bakugan>();
            gateCard = new GateCard(field);
        }


        /// <summary>
        /// Force put gateID card from brawlerID fighter
        /// </summary>
        /// <param name="brawlerID">Player ID</param>
        /// <param name="gateID">Player's Gate Card ID</param>
        /// <returns>Returns true - if bakugan was successfully installed</returns>
        public bool placePlayerGate(uint brawlerID, uint gateID)
        {
            isBusy = true;
            gateOwner = (int)brawlerID;
            gateCard = field.brawler[brawlerID].gateCard[gateID];
            field.brawler[brawlerID].gateCard[gateID].isPlaced = true;

            field.setAppLog($"Gate ({x},{y}) message: brawler {brawlerID} stand gate {gateID}");

            return true;
        }


        /// <summary>
        /// Remove an already installed card from the gate
        /// </summary>
        /// <returns>Returns true - if the map was successfully removed</returns>
        public bool removePlayerGate()
        {
            if (isBusy)
            {
                field.setAppLog($"Gate ({x},{y}) message: gate erased, removed card from gate ");
                gateCard.removeFromField();//need a reason why we delete: end of battle, end of game

                isBusy = false;
                bakugan = new List<Bakugan>();
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

    }
}
