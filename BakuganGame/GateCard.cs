using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace BakuganGame
{

    internal class GateCard
    {
        uint teamID; //you need a map to distinguish enemies from friends
        uint brawlerID;// Ability Card User Link
        public uint gateID{ get; set; }// You need to understand the order of the card in the inventory
        public uint gateType { get; set; }// 0 - empty, rest type for activate

        bool isActivated = false;
        bool isUsed = false;
        public bool isPlaced{ get; set; }//true - in gate,
                                //false - in pocket

        Field field;//The logic of the map is based on the situation on the battlefield

        public GateCard(Field field)
        {
            gateType = 1;
            this.field = field;
            isPlaced = false;
        }


        /// <summary>
        /// Define the card, knowing the code: team, player, bakugan, gate cards. And the view of the gate (see the activate function)
        /// </summary>
        /// <param name="teamID">team ID</param>
        /// <param name="brawlerID">Brawler ID</param>
        /// <param name="gateID">ID of gate cards in inventory</param>
        /// <param name="gateType">Gate card type (see activate function)</param>
        /// <returns>Returns true - if Bakugan was installed successfully</returns>
        public bool define(uint teamID, uint brawlerID, uint gateID)
        {
            this.teamID = teamID;
            this.brawlerID = brawlerID;
            this.gateID = gateID;

            return true;
        }



        /// <summary>
        /// Mark the card as removed from the battlefield
        /// </summary>
        /// <returns>Returns true - if successful completion</returns>
        public bool removeFromField()
        {
            if(isPlaced)
            {
                isPlaced = false;

                return true;
            }
            else
            {
                field.setAppLog("Gate card message: ERROR in removeFromField function");
                field.setAppLog("Can't remove card that placed in pocket");

                return false;
            }
        }


        /// <summary>
        /// Activate the operation of the gate card (The main logic of all cards is described here)
        /// </summary>
        /// <returns>Returns true - if successful</returns>
        public bool activate(int x, int y)
        {
            // In the future, it is important to check whether it is possible in principle to activate the function. Suddenly on
            // special conditions field and we can't activate the card
            if (!isUsed && !isActivated && gateType != 0)
            {
                switch (gateType)
                {
                    case 1://Claim 500 for yourself
                        field.brawler[brawlerID].bakugan[0].g += 500;
                        break;
                    case 2://Add 500 to yourself and remove 100 to all enemies
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
                field.setAppLog("Gate card message: ERROR in activate function");

                if (isActivated)
                    field.setAppLog("Why do you try to activate activated card?");
                if (isUsed)
                    field.setAppLog("Why do you try to activate used card?");
                if (gateType == 0)
                    field.setAppLog("Your gate slot is empty");

                return false;
            }
        }


        /// <summary>
        /// Deactivate the gate map. Occurs when the queueBattle ends.
        /// </summary>
        /// <returns>Returns true - if successful</returns>
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
                field.setAppLog("Gate card message: ERROR in deactivate function");

                if (!isActivated)
                    field.setAppLog("Why do you try to deactivate deactivated card?");
                if (!isUsed)
                    field.setAppLog("Why do you try to deactivate unused card?");

                return false;
            }

        }

    }

}
