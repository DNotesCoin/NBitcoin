CREATE TABLE [dbo].[TransactionOutput]
(
	[Id] int NOT NULL IDENTITY,
	[TransactionId] int NOT NULL, 
    [AddressId] INT NOT NULL,
    [Value] BIGINT NOT NULL, 
	CONSTRAINT [PK_TransactionOutput] PRIMARY KEY ([Id]),
	CONSTRAINT [FK_TransactionOutput_Transaction_TransactionId] FOREIGN KEY  ([TransactionId])  REFERENCES [Transaction](Id),
	CONSTRAINT [FK_TransactionOutput_Address_AddressId] FOREIGN KEY  ([AddressId])  REFERENCES [Address](Id)

)
