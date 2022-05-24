using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapsExplorer
{
	public class Dunge
	{
		public DungeLine DungeLine;
		public int Steps;
		public bool[] CanBeHints = new bool[101];
		public int UniqueSteps;
		public List<Boss> Bosses = new List<Boss>();
		public List<VoiceKind>[] Voices;
		public List<string> VoiceTexts = new List<string>();
		public List<Member> Members = new List<Member>();
		public int MembersSumHp;
		public List<Step> Moves = new List<Step>();
		public List<int> MoveFloors = new List<int>();
		public int FirstStairMove;
		public List<int[]> Hps = new List<int[]>();
		public List<int> HintMoves = new List<int>();
		public List<int> HintCounters = new List<int>();
		public SecretRom SecretRom = new SecretRom();
		public bool IsCustom;
		public string UnknownImportant;
		public string UnknownSecretResult;
		public string UnknownMoveDir;
		public string UnknownHint;
		public bool MiniQuest;
		public DungeKind StartKind;

		public int LastFloor;
		public Map[] Maps = new Map[2];
		public Step TreasurePos;
		public bool TreasureBetweenWalls;
		public TreasureSchemeKind TreasureSchemeKind;
		public TreasureScheme TreasureScheme;
		public bool AllBossesFound;

		public Stable Stable;
		public int Rotation;

		public bool HintOnCache;

		public bool WrongDetectedAqua;

		public Dunge(DungeLine dungeLine)
		{
			DungeLine = dungeLine;
		}

		public Cell GetCell(Step step)
		{
			return Maps[step.Floor - 1].GetCell(step.Pos.x, step.Pos.y);
		}

		public Cell GetCell(Int2 pos, int floor)
		{
			return Maps[floor - 1].GetCell(pos.x, pos.y);
		}


		public Cell GetCell(int moveStep)
		{
			Step step = Moves[moveStep - 1];
			return GetCell(step);
		}

		public bool IsNormalBosses()
		{
			return DungeLine.Kind != DungeKind.Миграции && DungeLine.Kind != DungeKind.Загадки && DungeLine.Kind != DungeKind.Заброшенности;
		}
	}
}
