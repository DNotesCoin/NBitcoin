CREATE TABLE [dbo].[TransactionInput]
(
	[Id] int NOT NULL IDENTITY,
	[TransactionId] int NOT NULL, 
    [Index] INT NOT NULL,
	CONSTRAINT [PK_TransactionInput] PRIMARY KEY ([Id]),
	CONSTRAINT [FK_TransactionInput_Transaction_TransactionId] FOREIGN KEY  ([TransactionId])  REFERENCES [Transaction](Id)

)
