

using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Octokit;
using Octokit.Reactive;

using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.Serialization;


//GROUP 12
//To run this code you need to set the GITHUB_TOKEN environment variable to a valid GitHub personal access token.
// then the command dotnet run
class Program
{
    private const string Owner = "yt-dlp";
    private const string Repo = "yt-dlp";
    private const string? Branch = null; // null means default branch



    public static async Task Main()
    {
        var token = Environment.GetEnvironmentVariable("GITHUB_TOKEN")
                   ?? throw new InvalidOperationException("Set GITHUB_TOKEN env var.");

        // Authenticated REST client (for repo/branch checks)
        var rest = new GitHubClient(new ProductHeaderValue("Capstone"))
        {
            Credentials = new Credentials(token)
        };

        var rx = new ObservableGitHubClient(rest);

        var since = DateTimeOffset.UtcNow.AddYears(-1);
        var later_since = DateTimeOffset.UtcNow.AddYears(-2);
        //var targetBranch = "master";
        //var req = new CommitRequest { Sha = targetBranch };

        /*

                var Commits = await rx.Repository.Commit
                    .GetAll(Owner, Repo, req)
                    .ToList();



                var countCommits = Commits.Count();

                Console.WriteLine($"All commits: ({Owner}/{Repo}@{targetBranch}): {countCommits}");

                var prRequest = new PullRequestRequest
                {
                    State = ItemStateFilter.All, //All will include both open and closed
                    SortProperty = PullRequestSort.Created,
                    SortDirection = SortDirection.Descending
                };

                var prList = await rx.PullRequest
                    .GetAllForRepository(Owner, Repo, prRequest)
                    .ToList()
                    .ToTask();

                var prCountClosed = prList.Count(pr => pr.State == ItemState.Closed);

                var prCountOpen = prList.Count(pr => pr.State == ItemState.Open);

                Console.WriteLine($"Pull requests: ({Owner}/{Repo}): \nAll:{prList.Count()}\nClosed:{prCountClosed}\nOpen:{prCountOpen}");


                var issueRequest = new RepositoryIssueRequest
                {
                    State = ItemStateFilter.All, // All includes open and closed
                    SortProperty = IssueSort.Created,
                    SortDirection = SortDirection.Descending,
                };


                var allIssues = await rest.Issue.GetAllForRepository(Owner, Repo, issueRequest);
                var issueList = allIssues
                    .Where(i => i.PullRequest == null)
                    .ToList();

                var issueCountAll = issueList.Count;
                var issueCountOpen = issueList.Count(i => i.State.Value == ItemState.Open);
                var issueCountClosed = issueList.Count(i => i.State.Value == ItemState.Closed);


                Console.WriteLine(
                    $"Issues in ({Owner}/{Repo}):\nAll: {issueCountAll}\nOpen: {issueCountOpen}\nClosed: {issueCountClosed}"
                );

                var commitsByContributor = Commits
                    .GroupBy(c => c.Author?.Login ?? "Unknown")
                    .Select(g => new { Contributor = g.Key, CommitCount = g.Count() })
                    .OrderByDescending(x => x.CommitCount)
                    .ToList();

                //int rank = 1;
                // foreach (var x in commitsByContributor)
                //  {
                //      Console.WriteLine($"{rank}. {x.Contributor}: {x.CommitCount} commit{(x.CommitCount == 1 ? "" : "s")}");
                //      rank++;
                //  }

                Console.WriteLine("Check point 1");
                var issuesByContributor = issueList
                    .GroupBy(i => i.User?.Login ?? "Unknown")
                    .Select(g => new { Contributor = g.Key, IssueCount = g.Count() })
                    .OrderByDescending(x => x.IssueCount)
                    .ToList();

                var prByContributor = prList
                    .GroupBy(pr => pr.User?.Login ?? "Unknown")
                    .Select(g => new { Contributor = g.Key, PRCount = g.Count() })
                    .OrderByDescending(x => x.PRCount)
                    .ToList();



                // Newcommers and dormant contributors
                Console.WriteLine("Check point 2");

                var last_year_commits = await rx.Repository.Commit
                    .GetAll(Owner, Repo, req)
                    .TakeWhile(c => c.Commit?.Author?.Date >= since)
                    .Where(c => c.Commit?.Author?.Date >= since)
                    .ToList();


                var newcommers_commits = last_year_commits
                    .GroupBy(c => c.Author?.Login ?? c.Commit?.Author?.Name ?? "Unknown")
                    .Select(g => new
                    {
                        Contributor = g.Key,
                        CommitCount = g.Count(),
                    })
                    .ToList();



                var last_year_issues = allIssues
                    .TakeWhile(i => i.CreatedAt >= since)
                    .Where(i => i.PullRequest == null && i.CreatedAt >= since)
                    .ToList();

                Console.WriteLine("Check point 3");

                var newcommers_issues = last_year_issues
                .GroupBy(i => i?.User?.Login ?? "Unknown")
                .Select(g => new
                {
                    Contributor = g.Key,
                    IssueCount = g.Count(),
                })
                .ToList();

                var last_year_prs = prList
                    .TakeWhile(i => i.CreatedAt >= since)
                    .Where(i => i.CreatedAt >= since)
                    .ToList();

                var newcommers_prs = last_year_prs
                    .GroupBy(i => i?.User?.Login ?? "Unknown")
                    .Select(g => new
                    {
                        Contributor = g.Key,
                        PrCount = g.Count(),
                    })
                    .ToList();

                Console.WriteLine("Check point 4");



                // Console.WriteLine("Newcomers in the last year:");
                // foreach (var newcomer in newcommers_prs)
                // {
                //     Console.WriteLine($"{rank}. Contributor:  {newcomer.Contributor} - Issues: {newcomer.PrCount}");
                //     rank++;
                // }
                Console.WriteLine("Check point 4.5");


                var baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                Export.ToCsv(commitsByContributor, Path.Combine(baseDirectory, "commits_by_contributor.csv"),
                    ("Contributor", X => X.Contributor),
                    ("CommitCount", X => X.CommitCount));
                Export.ToCsv(issuesByContributor, Path.Combine(baseDirectory, "issues_by_contributor.csv"),
                    ("Contributor", X => X.Contributor),
                    ("IssueCount", X => X.IssueCount));
                Export.ToCsv(prByContributor, Path.Combine(baseDirectory, "prs_by_contributor.csv"),
                    ("Contributor", X => X.Contributor),
                    ("PRCount", X => X.PRCount));

                Console.WriteLine("Check point 5");

                Export.ToCsv(newcommers_commits, Path.Combine(baseDirectory, "newcomers_commits.csv"),
                    ("Contributor", X => X.Contributor),
                    ("CommitCount", X => X.CommitCount));
                Export.ToCsv(newcommers_issues, Path.Combine(baseDirectory, "newcomers_issues.csv"),
                    ("Contributor", X => X.Contributor),
                    ("IssueCount", X => X.IssueCount));
                Export.ToCsv(newcommers_prs, Path.Combine(baseDirectory, "newcomers_prs.csv"),
                    ("Contributor", X => X.Contributor),
                    ("PrCount", X => X.PrCount));
        */
        // Average first response time to issues for Q3
        // Fetch latest 50 closed issues
        var issues = await rest.Issue.GetAllForRepository(Owner, Repo, new RepositoryIssueRequest
        {
            State = ItemStateFilter.Closed
        });

        int count = 0;
        double totalHours = 0;

        foreach (var issue in issues.Take(7000))
        {
            var comments = await rest.Issue.Comment.GetAllForIssue(Owner, Repo, issue.Number);

            if (comments.Count > 0)
            {
                var firstComment = comments.OrderBy(c => c.CreatedAt).First();
                var delta = firstComment.CreatedAt - issue.CreatedAt;
                totalHours += delta.TotalHours;
                count++;
            }
        }

        if (count > 0)
        {
            double avg = totalHours / count;
            Console.WriteLine($"Average first response time: {avg:F2} hours ({count} issues analyzed)");
        }
        else
        {
            Console.WriteLine("No comments found for analyzed issues.");
        }

    }
}