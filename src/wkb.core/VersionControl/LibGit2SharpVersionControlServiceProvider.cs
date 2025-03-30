using LibGit2Sharp;
using wkb.core.UserService;

namespace wkb.core.VersionControl
{
	public class LibGit2SharpVersionControlServiceProvider : IVersionControlServiceProvider
	{
		string? KBPath;
		public void Commit(User u, string message)
		{
			if (KBPath is null) return;

			var repo = LibGit2Sharp.Repository.Discover(KBPath);
			if (repo == null) return;
			Repository repository = new Repository(repo);
			var sig = new Signature(u.DisplayName, u.Email, DateTime.Now);
			repository.Commit(message, sig, sig);
		}

		public void Init(string KBPath)
		{
			throw new NotImplementedException();
		}
	}
}
