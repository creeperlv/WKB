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
			using Repository repository = new Repository(repo);
			var sig = new Signature(u.DisplayName, u.Email, DateTime.Now);
			repository.Commit(message, sig, sig);
		}
		public IEnumerable<(string,string)> History(string path, int StartIndex, int Length)
		{
			if (KBPath is null)yield break;
			var repo = LibGit2Sharp.Repository.Discover(KBPath);
			if (repo == null) yield break; 
			using Repository repository = new Repository(repo);
			foreach(var entry in repository.Commits.QueryBy(path).Take(new Range(StartIndex, StartIndex + Length)))
			{
				yield return (entry.Path,entry.Commit.Sha);
			}
		}
		public void Init(string KBPath)
		{
			this.KBPath = KBPath;
		}
	}
}
