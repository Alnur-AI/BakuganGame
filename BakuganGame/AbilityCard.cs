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
        uint teamID; // you need to distinguish enemies from friends on map
        uint brawlerID;// Ability Card User Link
        public uint abilityID { get; set; }// You need to understand the order of the card in the inventory
        public uint abilityType { get; set; }// ability tipe for activate()

        public bool isActivated{ get; set; }
        public bool isUsed{ get; set; }

        Field field;// Map logic is based on the situation on the battlefield

        public AbilityCard(Field field)
        {
            abilityType = 1;
            this.field = field;
            isActivated = false;
            isUsed = false;
}


        /// <summary>
        /// Define the card, knowing the code: team, player, bakugan, ability. And the kind of ability (see the activate function)
        /// </summary>
        /// <param name="teamID">team ID</param>
        /// <param name="brawlerID">Brawler ID (The card must know the order of its owner)</param>
        /// <param name="bakuganID">ID of the bakugan in the inventory that the ability is assigned to</param>
        /// <param name="abilityID">ID of the ability in the inventory (it is necessary that the card knows its order in the inventory)</param>
        /// <param name="abilityType">Ability card type (see activate function)</param>
        /// <returns>Returns true - if Bakugan was installed successfully</returns>
        public bool define(uint teamID, uint brawlerID, uint abilityID)
        {
            this.teamID = teamID;
            this.brawlerID = brawlerID;
            this.abilityID = abilityID;

            return true;
        }


        /// <summary>
        /// Activate the operation of the ability card (The main logic of all cards is described here)
        /// </summary>
        /// <returns>Returns true - if successful</returns>
        public bool activate()
        {
            // In the future, it is important to check whether it is possible in principle to activate the function. 
            // Suddenly on special conditions field and we can't activate the card
            if (!isUsed && !isActivated && abilityType != 0)
            {
                switch (abilityType)
                {
                    case 1: //Water Refrain:
                            //Nullifies and blocks the opponent's abilities from activating for a short time.

                        // 1) Search through the list of activated abilities for the enemy card we want to cancel
                        // 2) After selecting the desired ability in the menu, deactivate it with deactivate
                        // 3) After deactivation, we prohibit the enemy fighter from using the ability card when he walks
                        break;



                    case 2: //Aquos Quadruple Chain Attack - Blue Lagoon Force Gate Special:
                            //(requires 4 Aquos Bakugan on your team to activate) 

                        // 1) Check the battlefield for the presence of 4 aquas fighters belonging to our team
                        // 2) If there are 4 fighters on the field, then we destroy or kill all Bakugan on the battlefield, except for the activating card
                        // 3) The surviving Bakugan a priori becomes the winner of all battleLinks on the field

                        break;



                    case 3: //Dive Mirage Type 1:
                            //Moves Bakugan to another Gate Card, making the Gate Card it goes on nullified. 

                        // 1) Move the bakugan to any map of your choice (when choosing a place through WASD, press Enter)
                        // 2) After moving, we cancel the gate map we hit
                        break;



                    case 4: //Dive Mirage Type 2:
                            //Moves Griffon to a different Gate Card and prevents the opponent from activating it.  

                        //1) Move the bakugan to any map of your choice (when choosing a place through WASD, press Enter)
                        //2) After moving the gate card, it will never be possible to activate it again
                        break;



                    case 5: //Spiced Assault
                            //Adds 100 Gs to Centipoid and subtracts 100 Gs from each opponent.

                        //1) Add 100 points to the player
                        //2) Remove 100 points for all players on the battlefield
                        break;



                    case 6: //Grand Down Type 1:
                            //Nullifies the opponent's Gate Card. 

                        // 1) Select the gate card we want to reset
                        // 2) If it is enemy, then deactivate it
                        break;



                    case 7: //Grand Down Type 2:
                            //Nullifies everyone's Gate Card. 

                        // 1) Select the gate card we want to reset
                        // 2) If it exists, then deactivate it
                        break;



                    case 8: //Auragano Revenge
                            //Adds 100 Gs to Hydranoid and subtracts 100 Gs from each opponent. 

                        //1) Add 100 points to the player
                        //2) Remove 100 points for all players on the battlefield

                        //field.brawler[brawlerID].bakugan[bakuganID].g += 100;
                        //for (int i = 0; i < field.NbrBraw; i++)
                        //if (i != brawlerID && teamID != field.brawler[i].teamID)
                        //field.brawler[i].bakugan[bakuganID].g -= 100;

                        break;



                    case 9: //Darkus Gravity:
                            //Adds another Darkus Bakugan to the battle. 

                        // 1) In the menu, select the desired Bakugan Darkus
                        // 2) Send this bakugan to the selected gate card
                        break;



                    case 10: //Spiced Slayer
                             //Adds 100 Gs to Laserman and subtracts 100 Gs from each opponent. 

                        //1) Add 100 points to the player
                        //2) Remove 100 points for all players on the battlefield

                        //field.brawler[brawlerID].bakugan[bakuganID].g += 100;
                        //for (int i = 0; i < field.NbrBraw; i++)
                        //if (i != brawlerID && teamID != field.brawler[i].teamID)
                        //field.brawler[i].bakugan[bakuganID].g -= 100;
                        break;



                    case 11: //Lightning Shield:
                             //Nullifies the opponent's Gate Card. 

                        break;



                    case 12: //Haos Freeze Attack
                             //Nullifies all of the opponent's abilities, stops time and allows you to add more Bakugan to the field. 

                        break;



                    case 13: //Ability Counter:
                             //Nullifies the opponent's ability. 

                        break;



                    case 14: //Lightning Tornado Type 1: 
                             //Adds 100 Gs to Tigrerra and subtracts 100 Gs from each opponent. 

                        break;



                    case 15: //Lightning Tornado Type 2: 
                             //Adds 100 Gs to Griffon and subtracts 100 Gs from each opponent. 

                        break;



                    case 16: //Shade Ability:
                             //Nullifies the opponent's abilities anywhere in the field. 

                        break;



                    case 17: //Rapid Haos Type 1:
                             //Adds another Haos Bakugan to the battle.

                        break;



                    case 18: //Rapid Haos Type 2:
                             //Brings another Bakugan into the battle if there are more than one э
                             //Pyrus, Aquos, or Haos Bakugan on the field. 

                        break;



                    case 19: //Fire Wall:
                             //Subtracts 50 Gs from the opponent. 

                        break;



                    case 20: //Fire Tornado:
                             //Adds 100 Gs to Ultimate Dragonoid and subtracts 100 Gs from each opponent. 

                        break;



                    case 21: //Fire Judge:
                             //Adds 100 Gs to Ultimate Dragonoid. 

                        break;



                    case 22: //Rapid Fire:
                             //Adds another Bakugan to the battle if there are 2 or more Pyrus Bakugan on the field. 

                        break;



                    case 23: //Power Charge:
                             //Adds 100 Gs to Saurus and allows him to attack from any part of a field. 

                        break;



                    case 24: //Grand Slide Type 1: 
                            //Moves the opponent's Gate Card to anywhere on the field and 
                            //allows Hammer Gorem to attack if there is a Bakugan on an adjacent card. 

                        break;



                    case 25: //Grand Slide Type 2: 
                            //Moves the opponent's Gate Card to anywhere on the field and 
                            //allows Hammer Gorem to attack if there is a Bakugan on an adjacent card. 

                        break;



                    case 26: //Copycat: 
                            //Copies an ability that the opponent used or is using. 

                        break;



                    case 27: //Scarlet Twister (Crimson Twister): 
                             //Bee Striker can send any Bakugan on the field back to its owner, 
                             //but if it does so to save a Bakugan from defeat, 
                             //Bee Striker is in turn eliminated.

                        break;



                    case 28: //Blow Away:
                             //Moves the opponent to another Gate Card. 

                        break;



                    case 29: //Air Battle:
                             //Monarus can fly beyond Gate Cards and nullifies the Gate Cards that it lands on. 

                        break;




                        /*
                         case 6: //
                                //

                            break;
                        */

                }
                isActivated = true;
                isUsed = true;

                return true;
            }
            else
            {
                field.setAppLog("Ability card message: ERROR in activate function");

                if (isActivated)
                    field.setAppLog("Why do you try to activate activated card?");
                if (isUsed)
                    field.setAppLog("Why do you try to activate used card?");
                if (abilityType == 0)
                    field.setAppLog("Your ability slot is empty");

                return false;
            }
        }


        /// <summary>
        /// Deactivate the ability card. Occurs when a queueBattle ends or a sudden impact
        /// </summary>
        /// <returns>Returns true - if successful</returns>
        public void deactivate()
        {
            if (isActivated && isUsed)
            {
                isActivated = false;
                //isUsed = true;
            }
            else
            {
                field.setAppLog("Ability card message: ERROR in deactivate function");
                
                if(!isActivated)
                    field.setAppLog("Why do you try to deactivate deactivated card?");
                if(!isUsed)
                    field.setAppLog("Why do you try to deactivate unused card?");
            }
            
        }


       
    }
}
