using System;
using GitVersion.BuildServers;
using GitVersion.Common;
using GitVersion.Configuration;
using GitVersion.Configuration.Init;
using GitVersion.Extensions;
using GitVersion.Logging;
using GitVersion.VersionCalculation;
using GitVersion.VersionCalculation.Cache;
using GitVersion.VersionConverters.GitVersionInformationResources;
using GitVersion.VersionConverters.VersionAssemblyInfoResources;
using GitVersion.VersionConverters.WixUpdater;
using LibGit2Sharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GitVersion
{
    public class GitVersionCoreModule : IGitVersionModule
    {
        public void RegisterTypes(IServiceCollection services)
        {
            services.AddSingleton<ILog, Log>();
            services.AddSingleton<IFileSystem, FileSystem>();
            services.AddSingleton<IEnvironment, Environment>();
            services.AddSingleton<IRepository, GitRepository>();

            services.AddSingleton<IConsole, ConsoleAdapter>();
            services.AddSingleton<IGitVersionCache, GitVersionCache>();

            services.AddSingleton<IGitVersionCacheKeyFactory, GitVersionCacheKeyFactory>();
            services.AddSingleton<IGitVersionContextFactory, GitVersionContextFactory>();
            services.AddSingleton<IConfigFileLocatorFactory, ConfigFileLocatorFactory>();

            services.AddSingleton<IConfigProvider, ConfigProvider>();
            services.AddSingleton<IVariableProvider, VariableProvider>();

            services.AddSingleton<IBaseVersionCalculator, BaseVersionCalculator>();
            services.AddSingleton<IMainlineVersionCalculator, MainlineVersionCalculator>();
            services.AddSingleton<INextVersionCalculator, NextVersionCalculator>();
            services.AddSingleton<IGitVersionTool, GitVersionTool>();
            services.AddSingleton<IBranchConfigurationCalculator, BranchConfigurationCalculator>();

            services.AddSingleton<IBuildServerResolver, BuildServerResolver>();
            services.AddSingleton<IGitPreparer, GitPreparer>();
            services.AddSingleton<IRepositoryMetadataProvider, RepositoryMetadataProvider>();

            services.AddSingleton<IGitVersionInformationGenerator, GitVersionInformationGenerator>();
            services.AddSingleton<IWixVersionFileUpdater, WixVersionFileUpdater>();
            services.AddSingleton<IAssemblyInfoFileUpdater, AssemblyInfoFileUpdater>();

            services.AddSingleton(sp => sp.GetService<IConfigFileLocatorFactory>().Create());

            services.AddSingleton(sp =>
            {
                var options = sp.GetService<IOptions<Arguments>>();
                var contextFactory = sp.GetService<IGitVersionContextFactory>();
                return new Lazy<GitVersionContext>(() => contextFactory.Create(options.Value));
            });

            services.AddModule(new BuildServerModule());
            services.AddModule(new GitVersionInitModule());
            services.AddModule(new VersionStrategyModule());
        }
    }
}
