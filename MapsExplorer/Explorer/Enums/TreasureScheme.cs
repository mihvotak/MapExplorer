using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum TreasureScheme
{
	Unknown,
	T,
    T_hz,
    T2,
    T2_hz,
	L0, //(не Г)
    L1, //(Г)
    O_0w0,
	O_0w_hz,
	O_1w_2,
	O_1w_1,
	O_1w_hz,
	C,
}

public enum TreasureSchemeKind
{
	Unknown,
	Diagonal,
	Near,
}
