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
        // BRAWLER MAIN INFORMATION SEGMENT
        public string name { get; set; }// name of brawler
        public ulong BID{ get; set; }// unique Brawler Identification number in World System
        public ulong rank { get; set; }// current rank in battle. Just for read


        //The amount of the bank check to be collected or contributed
        //in cash to the player's bank account EBoV at the end of the battle
        public double money { get; set; }
        


        // BRAWLER DOOMCARD SEGMENT
        public bool avDoom { get; set; }// Is doom card in pocket?
        public bool setDoom { get; set; }// Is doom card set on field?



        // BRAWLER IN-BATTLE INFORMATION SEGMENT
        bool inGame = false;// Is still playing?
        uint winBakugan = 0;
        public uint loseBakugan { get; set; }

        public uint teamID { get; set; } //ID of team. 0 - is Spectator. Only non-null values are allowed for battle
        public uint brawlerID { get; set; } //ID in field. The value is assigned in the order in the battle.xml file
        public uint queueID { get; set; }// Queue in game. In (0,NbrBraw-1). Sorting by field.SortBrawlers();

        public uint usedGateCard { get; private set; }  // max NbrBaku
        public uint usedAbilityCard { get; private set; }  // max NbrBaku


        // MUST BE PRIVATE IN FUTURE
        public Bakugan[] bakugan;
        public AbilityCard[] abilityCard { get; private set; } 
        public GateCard[] gateCard { get; private set; }


        // I could use the Singleton design pattern here,
        // but I expect to expand the application to simulate several simultaneous battles,
        // which can be greatly hindered by the uniqueness of the Field class
        // executed in the style of the Singleton pattern
        Field field;




        /// <summary>
        /// Brawler constructor. Allocate memory for internal objects
        /// </summary>
        /// <param name="brawlerID"> Brawler ID according to the order in the battle.xml file </param>
        public Brawler ( uint brawlerID, Field field)
        {
            setDoom = false;
            this.field = field;

            bakugan = new Bakugan[field.NbrBaku];
            for (int i = 0; i < field.NbrBaku; i++)
                bakugan[i] = new Bakugan();

            abilityCard = new AbilityCard[3*field.NbrBaku];
            for (int i = 0; i < 3*field.NbrBaku; i++)
                abilityCard[i] = new AbilityCard(field);

            gateCard = new GateCard[field.NbrBaku];
            for (int i = 0; i < field.NbrBaku; i++)
                gateCard[i] = new GateCard(field);

            usedGateCard = 0;
            usedAbilityCard = 0;

            this.brawlerID = brawlerID;
        }


        /// <summary>
        /// Brawler forcibly initiates a bakuganID bakugan throw at gate (x,y)
        /// </summary>
        /// <param name="x">Gate x coordinate.</param>
        /// <param name="y">Gate y coordinate.</param>
        /// <param name="bakuganID">The ID of the Bakugan from the arsenal we want to send on battle.</param>
        /// <returns>Returns true - if bakugan was successfully installed</returns>
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
                    bool isYourTeam = false;// Are there Bakugan on your team?
                    int enemyCount = 0;// Number of enemy Bakugan
                    int friendCount = 0;// Number of friendly Bakugan
                    bool bakuInBattleLink = false;// Is there a bakugan in the battleLink list

                    // Checking if a card is friendly or enemy
                    for (int i = 0; i < gateBakuID; i++)
                    {
                        if (field.gate[x, y].bakugan[i].team != bakugan[bakuganID].team)
                            enemyCount++;
                        if (field.gate[x, y].bakugan[i].team == bakugan[bakuganID].team)
                            friendCount++;

                        isYourTeam = isYourTeam || field.gate[x, y].bakugan[i].team == bakugan[bakuganID].team;
                    }

                    // Searching For Each Bakugan In The BattleLink List
                    // When the code for maps is ready (battleLink changes)
                    // Partially change the code if there are only allies on the map
                    // NOT THOUGHT IF THE MAP IS ONE OR MORE ALLIES

                    bool myBakuInList = false;
                    bool enBakuFound = false;

                    // For all Bakugan on a non-empty map
                    for (int i = 0; i < gateBakuID; i++)
                    {
                        int countBL = field.battleLink.Count;

                        // For All Enemy Bakugan
                        if (field.gate[x, y].bakugan[i].team != bakugan[bakuganID].team && 
                            field.gate[x, y].bakugan[i].state != BakuState.DoesntExist)
                        {

                            // Looking for enemy Bakugan all over BattleLink
                            foreach (List<Bakugan> innerList in field.battleLink)
                            {
                                foreach (Bakugan obj in Enumerable.Reverse(innerList))
                                //for(int obj =0; obj < innerList.Count; obj++)
                                {
                                    // We are already in the current sublist. We don't need duplicates of our Bakugan
                                    if (obj == bakugan[bakuganID])
                                    {
                                        myBakuInList = true;
                                        //break;
                                    }
                                    // Enemy Bakugan is already in the battle, let's join him
                                    if (obj == field.gate[x, y].bakugan[i] )
                                    {
                                        enBakuFound = true;

                                        if (!myBakuInList && obj.state != BakuState.DoesntExist)
                                            innerList.Add(bakugan[bakuganID]);
                                        
                                        break;
                                    }
                                }
                            }

                            // For an empty battleLink
                            // If there is no bakugan in the battleLink list, first add yourself, then another bakugan
                            if ((!enBakuFound || countBL == 0) && !myBakuInList)
                            {
                                field.battleLink.Add(new List<Bakugan>());
                                countBL = field.battleLink.Count;
                                field.battleLink[countBL - 1].Add(bakugan[bakuganID]);
                                field.battleLink[countBL - 1].Add(field.gate[x, y].bakugan[i]);
                            }

                            // If we've already found our Bakugan but haven't added our enemy to the list
                            if (myBakuInList && !enBakuFound)
                            {
                                field.battleLink[countBL - 1].Add(field.gate[x, y].bakugan[i]);
                            }
                        }
                    }


                    // Adding our Bakugan to the field
                    field.gate[x, y].bakugan.Add(bakugan[bakuganID]);

                    bakugan[bakuganID].x = x;
                    bakugan[bakuganID].y = y;
                    bakugan[bakuganID].bakuganInGateID = (int)gateBakuID;
                    bakugan[bakuganID].state = BakuState.OnGate;

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
        /// The brawler activates an ability card from the arsenal with the number abilityID
        /// </summary>
        /// <param name="abilityID">Card number in the arsenal</param>
        /// <param name="bakuganID">Bakugan on which the ability card will be used</param>
        /// <returns>Returns true - if it was possible to activate the ability</returns>
        public bool useAbility(uint abilityID,uint bakuganID)
        {
            if (field.NbrBaku >= usedAbilityCard)
            {
                bool ableActivate = abilityCard[abilityID].activate();
                
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
        /// Activates map with position (x,y)
        /// </summary>
        /// <param name="x">Gate x coordinate.</param>
        /// <param name="y">Gate y coordinate.</param>
        /// <returns>Returns true - if it was possible to activate the card</returns>
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
        /// Places a gateID gate map at coordinates (x,y)
        /// </summary>
        /// <param name="x">Gate x coordinate.</param>
        /// <param name="y">Gate y coordinate.</param>
        /// <param name="gateID">Gate card number in inventory.</param>
        /// <returns>Returns true - if it was possible to place the map</returns>
        public bool setGate(int x, int y, uint gateID)
        {
            //Console.WriteLine(field.gate[x, y].isBusy);
            if (0 <= x && x < field.NbrBraw && 
                0 <= y && y < field.NbrBaku &&
                !field.gate[x, y].isBusy && 
                !gateCard[gateID].isPlaced && 
                usedGateCard!= field.NbrBaku )
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
                    field.setAppLog($"Brawler {brawlerID} Error message: Brawler trying to set used gateCard {gateID}");

                if (usedGateCard == field.NbrBaku)
                    field.setAppLog($"Brawler {brawlerID} Error message: All cards is already used");
                return false;
            }
        }


        /// <summary>
        /// Set Doom Card
        /// </summary>
        /// <returns>Returns true - if it was possible to fill in the field correctly</returns>
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
        /// Remove Doom Card
        /// </summary>
        /// <returns>Returns true - if it was possible to fill in the field correctly</returns>
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
