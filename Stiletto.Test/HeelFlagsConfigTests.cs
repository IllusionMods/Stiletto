using Stiletto.Configurations;
using System;
using System.IO;
using Xunit;

namespace Stiletto.Test
{
    public class HeelFlagsConfigTests
    {
        [Fact]
        public void HeelFlagsShouldLoaded()
        {
            var file = GetResourcesFile("02_hsense.txt");
            var config = new HeelFlagsConfig(file);
            var flags = config.GetHeelFlags("kha_f_00", "S_Idle");

        }

        private string GetResourcesFile(string fileName) 
        {
            return Path.Combine(AppContext.BaseDirectory, "Resources", fileName);
        }
    }
}
