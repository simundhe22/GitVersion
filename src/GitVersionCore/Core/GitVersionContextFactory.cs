using System;
using GitVersion.Common;
using GitVersion.Configuration;
using GitVersion.Extensions;
using LibGit2Sharp;
using Microsoft.Extensions.Options;

namespace GitVersion
{
    public class GitVersionContextFactory : IGitVersionContextFactory
    {
        private readonly IConfigProvider configProvider;
        private readonly IRepositoryMetadataProvider repositoryMetadataProvider;
        private readonly IBranchConfigurationCalculator branchConfigurationCalculator;
        private readonly IOptions<Arguments> options;

        public GitVersionContextFactory(IConfigProvider configProvider, IRepositoryMetadataProvider repositoryMetadataProvider, IBranchConfigurationCalculator branchConfigurationCalculator, IOptions<Arguments> options)
        {
            this.configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
            this.repositoryMetadataProvider = repositoryMetadataProvider ?? throw new ArgumentNullException(nameof(repositoryMetadataProvider));
            this.branchConfigurationCalculator = branchConfigurationCalculator ?? throw new ArgumentNullException(nameof(branchConfigurationCalculator));
            this.options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public GitVersionContext Create(Arguments arguments)
        {
            var targetBranch = repositoryMetadataProvider.GetTargetBranch(arguments.TargetBranch);
            return Init(targetBranch, arguments.CommitId, arguments.OnlyTrackedBranches);
        }

        private GitVersionContext Init(Branch currentBranch, string commitId = null, bool onlyTrackedBranches = false)
        {
            if (currentBranch == null)
                throw new InvalidOperationException("Need a branch to operate on");

            var configuration = configProvider.Provide(overrideConfig: options.Value.OverrideConfig);

            var currentCommit = repositoryMetadataProvider.GetCurrentCommit(currentBranch, commitId);

            if (currentBranch.IsDetachedHead())
            {
                var branchForCommit = repositoryMetadataProvider.GetBranchesContainingCommit(currentCommit, onlyTrackedBranches: onlyTrackedBranches).OnlyOrDefault();
                currentBranch = branchForCommit ?? currentBranch;
            }

            var currentBranchConfig = branchConfigurationCalculator.GetBranchConfiguration(currentBranch, currentCommit, configuration);
            var effectiveConfiguration = configuration.CalculateEffectiveConfiguration(currentBranchConfig);
            var currentCommitTaggedVersion = repositoryMetadataProvider.GetCurrentCommitTaggedVersion(currentCommit, effectiveConfiguration);

            return new GitVersionContext(currentBranch, currentCommit, configuration, effectiveConfiguration, currentCommitTaggedVersion);
        }
    }
}
