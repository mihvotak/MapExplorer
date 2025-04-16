
public class BossState
{
	public int BossIndex;

	public string Letter;

	public Int2 Pos;
	public Int2 DeltaPos = Int2.Zero;
	
	public int HP;
	public int DeltaHP;
	public bool IsAlive => HP > 0;
	
	public int Bits;
	public int DeltaBits;
	
	public string ArrowStr;
	public Int2 ArrowDir => Poly.GetDirByArrow(ArrowStr);

	public InfluenceKind Influence;

	public BossState(int bossIndex, string letter)
	{
		BossIndex = bossIndex;
		Letter = letter;
	}
}
