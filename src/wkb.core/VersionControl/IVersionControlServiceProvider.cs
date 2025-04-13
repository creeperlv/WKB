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
		bool IsAvailable();
		void Init(string KBPath);
		void Commit(User u, string message);
		IEnumerable<(string, string)> History(string path, int StartIndex, int Length);
	}
	public enum VersionControlService
	{
		Flat, GitCommand, LibGit2Sharp,
	}
}
