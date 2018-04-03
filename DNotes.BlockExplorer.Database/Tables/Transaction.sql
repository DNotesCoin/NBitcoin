CREATE TABLE [dbo].[Transaction]
(
	[Id] int NOT NULL IDENTITY,
	[BlockId] int NOT NULL,
	[Hash] VARCHAR(64) NOT NULL,
	CONSTRAINT [PK_Transaction] PRIMARY KEY ([Id]),
	CONSTRAINT [FK_Transaction_Block_BlockId] FOREIGN KEY  (BlockId)  REFERENCES Block(Id)
)