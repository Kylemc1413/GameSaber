using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
namespace GameSaber
{
    public abstract class IGame
    {
        public System.Random rand = new Random();
        public Action<bool, bool> gameHasEnded;
        public Action<List<int>> playerTurnStart;
        public Action<List<int>> aiTurnStart;
        protected List<int> _choices;
        public float TurnLength { get; private set; } = 0;
        public bool Started { get; private set; } = false;
        protected string _GameText;
        public abstract string GetGameText();

        public void Start(bool playerIsFirst, float turnlength)
        {
            if (Started) return;
            Init(playerIsFirst);
            TurnLength = turnlength;
            if (playerIsFirst)
            {
                playerTurnStart?.Invoke(_choices);
            }
            else
            {
                aiTurnStart?.Invoke(_choices);
                AITurn();
            }
            Started = true;
        }
        public abstract string GetStartText();
        protected virtual void Init(bool playerIsFirst)
        {

        }
        public abstract void PlayerTurn(int selection);

        protected abstract void AITurn();

        protected abstract int GetAiSelection();


        protected abstract bool Evaluate();

    }
}
