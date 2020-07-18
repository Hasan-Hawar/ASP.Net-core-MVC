using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagment.Secutity
{
	public class CustomConfirmationTokenProvider<TUser>
		: DataProtectorTokenProvider<TUser> where TUser : class
	{
		public CustomConfirmationTokenProvider(IDataProtectionProvider dataProtectionProvider, 
			IOptions<CustomEmailConfirmationTokenProviderOptions> options)
			: base(dataProtectionProvider, options)
		{

		}
	}
}
