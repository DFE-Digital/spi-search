using CommandLine;

namespace SeedIndexWithLearningProviders
{
    class CommandLineOptions
    {
        [Option('i', "input", Required = true, HelpText = "Path to input learning provider JSON file to")]
        public string InputPath { get; set; }
        
        [Option('s', "source", Required = true, HelpText = "Source of learning providers")]
        public string Source { get; set; }
        
        [Option('n', "asc-service-name", Required = true, HelpText = "Name of the Azure Cognitive Search instamce")]
        public string AcsServiceName { get; set; }
        
        [Option('k', "asc-key", Required = true, HelpText = "Key of the Azure Cognitive Search instamce")]
        public string AcsKey { get; set; }
        
        [Option('x', "index-name", Required = false, Default = "spi-learning-providers", HelpText = "Name of the index")]
        public string IndexName { get; set; }
    }
}