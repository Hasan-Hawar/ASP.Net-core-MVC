using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EmployeeManagment.Models
{
	public class MockEmployeeRepository : IEmployeeRepository
	{
		private List<Employee> employeeList;

		public MockEmployeeRepository()
		{
			employeeList = new List<Employee>()
			{
				new Employee() {Id = 1, Name = "Mary", Depratment = Dept.HR, Email = "mary@pragim.com" },
				new Employee() {Id = 2, Name = "John", Depratment = Dept.IT, Email = "john@pragim.com" },
				new Employee() {Id = 3, Name  = "Sam", Depratment = Dept.IT, Email = "sam@pragim.com" }
		};
		}

		public Employee Add(Employee employee)
		{
			employee.Id = employeeList.Max(e => e.Id) + 1;
			employeeList.Add(employee);
			return employee;
		}
		public Employee Delete(int id)
		{
			Employee employee = employeeList.FirstOrDefault(e => e.Id == id);
			if (employee != null)
			{
				employeeList.Remove(employee);
			}
			return employee;
		}
		public Employee Update(Employee employeeChanges)
		{
			Employee employee = employeeList.FirstOrDefault(e => e.Id == employeeChanges.Id);
			if (employee != null)
			{
				employee.Name = employeeChanges.Name;
				employee.Email = employeeChanges.Email;
				employee.Depratment = employeeChanges.Depratment;
			}
			return employee;
		}
		

		public IEnumerable<Employee> GetAllEmployees()
		{
			return employeeList;
		}
		public Employee GetEmployee(int Id)
		{
			return employeeList.FirstOrDefault(e => e.Id == Id); 
		}

	
	}
}
