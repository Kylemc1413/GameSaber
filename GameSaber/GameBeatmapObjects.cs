﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSaber
{
    public enum PlayerType { First, Second};

    public class GameNote : NoteData
    {
        public override int subtypeGroupIdentifier
        {
            get
            {
                return 1413;
            }
        }
        public PlayerType playerType;
        public int selectionNum;
        public GameNote(PlayerType type, int selectionNum, float time, int lineIndex, NoteLineLayer noteLineLayer, NoteLineLayer startNoteLineLayer, ColorType noteType, NoteCutDirection cutDirection, float timeToNextBasicNote, float timeToPrevBasicNote) : base(time, lineIndex, noteLineLayer, startNoteLineLayer, GameplayType.Normal, ScoringType.NoScore, noteType, cutDirection, timeToNextBasicNote, timeToPrevBasicNote, lineIndex, 0f, 0f, 1f)
        {
            playerType = type;
            this.selectionNum = selectionNum;
        }

    }

    public class GameObstacle : ObstacleData
    {
        public override int subtypeGroupIdentifier
        {
            get
            {
                return 1414;
            }
        }
        public GameObstacle(float time, int lineIndex, NoteLineLayer layer, float duration, int width, int height) : base(time, lineIndex, layer, duration, width, height)
        {
        }
    }
}
