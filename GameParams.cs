using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IPA;
using IPA.Config;
using IPA.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace GameSaber
{
   public enum GameType { None, TicTacToe, ConnectFour }
    public class GameParams
    {
        public DiffGameParams[] games;
        public class DiffGameParams
        {
            public string beatmapDifficultyName = "";
            public string beatmapCharacteristicName = "";
            public GameType gameType = GameType.None;
            public float gameStart = 5f;
            public float gameTurnInterval = 5f;
            public float gameEndTime = float.MaxValue;
        }

    }


}
