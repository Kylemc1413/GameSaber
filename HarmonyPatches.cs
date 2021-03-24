using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using TMPro;
using UnityEngine;
namespace GameSaber
{
    [HarmonyPatch(typeof(FlyingTextEffect), "InitAndPresent")]
    public class FlyingTextRichTextPatch
    {
        public static void Prefix(ref TextMeshPro ____text)
        {
            if (GameController.mapParams == null) return;
            ____text.richText = true;
            ____text.enableWordWrapping = false;
        }
    }
    [HarmonyPatch(typeof(BeatmapObjectManager), "HandleNoteControllerNoteWasMissed")]
    public class GameNoteMiss
    {
        public static bool Prefix(ref NoteController noteController)
        {
            if(noteController.noteData is GameNote)
            {
                return false;
            }
            return true;
        }
    }
    
    [HarmonyPatch(typeof(GameNoteController), "NoteDidStartJump")]
    public class GameNoteHitting
    {
        public static void Postfix(ref GameNoteController __instance, ref BoxCuttableBySaber[] ____bigCuttableBySaberList, ref Transform ____noteTransform)
        {
            ____noteTransform.localScale = new Vector3(1f, 1f, 1f);
            if (__instance.noteData is GameNote)
            {
                foreach(var bigCuttable in ____bigCuttableBySaberList)
                    bigCuttable.canBeCut = false;
                if (GameController.mapParams?.gameType == GameType.TicTacToe)
                    ____noteTransform.localScale *= 0.6f;
            }
        }
    }
    
    [HarmonyPatch(typeof(ObstacleController), "Init"), HarmonyPriority(Priority.LowerThanNormal)]
    public class ObstacleSpawn
    {
        public static void Prefix(ref ObstacleData obstacleData)
        {
           if(obstacleData is GameObstacle)
            {
         //       ____color.SetColor(Color.blue * 0.4f);
            }
        }
    }

}
