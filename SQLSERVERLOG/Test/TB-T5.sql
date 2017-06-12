USE [Test]
GO

/****** Object:  Table [dbo].[t5]    Script Date: 06/12/2017 17:21:32 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[t5](
	[id] [int] NOT NULL,
	[age] [int] NOT NULL,
	[name] [nvarchar](400) NULL
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[t5] ADD  DEFAULT (NULL) FOR [name]
GO

