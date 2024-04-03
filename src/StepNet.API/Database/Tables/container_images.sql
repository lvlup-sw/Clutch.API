USE StepNet;
CREATE TABLE container_images (
    ImageID INT AUTO_INCREMENT PRIMARY KEY,
    ImageReference VARCHAR(255) NOT NULL, 
    GameVersion INT NOT NULL,
    BuildDate DATETIME NOT NULL,
    RegistryURL VARCHAR(100) NOT NULL,
    Status INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP NOT NULL,
    Plugins JSON,
    CommitHash VARCHAR(40),
    Layers INT,
    Size BIGINT,
    ServerConfig TEXT,
    Notes TEXT
);

CREATE UNIQUE INDEX idx_container_images_id
ON container_images (ImageID);

CREATE UNIQUE INDEX idx_container_images_reference 
ON container_images (imageReference);