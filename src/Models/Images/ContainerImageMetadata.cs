using System;
using System.ComponentModel.DataAnnotations;

namespace StepNet.API.Models.Images
{
    public class ContainerImageMetadata
    {
        public int Id { get; set; }

        [Required]
        public string ImageName { get; set; }

        [Required]
        public string ImageTag { get; set; }

        public DateTime BuildDate { get; set; }

        public string SourceCodeRepository { get; set; }

        public string CommitHash { get; set; }

        public string BuildPipeline { get; set; }

        public string Description { get; set; }

        public string Maintainer { get; set; }

        public string DocumentationURL { get; set; }

        public string Dependencies { get; set; }

        // ... Add fields for Vulnerability Scan Results, License Info, etc.

        public int MemoryRequirement { get; set; }

        public int CPURequirement { get; set; }

        public string EnvironmentVariables { get; set; }

        public string ExposedPorts { get; set; }

        public string HealthChecks { get; set; }
    }
}
