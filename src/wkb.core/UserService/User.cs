using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wkb.core.UserService
{
	[Serializable]
	public class User
	{
		public string DisplayName { get;set;}="User";
		public string Email { get;set;}="null@example.com";
		public string PasswordHash { get;set;}="";
	}
}
