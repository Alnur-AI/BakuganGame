using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BakuganGame
{
    /*
        Данный класс определяет поле, которое содержит в себе 
        Фиксированное количество ворот на время игры
        Менять количество ворот на поле запрещено 

        Так же поле содержит в себе информацию по бакуганам, их картам ворот и способностей    
    */
    internal class Field
    {

        public uint NbrBaku { get; private set; }
        public uint NbrTeam { get; private set; }
        public uint NbrBraw { get; private set; }

        public Brawler[] brawler;
        public Gate[,] gate;


        string battleLog;// логирование процессов битвы во время выполнения приложения (Console Information)
        string errorLog;// логирование процессов ошибок во время выполнения приложения (Console Information)

        public Field(uint NbrBaku, uint NbrTeam, uint NbrBraw)
        {
            this.NbrBaku = NbrBaku;
            this.NbrTeam = NbrTeam;
            this.NbrBraw = NbrBraw;

            brawler = new Brawler[NbrBraw];
            for (uint i = 0; i < NbrBraw; i++)
                brawler[i] = new Brawler(NbrBaku, NbrTeam, NbrBraw,i);
            
            gate = new Gate[NbrBraw, NbrBaku];
            for (int i = 0; i < NbrBraw; i++)
                for (int j = 0; j < NbrBaku; j++)
                    gate[i,j] = new Gate(NbrBaku, NbrTeam, NbrBraw,i,j);

            //Предполагается, что поле содержит в себе списки боевых связей между бакуганами.
        }

        /// <summary>
        /// Записываем строку str в log
        /// </summary>
        /// <returns>Возвращает true - если удалось заполнить </returns>
        public bool setBattleLog(string str)
        {
            battleLog = battleLog + str;
            return true;
        }

        /// <summary>
        /// Записываем строку str в log
        /// </summary>
        /// <returns>Возвращает true - если удалось заполнить </returns>
        public bool setErrorLog(string str)
        {
            errorLog = errorLog + str;
            return true;
        }

        /// <summary>
        /// Обязательна после конструктора. Каждый обьект на поле заполняется ссылкой field на текущее поле.
        /// </summary>
        /// <returns>Возвращает true - если удалось заполнить </returns>
        public bool setReferenceField()
        {
            for (int i = 0; i < NbrBraw; i++)
                brawler[i].setField(this);

            for (int i = 0; i < NbrBraw; i++)
                for (int j = 0; j < NbrBaku; j++)
                    gate[i, j].setField(this);

            return true;
        }
    }
}
