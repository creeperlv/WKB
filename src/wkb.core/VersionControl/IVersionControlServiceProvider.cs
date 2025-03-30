using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wkb.core.UserService;

namespace wkb.core.VersionControl
{
	public interface IVersionControlServiceProvider
	{
		void Init(string KBPath);
		void Commit(User u, string message);
	}
	public enum VersionControlService
	{
		Flat, GitCommand, LibGit2Sharp,
	}
}
