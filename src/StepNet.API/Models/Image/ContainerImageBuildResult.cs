namespace StepNet.API.Models.Image
{
    public class ContainerImageBuildResult
    {
        public bool Success { get; set; }

        public required ContainerImageModel ContainerImageModel { get; set; }
    }
}
