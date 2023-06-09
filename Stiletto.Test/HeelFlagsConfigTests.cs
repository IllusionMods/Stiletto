using Stiletto.Configurations;
using Stiletto.Models;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using Xunit;

namespace Stiletto.Test
{
    public class HeelFlagsConfigTests
    {
        [Fact]
        public void HeelFlagsShouldLoaded()
        {
            var file = GetResourcesFile("Flags/02_hsense.txt");
            var config = new AnimationSettingsCollection<AnimationFlags>(file);
            var flags = config.Load("kha_f_00", "S_Idle");

            Assert.True(flags.ACTIVE);
        }

        private string GetResourcesFile(string fileName) 
        {
            return Path.Combine(AppContext.BaseDirectory, "Resources", fileName);
        }
    }
}
