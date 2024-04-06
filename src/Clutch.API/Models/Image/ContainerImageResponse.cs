﻿namespace Clutch.API.Models.Image
{
    public class ContainerImageResponse(bool success, ContainerImage image, RegistryManifest properties)
    {
        public bool Success { get; set; } = success;
        public ContainerImage ContainerImage { get; set; } = image;
        public RegistryManifest RegistryManifest { get; set; } = properties;
    }
}