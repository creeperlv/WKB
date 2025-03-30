using System.Diagnostics;
using wkb.core.UserService;

namespace wkb.core.VersionControl
{
	public class GitCommandVersionControlServiceProvider : IVersionControlServiceProvider
	{
		string? KBPath;
		public void Commit(User u, string message)
		{
			if (KBPath is null) return;

			ProcessStartInfo psi = new ProcessStartInfo("git");
			psi.WorkingDirectory = KBPath;
			psi.EnvironmentVariables.Add("GIT_AUTHOR_NAME", u.DisplayName);
			psi.EnvironmentVariables.Add("GIT_AUTHOR_EMAIL", u.Email);
			psi.EnvironmentVariables.Add("GIT_COMMITTER_NAME", u.DisplayName);
			psi.EnvironmentVariables.Add("GIT_COMMITTER_EMAIL", u.Email);
			psi.Arguments = $"commit -m \"{message}\" --author";
			Process.Start(psi)?.WaitForExit();
		}

		public void Init(string KBPath)
		{
			this.KBPath = KBPath;
		}
	}
}
