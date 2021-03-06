﻿using NSubstitute;
using Octokit.Internal;
using Octokit.Reactive;
using Octokit.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Octokit.Tests.Reactive
{
    public class ObservableIssuesClientTests
    {
        public class TheGetMethod
        {
            [Fact]
            public void GetsFromClientIssueIssue()
            {
                var gitHubClient = Substitute.For<IGitHubClient>();
                var client = new ObservableIssuesClient(gitHubClient);

                client.Get("fake", "repo", 42);

                gitHubClient.Issue.Received().Get("fake", "repo", 42);
            }

            [Fact]
            public async Task EnsuresNonNullArguments()
            {
                var client = new ObservableIssuesClient(Substitute.For<IGitHubClient>());

                await AssertEx.Throws<ArgumentNullException>(async () => await client.Get(null, "name", 1));
                await AssertEx.Throws<ArgumentNullException>(async () => await client.Get("owner", null, 1));
                await AssertEx.Throws<ArgumentException>(async () => await client.Get(null, "", 1));
                await AssertEx.Throws<ArgumentException>(async () => await client.Get("", null, 1));
            }
        }

        public class TheGetForRepositoryMethod
        {
            [Fact]
            public void ReturnsEveryPageOfIssues()
            {
                var firstPageUrl = new Uri("repos/fake/repo/issues", UriKind.Relative);
                var secondPageUrl = new Uri("https://example.com/page/2");
                var firstPageLinks = new Dictionary<string, Uri> { { "next", secondPageUrl } };
                var firstPageResponse = new ApiResponse<List<Issue>>
                {
                    BodyAsObject = new List<Issue>
                    {
                        new Issue {Number = 1},
                        new Issue {Number = 2},
                        new Issue {Number = 3},
                    },
                    ApiInfo = CreateApiInfo(firstPageLinks)
                };
                var thirdPageUrl = new Uri("https://example.com/page/3");
                var secondPageLinks = new Dictionary<string, Uri> { { "next", thirdPageUrl } };
                var secondPageResponse = new ApiResponse<List<Issue>>
                {
                    BodyAsObject = new List<Issue>
                    {
                        new Issue {Number = 4},
                        new Issue {Number = 5},
                        new Issue {Number = 6},
                    },
                    ApiInfo = CreateApiInfo(secondPageLinks)
                };
                var lastPageResponse = new ApiResponse<List<Issue>>
                {
                    BodyAsObject = new List<Issue>
                    {
                        new Issue {Number = 7},
                    },
                    ApiInfo = CreateApiInfo(new Dictionary<string, Uri>())
                };
                var gitHubClient = Substitute.For<IGitHubClient>();
                gitHubClient.Connection.GetAsync<List<Issue>>(Arg.Is(firstPageUrl),
                    Arg.Is<Dictionary<string, string>>(d => d.Count == 4
                        && d["direction"] == "desc"
                        && d["state"] == "open"
                        && d["sort"] == "created"
                        && d["filter"] == "assigned"), Arg.Any<string>())
                    .Returns(Task.Factory.StartNew<IResponse<List<Issue>>>(() => firstPageResponse));
                gitHubClient.Connection.GetAsync<List<Issue>>(secondPageUrl, null, null)
                    .Returns(Task.Factory.StartNew<IResponse<List<Issue>>>(() => secondPageResponse));
                gitHubClient.Connection.GetAsync<List<Issue>>(thirdPageUrl, null, null)
                    .Returns(Task.Factory.StartNew<IResponse<List<Issue>>>(() => lastPageResponse));
                var client = new ObservableIssuesClient(gitHubClient);

                var results = client.GetForRepository("fake", "repo").ToArray().Wait();

                Assert.Equal(7, results.Length);
                Assert.Equal(firstPageResponse.BodyAsObject[0].Number, results[0].Number);
                Assert.Equal(secondPageResponse.BodyAsObject[1].Number, results[4].Number);
                Assert.Equal(lastPageResponse.BodyAsObject[0].Number, results[6].Number);
            }
        }

        public class TheGetAllForOwnedAndMemberRepositoriesMethod
        {
            [Fact]
            public void ReturnsEveryPageOfIssues()
            {
                var firstPageUrl = new Uri("user/issues", UriKind.Relative);
                var secondPageUrl = new Uri("https://example.com/page/2");
                var firstPageLinks = new Dictionary<string, Uri> { { "next", secondPageUrl } };
                var firstPageResponse = new ApiResponse<List<Issue>>
                {
                    BodyAsObject = new List<Issue>
                    {
                        new Issue {Number = 1},
                        new Issue {Number = 2},
                        new Issue {Number = 3},
                    },
                    ApiInfo = CreateApiInfo(firstPageLinks)
                };
                var thirdPageUrl = new Uri("https://example.com/page/3");
                var secondPageLinks = new Dictionary<string, Uri> { { "next", thirdPageUrl } };
                var secondPageResponse = new ApiResponse<List<Issue>>
                {
                    BodyAsObject = new List<Issue>
                    {
                        new Issue {Number = 4},
                        new Issue {Number = 5},
                        new Issue {Number = 6},
                    },
                    ApiInfo = CreateApiInfo(secondPageLinks)
                };
                var lastPageResponse = new ApiResponse<List<Issue>>
                {
                    BodyAsObject = new List<Issue>
                    {
                        new Issue {Number = 7},
                    },
                    ApiInfo = CreateApiInfo(new Dictionary<string, Uri>())
                };
                var gitHubClient = Substitute.For<IGitHubClient>();
                gitHubClient.Connection.GetAsync<List<Issue>>(Arg.Is(firstPageUrl),
                    Arg.Is<Dictionary<string, string>>(d => d.Count == 4
                        && d["direction"] == "desc"
                        && d["state"] == "open"
                        && d["sort"] == "created"
                        && d["filter"] == "assigned"), Arg.Any<string>())
                    .Returns(Task.Factory.StartNew<IResponse<List<Issue>>>(() => firstPageResponse));
                gitHubClient.Connection.GetAsync<List<Issue>>(secondPageUrl, null, null)
                    .Returns(Task.Factory.StartNew<IResponse<List<Issue>>>(() => secondPageResponse));
                gitHubClient.Connection.GetAsync<List<Issue>>(thirdPageUrl, null, null)
                    .Returns(Task.Factory.StartNew<IResponse<List<Issue>>>(() => lastPageResponse));
                var client = new ObservableIssuesClient(gitHubClient);

                var results = client.GetAllForOwnedAndMemberRepositories().ToArray().Wait();

                Assert.Equal(7, results.Length);
                Assert.Equal(firstPageResponse.BodyAsObject[0].Number, results[0].Number);
                Assert.Equal(secondPageResponse.BodyAsObject[1].Number, results[4].Number);
                Assert.Equal(lastPageResponse.BodyAsObject[0].Number, results[6].Number);
            }
        }

        public class TheGetAllForOrganizationMethod
        {
            [Fact]
            public void ReturnsEveryPageOfIssues()
            {
                var firstPageUrl = new Uri("orgs/test/issues", UriKind.Relative);
                var secondPageUrl = new Uri("https://example.com/page/2");
                var firstPageLinks = new Dictionary<string, Uri> { { "next", secondPageUrl } };
                var firstPageResponse = new ApiResponse<List<Issue>>
                {
                    BodyAsObject = new List<Issue>
                    {
                        new Issue {Number = 1},
                        new Issue {Number = 2},
                        new Issue {Number = 3},
                    },
                    ApiInfo = CreateApiInfo(firstPageLinks)
                };
                var thirdPageUrl = new Uri("https://example.com/page/3");
                var secondPageLinks = new Dictionary<string, Uri> { { "next", thirdPageUrl } };
                var secondPageResponse = new ApiResponse<List<Issue>>
                {
                    BodyAsObject = new List<Issue>
                    {
                        new Issue {Number = 4},
                        new Issue {Number = 5},
                        new Issue {Number = 6},
                    },
                    ApiInfo = CreateApiInfo(secondPageLinks)
                };
                var lastPageResponse = new ApiResponse<List<Issue>>
                {
                    BodyAsObject = new List<Issue>
                    {
                        new Issue {Number = 7},
                    },
                    ApiInfo = CreateApiInfo(new Dictionary<string, Uri>())
                };
                var gitHubClient = Substitute.For<IGitHubClient>();
                gitHubClient.Connection.GetAsync<List<Issue>>(Arg.Is(firstPageUrl),
                    Arg.Is<Dictionary<string, string>>(d => d.Count == 4
                        && d["direction"] == "desc"
                        && d["state"] == "open"
                        && d["sort"] == "created"
                        && d["filter"] == "assigned"), Arg.Any<string>())
                    .Returns(Task.Factory.StartNew<IResponse<List<Issue>>>(() => firstPageResponse));
                gitHubClient.Connection.GetAsync<List<Issue>>(secondPageUrl, null, null)
                    .Returns(Task.Factory.StartNew<IResponse<List<Issue>>>(() => secondPageResponse));
                gitHubClient.Connection.GetAsync<List<Issue>>(thirdPageUrl, null, null)
                    .Returns(Task.Factory.StartNew<IResponse<List<Issue>>>(() => lastPageResponse));
                var client = new ObservableIssuesClient(gitHubClient);

                var results = client.GetAllForOrganization("test").ToArray().Wait();

                Assert.Equal(7, results.Length);
                Assert.Equal(firstPageResponse.BodyAsObject[0].Number, results[0].Number);
                Assert.Equal(secondPageResponse.BodyAsObject[1].Number, results[4].Number);
                Assert.Equal(lastPageResponse.BodyAsObject[0].Number, results[6].Number);
            }
        }

        public class TheGetAllForCurrentMethod
        {
            [Fact]
            public void ReturnsEveryPageOfIssues()
            {
                var firstPageUrl = new Uri("issues", UriKind.Relative);
                var secondPageUrl = new Uri("https://example.com/page/2");
                var firstPageLinks = new Dictionary<string, Uri> { { "next", secondPageUrl } };
                var firstPageResponse = new ApiResponse<List<Issue>>
                {
                    BodyAsObject = new List<Issue>
                    {
                        new Issue {Number = 1},
                        new Issue {Number = 2},
                        new Issue {Number = 3},
                    },
                    ApiInfo = CreateApiInfo(firstPageLinks)
                };
                var thirdPageUrl = new Uri("https://example.com/page/3");
                var secondPageLinks = new Dictionary<string, Uri> { { "next", thirdPageUrl } };
                var secondPageResponse = new ApiResponse<List<Issue>>
                {
                    BodyAsObject = new List<Issue>
                    {
                        new Issue {Number = 4},
                        new Issue {Number = 5},
                        new Issue {Number = 6},
                    },
                    ApiInfo = CreateApiInfo(secondPageLinks)
                };
                var lastPageResponse = new ApiResponse<List<Issue>>
                {
                    BodyAsObject = new List<Issue>
                    {
                        new Issue {Number = 7},
                    },
                    ApiInfo = CreateApiInfo(new Dictionary<string, Uri>())
                };
                var gitHubClient = Substitute.For<IGitHubClient>();
                gitHubClient.Connection.GetAsync<List<Issue>>(Arg.Is(firstPageUrl),
                    Arg.Is<Dictionary<string, string>>(d => d.Count == 4
                        && d["direction"] == "desc"
                        && d["state"] == "open"
                        && d["sort"] == "created"
                        && d["filter"] == "assigned"), Arg.Any<string>())
                    .Returns(Task.Factory.StartNew<IResponse<List<Issue>>>(() => firstPageResponse));
                gitHubClient.Connection.GetAsync<List<Issue>>(secondPageUrl, null, null)
                    .Returns(Task.Factory.StartNew<IResponse<List<Issue>>>(() => secondPageResponse));
                gitHubClient.Connection.GetAsync<List<Issue>>(thirdPageUrl, null, null)
                    .Returns(Task.Factory.StartNew<IResponse<List<Issue>>>(() => lastPageResponse));
                var client = new ObservableIssuesClient(gitHubClient);

                var results = client.GetAllForCurrent().ToArray().Wait();

                Assert.Equal(7, results.Length);
                Assert.Equal(firstPageResponse.BodyAsObject[0].Number, results[0].Number);
                Assert.Equal(secondPageResponse.BodyAsObject[1].Number, results[4].Number);
                Assert.Equal(lastPageResponse.BodyAsObject[0].Number, results[6].Number);
            }
        }

        public class TheCreateMethod
        {
            [Fact]
            public void CreatesFromClientIssueIssue()
            {
                var newIssue = new NewIssue("some title");
                var gitHubClient = Substitute.For<IGitHubClient>();
                var client = new ObservableIssuesClient(gitHubClient);

                client.Create("fake", "repo", newIssue);

                gitHubClient.Issue.Received().Create("fake", "repo", newIssue);
            }

            [Fact]
            public async Task EnsuresArgumentsNotNull()
            {
                var gitHubClient = Substitute.For<IGitHubClient>();
                var client = new ObservableIssuesClient(gitHubClient);

                AssertEx.Throws<ArgumentNullException>(async () => await
                    client.Create(null, "name", new NewIssue("title")));
                AssertEx.Throws<ArgumentException>(async () => await
                    client.Create("", "name", new NewIssue("x")));
                AssertEx.Throws<ArgumentNullException>(async () => await
                    client.Create("owner", null, new NewIssue("x")));
                AssertEx.Throws<ArgumentException>(async () => await
                    client.Create("owner", "", new NewIssue("x")));
                AssertEx.Throws<ArgumentNullException>(async () => await
                    client.Create("owner", "name", null));
            }
        }

        public class TheUpdateMethod
        {
            [Fact]
            public void UpdatesClientIssueIssue()
            {
                var IssueUpdate = new IssueUpdate();
                var gitHubClient = Substitute.For<IGitHubClient>();
                var client = new ObservableIssuesClient(gitHubClient);

                client.Update("fake", "repo", 42, IssueUpdate);

                gitHubClient.Issue.Received().Update("fake", "repo", 42, IssueUpdate);
            }

            [Fact]
            public async Task EnsuresArgumentsNotNull()
            {
                var gitHubClient = Substitute.For<IGitHubClient>();
                var client = new ObservableIssuesClient(gitHubClient);

                AssertEx.Throws<ArgumentNullException>(async () => await
                    client.Create(null, "name", new NewIssue("title")));
                AssertEx.Throws<ArgumentException>(async () => await
                    client.Create("", "name", new NewIssue("x")));
                AssertEx.Throws<ArgumentNullException>(async () => await
                    client.Create("owner", null, new NewIssue("x")));
                AssertEx.Throws<ArgumentException>(async () => await
                    client.Create("owner", "", new NewIssue("x")));
                AssertEx.Throws<ArgumentNullException>(async () => await
                    client.Create("owner", "name", null));
            }
        }

        public class TheCtor
        {
            [Fact]
            public void EnsuresArgument()
            {
                Assert.Throws<ArgumentNullException>(() => new IssuesClient(null));
            }
        }

        static ApiInfo CreateApiInfo(IDictionary<string, Uri> links)
        {
            return new ApiInfo(links, new List<string>(), new List<string>(), "etag", new RateLimit(new Dictionary<string, string>()));
        }
    }
}
