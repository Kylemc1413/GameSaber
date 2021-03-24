using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSaber
{
    class ConnectFour : IGame
    {
        Dictionary<int, bool> claimedSpaces = new Dictionary<int, bool>();
        //⬤
        //◯
        public string spaceChar = "O";
        protected new string _GameText =
                "<#CCCCCC>" +
                "|                                                                  |\n" +
                "|  11  |  12  |  13  |  14  |  15  |  16  |  17  |\n" +
                "|---------------------------------|\n" +
                "|  21  |  22  |  23  |  24  |  25  |  26  |  27  |\n" +
                "|---------------------------------|\n" +
                "|  31  |  32  |  33  |  34  |  35  |  36  |  37  |\n" +
                "|---------------------------------|\n" +
                "|  41  |  42  |  43  |  44  |  45  |  46  |  47  |\n" +
                "|---------------------------------|\n" +
                "|  51  |  52  |  53  |  54  |  55  |  56  |  57  |\n" +
                "|---------------------------------|\n" +
                "|  61  |  62  |  63  |  64  |  65  |  66  |  67  |\n" +
                "|---------------------------------|";
        public override string GetGameText()
        {
            string result = _GameText;
            for (int i = 1; i < 7; ++i)
            {
                for (int j = 1; j < 8; j++)
                {
                    int place = (10 * i) + j;
                    string character = "?";
                    if (claimedSpaces.TryGetValue(place, out bool player))
                    {
                        if (player)
                            character = $"<#00FF00>{spaceChar}</color>";
                        else
                            character = $"<#FF0000>{spaceChar}</color>";
                    }
                    result = result.Replace(place.ToString(), $" {character} ");
                }
            }
            return result;
        }

        protected int getNextSpaceForColumn(int col)
        {
            for (int i = 6; i > 0; i--)
            {
                int space = (10 * i) + col;
                if (!claimedSpaces.ContainsKey(space))
                {
                    if (i == 1)
                        _choices.Remove(col);
                    return space;
                }

            }
            return -1;
        }

        protected int CheckNextSpaceForColumn(int col)
        {
            for (int i = 6; i > 0; i--)
            {
                int space = (10 * i) + col;
                if (!claimedSpaces.ContainsKey(space))
                {
                    return space;
                }

            }
            return -1;
        }
        protected override void Init(bool playerIsFirst)
        {
            _choices = new List<int> { 1, 2, 3, 4, 5, 6, 7 };
        }
        public override void PlayerTurn(int selection)
        {
            int place = getNextSpaceForColumn(selection);
            claimedSpaces.Add(place, true);
            Logger.log.Debug($"Player Turn: {selection} | {place}");
            if (Evaluate()) return;
            aiTurnStart?.Invoke(_choices);
            AITurn();
        }

        protected override async void AITurn()
        {
            await Task.Delay((int)(TurnLength * 1000 * 0.75f));
            int aiSelection = GetAiSelection();
            int place = getNextSpaceForColumn(aiSelection);
            claimedSpaces.Add(place, false);
            Logger.log.Debug($"AI Turn: {aiSelection} | {place}");
            /*
            Logger.log.Debug($"-------------------");
            foreach (var a in claimedSpaces)
            {
                Logger.log.Debug($"{a.Key} | {a.Value}");
            }

            Logger.log.Debug($"-------------------");
                        */
            if (Evaluate()) return;
            playerTurnStart?.Invoke(_choices);
        }

        private bool FinishEvaluate(bool playerWon, bool aiWon)
        {
            if (playerWon || aiWon || _choices.Count == 0)
            {
                gameHasEnded?.Invoke(playerWon, (!aiWon && !playerWon));
                return true;
            }
            return false;
        }
        protected override bool Evaluate()
        {
            for (int i = 6; i > 0; --i)
            {
                for (int j = 1; j < 8; j++)
                {
                    int place = (10 * i) + j;
                    if (!claimedSpaces.TryGetValue(place, out bool player))
                        continue;

                    // Check Vertical
                    int connectCount = 1;
                    if (i < 4)
                    {
                        int mark = place;
                        bool stopReached = false;
                        while (!stopReached)
                        {
                            if (connectCount == 4)
                            {
                                return FinishEvaluate(player, !player);
                            }
                            int next = mark + 10;
                            if (claimedSpaces.TryGetValue(next, out bool owner))
                            {
                                if (player == owner)
                                {
                                    mark = next;
                                    connectCount++;
                                    continue;
                                }

                                else
                                    stopReached = true;
                            }
                            else
                                stopReached = true;
                        }
                    }
                    // Check Horizontal
                    connectCount = 1;
                    {
                        int mark = place;
                        bool stopReached = false;
                        while (!stopReached)
                        {
                            if (connectCount == 4)
                            {
                                return FinishEvaluate(player, !player);
                            }
                            int next = mark + 1;
                            if (claimedSpaces.TryGetValue(next, out bool owner))
                            {
                                if (player == owner)
                                {
                                    mark = next;
                                    connectCount++;
                                    continue;
                                }

                                else
                                    stopReached = true;
                            }
                            else
                                stopReached = true;
                        }
                    }
                    // Check Diagonal Asc
                    connectCount = 1;
                    if (i < 4 && j < 5)
                    {
                        int mark = place;
                        bool stopReached = false;
                        while (!stopReached)
                        {
                            if (connectCount == 4)
                            {
                                return FinishEvaluate(player, !player);
                            }
                            int next = mark + 11;
                            if (claimedSpaces.TryGetValue(next, out bool owner))
                            {
                                if (player == owner)
                                {
                                    mark = next;
                                    connectCount++;
                                    continue;
                                }

                                else
                                    stopReached = true;
                            }
                            else
                                stopReached = true;
                        }
                    }
                    // Check Diagonal Desc
                    connectCount = 1;
                    if (i < 4 && j >= 4)
                    {
                        int mark = place;
                        bool stopReached = false;
                        while (!stopReached)
                        {
                            if (connectCount == 4)
                            {
                                return FinishEvaluate(player, !player);
                            }
                            int next = mark + 9;
                            if (claimedSpaces.TryGetValue(next, out bool owner))
                            {
                                if (player == owner)
                                {
                                    mark = next;
                                    connectCount++;
                                    continue;
                                }

                                else
                                    stopReached = true;
                            }
                            else
                                stopReached = true;
                        }
                    }



                }
            }
            return FinishEvaluate(false, false);

        }

        protected override int GetAiSelection()
        {

            int aiWin = OpenConnect4(claimedSpaces.Where(x => x.Value == false));
            int playerBlock = OpenConnect4(claimedSpaces.Where(x => x.Value == true));
            Logger.log.Debug($"AI Win Check: {aiWin}");
            Logger.log.Debug($"PlayerBlock Check: {playerBlock}");
            // If 3 connect exists and 4th spot is open, take it to win
            if (aiWin != -1)
                return aiWin;
            //If player has 3 connect and last spot is open, take it to block
            if (playerBlock != -1)
                return playerBlock;
            int result = _choices[rand.Next(_choices.Count)];
            Logger.log.Debug($"Random Result: {result}");
            return result;
        }

        public int OpenConnect4(IEnumerable<KeyValuePair<int, bool>> spaces)
        {
            foreach (var space in spaces)
            {
                int place = space.Key;
                bool player = space.Value;
                int col = -1;
                // Check Vertical: -10
                //     Logger.log.Debug($"Vert Check {place}");
                if (ConnectThreeCheck(place, player, -10, out col))
                    return col;
                // Check Horizontal: +-1
                //    Logger.log.Debug($"Horiz Check {place}");
                if (ConnectThreeCheck(place, player, 1, out col))
                    return col;
                if (ConnectThreeCheck(place, player, -1, out col))
                    return col;

                // Check Diagonal Asc: +-9
                //      Logger.log.Debug($"Diag Asc Check {place}");
                if (ConnectThreeCheck(place, player, -9, out col))
                    return col;
                if (ConnectThreeCheck(place, player, 9, out col))
                    return col;
                // Check Diagonal Desc: +-11
                //         Logger.log.Debug($"Diag Desc Check {place}");
                if (ConnectThreeCheck(place, player, -11, out col))
                    return col;
                if (ConnectThreeCheck(place, player, 11, out col))
                    return col;
            }
            return -1;
        }
        private bool ConnectThreeCheck(int place, bool player, int increment, out int col)
        {
            col = -1;
            bool choosePrev = false;
            int prev = -1;
            int connectCount = 1;
            int mark = place;
            bool stopReached = false;
            while (!stopReached)
            {
                int next = mark + increment;
                int i = next / 10;
                int j = next % 10;

                if (connectCount == 3)
                {
                    if (!choosePrev)
                    {
                        col = next % 10;
                        if (col < 1 || col > 7)
                            break;
                        if (CheckNextSpaceForColumn(col) == next && !claimedSpaces.ContainsKey(col))
                            return true;
                    }
                    else
                    {
                        col = prev % 10;
                        if (col < 1 || col > 7)
                            break;
                        if (CheckNextSpaceForColumn(col) == prev && !claimedSpaces.ContainsKey(col))
                            return true;
                    }
                }
                if (claimedSpaces.TryGetValue(next, out bool owner))
                {
                    if (player == owner)
                    {
                        mark = next;
                        connectCount++;
                        continue;
                    }

                    else
                        stopReached = true;
                }
                else
                {

                    if (j > 0 && j < 8 && i > 1 && i < 7)
                    {
                        if (choosePrev)
                            stopReached = true;
                        choosePrev = true;
                        prev = next;
                        mark = next;
                        continue;
                    }
                    else
                        stopReached = true;
                }
            }


            return false;
        }
        public override string GetStartText()
        {
            return "<size=120%>Choose\n<#FF0000>First  </color><#FFFFFF>Or  </color><#0000FF>Second</color>";
        }

    }
}
