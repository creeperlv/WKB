using System.Diagnostics;
using System.IO;
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

		public IEnumerable<(string, string)> History(string path, int StartIndex, int Length)
		{
			var i = Environment.CommandLine;
			ProcessStartInfo psi = new ProcessStartInfo("git");
			psi.WorkingDirectory = KBPath;
			psi.Arguments = $"log -- {path}";
			psi.RedirectStandardOutput = true;
			var process = Process.Start(psi);
			if (process is null) yield break;
			while (true)
			{
				var l = process.StandardOutput.ReadLine();
				if (l == null) yield break;
				if (l.StartsWith("commit "))
				{
					yield return (path, l["commit ".Length..]);
				}
			}
		}

		public void Init(string KBPath)
		{
			this.KBPath = KBPath;
		}

		public bool IsAvailable()
		{
			var i = Environment.CommandLine;
			ProcessStartInfo psi = new ProcessStartInfo("git");
			psi.WorkingDirectory = KBPath;
			psi.Arguments = $"status";
			psi.RedirectStandardOutput = true;
			var process = Process.Start(psi);
			if (process is null) return false;
			while (true)
			{
				var l = process.StandardOutput.ReadLine();
				if (l == null) return true;
				if (l.StartsWith("fatal: not a git repository"))
				{
					return false;
				}
				if (l.StartsWith("On branch"))
				{
					return true;
				}
			}
		}
	}
}
