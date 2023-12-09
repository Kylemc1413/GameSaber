using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSaber
{
    public static class Utilities
    {
        public static List<string> IntroTexts = new List<string>
        {
            "Make your choices carefully",
            "If you don't choose\n the game will choose \nfor you",
            "The Game will begin soon",
            "If you lose it's all over",
            "You must keep playing until the end",
            "Slice your choice"
        };

        public static List<string> GameTexts = new List<string>
        {
            "Make your choices carefully",
            "If you don't choose\n the game will choose \nfor you",
            "Only $time seconds left\n Can you survive to the end?",
            "You've completed \n$gameCount games so far\n" +
            "How many more will it take?",
            "Do you end your turn?",
            "Your opponent is playing to win",


        };

        public static IGame NewGameForType(GameType type)
        {
            if (type == GameType.TicTacToe)
                return new TicTacToe();
            else if (type == GameType.ConnectFour)
                return new ConnectFour();
            else
                return null;
        }
        private static NoteLineLayer RowForNum(int num, PlayerType type)
        {
            switch(num)
            {
                case 1:
                case 2:
                case 3:
                    return NoteLineLayer.Top;
                case 4:
                case 5:
                case 6:
                    return NoteLineLayer.Upper;
                case 7:
                case 8:
                case 9:
                    return NoteLineLayer.Base;
                default:
                    return NoteLineLayer.Base;
            }
        }

        public static int ColForNum(int num, PlayerType type)
        {
            switch (num)
            {
                case 1:
                case 4:
                case 7:
                    return type == PlayerType.First ? 0 : 1;
                case 2:
                case 5:
                case 8:
                    return type == PlayerType.First ? 1 : 2;
                case 3:
                case 6:
                case 9:
                    return type == PlayerType.First ? 2 : 3;
                default:
                    return 0;
            }
        }
        // 7 lane lanes
        //  -3500 |   -2500 -1500  1500   2500    3500 4500 5500 | 6500
        public static List<BeatmapObjectData> GetPlayerSelectionBeatmapObjects(float time, GameType game, float gameEndTime)
        {
            List<BeatmapObjectData> result = new List<BeatmapObjectData>();
            if(game == GameType.TicTacToe)
            {
                result.Add(new GameNote(PlayerType.First, -1, time, 1, NoteLineLayer.Base, NoteLineLayer.Base, ColorType.ColorA, NoteCutDirection.Any, 0f, 0f));
                result.Add(new GameNote(PlayerType.Second, -1, time, 2, NoteLineLayer.Base, NoteLineLayer.Base, ColorType.ColorB, NoteCutDirection.Any, 0f, 0f));
            }
            else if(game == GameType.ConnectFour)
            {
                result.Add(new GameNote(PlayerType.First, -1, time, 1500, NoteLineLayer.Base, NoteLineLayer.Base, ColorType.ColorA, NoteCutDirection.Any, 0f, 0f));
                result.Add(new GameNote(PlayerType.Second, -1, time, 3500, NoteLineLayer.Base, NoteLineLayer.Base, ColorType.ColorB, NoteCutDirection.Any, 0f, 0f));
                for(int i = 1; i < 9; ++i)
                result.Add(new GameObstacle(time, IndexForColumn(i), 0, gameEndTime - time, 1050, 1100));

            }
            return result;
        }

        private static int IndexForColumn(int col)
        {
            switch(col)
            {
                case 0:
                    return -3500;
                case 1:
                    return -2500;
                case 2:
                    return -1500;
                case 3:
                    return 1500;
                case 4:
                    return 2500;
                case 5:
                    return 3500;
                case 6:
                    return 4500;
                case 7:
                    return 5500;
                case 8:
                    return 6500;
                default:
                    return 2500;
            }
        }
        // 7 lane lanes
        //  -3500 |   -2500 -1500  1500   2500    3500 4500 5500 | 6500
        public static List<BeatmapObjectData> GetTurnSelectionBeatmapObjects(float time, List<int> spaces, PlayerType playerType, GameType game)
        {
            List<BeatmapObjectData> result = new List<BeatmapObjectData>();
            foreach(int i in spaces)
            {
                if(game == GameType.TicTacToe)
                {
                    ColorType type = playerType == PlayerType.First ? ColorType.ColorA : ColorType.ColorB;
                    int index = ColForNum(i, playerType);
                    NoteLineLayer layer = RowForNum(i, playerType);
                    result.Add(new GameNote(playerType, i, time, index, layer, layer, type, NoteCutDirection.Any, 0f, 0f));
                }
                else if(game == GameType.ConnectFour)
                {
                    ColorType type = i < 4 ? ColorType.ColorA : i > 4 ? ColorType.ColorB : playerType == PlayerType.First ? ColorType.ColorA : ColorType.ColorB;
                    result.Add(new GameNote(playerType, i, time, IndexForColumn(i), NoteLineLayer.Base, NoteLineLayer.Base, type, NoteCutDirection.Any, 0f, 0f));
                }
            }


            return result;
        }

        public static BeatmapObjectData[] GetGridBeatmapObjects(float gridTime)
        {
            int gridWidth = 5;
            int gridHeight = 5;

            List<BeatmapObjectData> result = new List<BeatmapObjectData>();
            for(int i = 0; i <= gridWidth; i++)
                for(int j = 0; i < gridHeight; i++)
                {
                    if(i % 2 != 0 || j % 2 != 0)
                    {
               //         var gridItem = new NoteData(gridTime, i, (NoteLineLayer)j, (NoteLineLayer)j, ColorType.None, NoteCutDirection.Down, 0f, 0f, i, 0f, 0f);
                //        result.Add(gridItem);
                    }
                }
            return result.ToArray();
        }
    }
}
