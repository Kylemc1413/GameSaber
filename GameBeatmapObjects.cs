using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSaber
{
    public enum PlayerType { First, Second};

    public class GameNote : NoteData
    {
        public PlayerType playerType;
        public int selectionNum;
        public GameNote(PlayerType type, int selectionNum, float time, int lineIndex, NoteLineLayer noteLineLayer, NoteLineLayer startNoteLineLayer, ColorType noteType, NoteCutDirection cutDirection, float timeToNextBasicNote, float timeToPrevBasicNote) : base(time, lineIndex, noteLineLayer, startNoteLineLayer, noteType, cutDirection, timeToNextBasicNote, timeToPrevBasicNote, lineIndex, 0f, 0f)
        {
            playerType = type;
            this.selectionNum = selectionNum;
        }

    }

    public class GameObstacle : ObstacleData
    {
        public GameObstacle(float time, int lineIndex, ObstacleType obstacleType, float duration, int width) : base(time, lineIndex, obstacleType, duration, width)
        {
        }
    }
}
