CREATE TABLE container_image (
    object_id INT IDENTITY(1,1) PRIMARY KEY,
    unique_identifier VARCHAR(255) NOT NULL,
    container_image_name VARCHAR(255) NOT NULL, 
    container_image_tag VARCHAR(255), -- Optional 
    created_at DATETIME2 DEFAULT GETDATE()
);
GO

CREATE UNIQUE INDEX idx_container_image_unique_identifier 
ON container_image (unique_identifier);
GO