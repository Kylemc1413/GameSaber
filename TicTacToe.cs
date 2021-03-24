using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
namespace GameSaber
{
    public class TicTacToe : IGame
    {

        private char _aiChar = 'O';
        private char _playerChar = 'X';

        public override string GetGameText()
        {
            string result = _GameText;
            foreach (var i in _playerChoices)
                result = result.Replace(i.ToString(), $"<#00FFAA>{_playerChar}</color>");
            foreach (var i in _aiChoices)
                result = result.Replace(i.ToString(), $"<#FF0000>{_aiChar}</color>");
            foreach (var i in _choices)
                result = result.Replace(i.ToString(), "<#FFFFFF>?</color>");
            return result;
        }

        private List<int> _playerChoices = new List<int>();
        private List<int> _aiChoices = new List<int>();

        private static List<int[]> winCombinations = new List<int[]>
        {
            new int[] {1,2,3},
            new int[] {4,5,6},
            new int[] {7,8,9},
            new int[] {1,4,7},
            new int[] {2,5,8},
            new int[] {3,6,9},
            new int[] {1,5,9},
            new int[] {3,5,7}
        };

        protected override void Init(bool playerIsX)
        {
            _choices = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            _GameText =
            "<#FFFFFF>" +
            "1   |   2   |   3 \n" +
            "------------\n" +
            "4   |   5   |   6 \n" +
            "------------\n" +
            "7   |   8   |   9 \n" +
            "</color>";

            if (playerIsX)
            {
                _playerChar = 'X';
                _aiChar = 'O';

            }
            else
            {
                _aiChar = 'X';
                _playerChar = 'O';
            }
        }

        public override void PlayerTurn(int selection)
        {
            if (!_choices.Contains(selection))
                selection = _choices.First();
            _choices.Remove(selection);
            _playerChoices.Add(selection);
            if (Evaluate()) return;
            aiTurnStart?.Invoke(_choices);
            AITurn();
        }
        protected override async void AITurn()
        {

            await Task.Delay((int)(TurnLength * 1000));
            int aiSelection = GetAiSelection();
            _choices.Remove(aiSelection);
            _aiChoices.Add(aiSelection);
            if (Evaluate()) return;
            playerTurnStart?.Invoke(_choices);

        }

        protected override int GetAiSelection()
        {
            int selection = -1;
            List<int> validChoices = new List<int>();
            var combSelection = _choices.Concat(_aiChoices);
            foreach (int i in _choices)
            {
                if (winCombinations.Any(x => x.Intersect(combSelection).Count() == 3 && x.Contains(i)))
                    validChoices.Add(i);
            }
            if (_choices.Any(x => winCombinations.Any(y => _aiChoices.Append(x).Intersect(y).Count() == 3)))
            {
                validChoices.Clear();
                validChoices.Add(_choices.First(x => winCombinations.Any(y => _aiChoices.Append(x).Intersect(y).Count() == 3)));
            }
            if (!(validChoices.Count > 0))
                selection = _choices[rand.Next(_choices.Count)];
            else
                selection = validChoices[rand.Next(validChoices.Count)];
            return selection;

        }

        protected override bool Evaluate()
        {
            bool playerWon = winCombinations.Any(x => x.Intersect(_playerChoices).Count() == 3);
            bool aiWon = winCombinations.Any(x => x.Intersect(_aiChoices).Count() == 3);
            if (playerWon || aiWon || _choices.Count == 0)
            {
                gameHasEnded?.Invoke(playerWon, (!aiWon && !playerWon));
                return true;
            }
            return false;
        }

        public override string GetStartText()
        {
            return "<size=120%>Choose\n<#FF0000>X  </color><#FFFFFF>Or  </color><#0000FF>O</color>";
        }
    }
}
