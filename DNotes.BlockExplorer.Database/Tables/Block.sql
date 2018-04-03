CREATE TABLE [dbo].[Block]
(
	[Id] int NOT NULL IDENTITY,
	[Hash] VARCHAR(64) NOT NULL,
	[HashMerkleRoot] VARCHAR(64) NOT NULL, 
    [HashPrevBlock] VARCHAR(64) NULL,
	[Height] INT NOT NULL, 
    [Time] DATETIMEOFFSET NOT NULL, 
    [Bits] INT NOT NULL, 
    [Version] INT NOT NULL, 
    [Nonce] INT NOT NULL, 
    [Type] SMALLINT NOT NULL,
	CONSTRAINT [PK_Block] PRIMARY KEY ([Id])
)

