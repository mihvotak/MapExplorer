using System;
using System.Collections.Generic;

public class PolyMap
{
	public int StepNum;
	public int Move;
	
	public List<List<PolyCell>> Cells;
	
	public BossState BossA = new BossState(0, Poly.A);
	public BossState BossB = new BossState(1, Poly.B);
	public BossState BossC = new BossState(2, Poly.C);
	public BossState BossD = new BossState(3, Poly.D);
	public BossState GetBossByLetter(string letter)
	{
		if (letter == Poly.A)
			return BossA;
		if (letter == Poly.B)
			return BossB;
		if (letter == Poly.C)
			return BossC;
		if (letter == Poly.D)
			return BossD;
		return null;
	}
	public BossState GetBossByIndex(int moveIndex)
	{
		if (moveIndex == 0)
			return BossA;
		if (moveIndex == 1)
			return BossB;
		if (moveIndex == 2)
			return BossC;
		if (moveIndex == 3)
			return BossD;
		return null;
	}
}
