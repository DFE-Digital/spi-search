using CommandLine;

namespace GiasDataLoader
{
    class CommandLineOptions
    {
        [Option('e', "establishments-path", HelpText = "Path to Establishments file to load into index")]
        public string EstablishmentsFilePath { get; set; }
        
        [Option('s', "service-name", Required = true, HelpText = "Azure cognitive search service name")]
        public string SearchServiceName { get; set; }
        
        [Option('k', "service-key", Required = true, HelpText = "Azure cognitive search service API key")]
        public string SearchServiceKey { get; set; }
        
        [Option('i', "index-name", Required = true, HelpText = "Name of the search index")]
        public string IndexName { get; set; }
    }
}