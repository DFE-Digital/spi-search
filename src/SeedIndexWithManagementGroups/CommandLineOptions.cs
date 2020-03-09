using CommandLine;

namespace SeedIndexWithManagementGroups
{
    class CommandLineOptions
    {
        [Option('i', "input", Required = true, HelpText = "Path to input management group JSON file to import")]
        public string InputPath { get; set; }
        
        [Option('s', "source", Required = true, HelpText = "Source of management groups")]
        public string Source { get; set; }
        
        [Option('n', "asc-service-name", Required = true, HelpText = "Name of the Azure Cognitive Search instance")]
        public string AcsServiceName { get; set; }
        
        [Option('k', "asc-key", Required = true, HelpText = "Key of the Azure Cognitive Search instance")]
        public string AcsKey { get; set; }
        
        [Option('x', "index-name", Required = false, Default = "spi-management-groups", HelpText = "Name of the index")]
        public string IndexName { get; set; }
    }
}