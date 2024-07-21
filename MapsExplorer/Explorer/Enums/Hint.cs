using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapsExplorer
{
	public enum Hint
	{
		С,
		В,
		Ю,
		З,

		СВ,
		ЮВ,
		ЮЗ,
		СЗ,

		СВ_СЗ,
		СВ_ЮВ,
		ЮЗ_ЮВ,
		СЗ_ЮЗ,

		С_В,
		Ю_В,
		Ю_З,
		С_З,

		СП,
		ВП,
		ЮП,
		ЗП,

		ОчХолодно,   // ✵ 19+
		Холодно,     // ❄ 14-18
		Прохладно,   // ☁ 10-13
		Тепло,       // ♨ 6-9
		Горячо,      // ☀ 3-5
		ОчГорячо,    // ✺ 1-2

		СВ_В,
	}
}
