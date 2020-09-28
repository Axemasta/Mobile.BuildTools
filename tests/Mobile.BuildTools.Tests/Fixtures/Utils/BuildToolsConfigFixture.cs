﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mobile.BuildTools.Utils;
using Xunit;

namespace Mobile.BuildTools.Tests.Fixtures.Utils
{
    public sealed class BuildToolsConfigFixture : IDisposable
    {
        private static readonly string MockEmptyConfigPath = Path.Combine(Environment.CurrentDirectory, nameof(BuildToolsConfigFixture));

        public BuildToolsConfigFixture()
        {
            Directory.CreateDirectory(MockEmptyConfigPath);
            ConfigHelper.SaveDefaultConfig(MockEmptyConfigPath);
        }

        [Fact]
        public void AppConfigIsNotNull()
        {
            var config = ConfigHelper.GetConfig(MockEmptyConfigPath);

            Assert.NotNull(config.AppConfig);
        }

        [Fact]
        public void ArtifactCopyIsNotNull()
        {
            var config = ConfigHelper.GetConfig(MockEmptyConfigPath);

            Assert.NotNull(config.ArtifactCopy);
        }

        [Fact]
        public void AutomaticVersioningIsNotNull()
        {
            var config = ConfigHelper.GetConfig(MockEmptyConfigPath);

            Assert.NotNull(config.AutomaticVersioning);
        }

        [Fact]
        public void CssIsNotNull()
        {
            var config = ConfigHelper.GetConfig(MockEmptyConfigPath);

            Assert.NotNull(config.Css);
        }

        [Fact]
        public void ImagesIsNotNull()
        {
            var config = ConfigHelper.GetConfig(MockEmptyConfigPath);

            Assert.NotNull(config.Images);
        }

        [Fact]
        public void ManifestsIsNotNull()
        {
            var config = ConfigHelper.GetConfig(MockEmptyConfigPath);

            Assert.NotNull(config.Manifests);
        }

        [Fact]
        public void ProjectSecretsIsNotNull()
        {
            var config = ConfigHelper.GetConfig(MockEmptyConfigPath);

            Assert.NotNull(config.ProjectSecrets);
        }

        [Fact]
        public void ReleaseNotesIsNotNull()
        {
            var config = ConfigHelper.GetConfig(MockEmptyConfigPath);

            Assert.NotNull(config.ReleaseNotes);
        }

        public void Dispose()
        {
            if(File.Exists(MockEmptyConfigPath))
            {
                File.Delete(MockEmptyConfigPath);
            }
        }
    }
}
