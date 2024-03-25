USE StepNet;
CREATE TABLE container_images (
    ImageID INT AUTO_INCREMENT PRIMARY KEY,
    ImageName VARCHAR(100) NOT NULL,
    GameVersion VARCHAR(20) NOT NULL,
    BuildDate DATETIME NOT NULL,
    ManifestDigest VARCHAR(255) NOT NULL,
    RegistryURL VARCHAR(100) NOT NULL,
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