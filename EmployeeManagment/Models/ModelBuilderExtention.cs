using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagment.Models
{
	public static class ModelBuilderExtention
	{
		public static void Seed(this ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Employee>().HasData(
				new Employee
				{
					Id = 1,
					Name = "Mary",
					Depratment = Dept.HR,
					Email = "mary@pragomtech.com"
				},
				new Employee
				{
					Id = 2,
					Name = "John",
					Depratment = Dept.Payroll,
					Email = "john@pragomtech.com"
				},
				new Employee
				{
					Id = 3,
					Name = "Hasan",
					Depratment = Dept.IT,
					Email = "hasan@pragomtech.com"
				}
				);
		}
	}
}
