using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GameSaber
{
    class GameColorSO : ColorSO
    {
        private Color _colorA;
        private Color _colorB;
        private ColorType _originalType;
        public void Init(Color colorA, Color colorB, ColorType originalType)
        {
            _colorA = colorA;
            _colorB = colorB;
            _originalType = originalType;
        }

        public override Color color
        {
            get
            {
                if(GameController._game != null && GameController.IsPlayerTurn)
                {
                    return 0.9f * (GameController.playerType == PlayerType.First ? _colorB : _colorA);
                }
                else if(GameController._game != null && GameController._game.Started)
                {
                    return GameController.playerType == PlayerType.First ? _colorA : _colorB;
                }
                return _originalType == ColorType.ColorA ? _colorB : _colorA;
            }
        }
    }
}
