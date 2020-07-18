using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;

namespace EmployeeManagment
{
	public static class StringHelper
	{
		public static string ChangeFirstLetterCase(this string inputStr)
		{
			if(inputStr.Length > 0)
			{
				char[] charArray = inputStr.ToCharArray();
				charArray[0] = char.IsUpper(charArray[0]) ? char.ToLower(charArray[0]) : char.ToUpper(charArray[0]);
				return new string(charArray);
			}
			return inputStr;
		}
	}
}
