using Cake.Common.Build;
using Cake.Common.Build.GitHubActions;
using Cake.Common.Build.GitHubActions.Commands;
using Cake.Common.Build.GitHubActions.Data;
using Cake.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Build
{
    internal class CustomGithubActionsProvider : IGitHubActionsProvider
    {
        private GitHubActionsProvider _provider;
        public bool IsRunningOnGitHubActions => _provider?.IsRunningOnGitHubActions ?? false;

        public GitHubActionsEnvironmentInfo Environment => _provider?.Environment;

        public GitHubActionsCommands Commands => _provider?.Commands;

        public CustomGithubActionsProvider(BuildContext ctx)
        {
            try
            {
                _provider = ctx.GitHubActions() as GitHubActionsProvider;
            }
            catch { }
        }
    }
}
