CREATE TABLE [dbo].[Note](
	[ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Text] [nvarchar](4000) NULL,
	[Left] [int] NOT NULL,
	[Top] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
 CONSTRAINT [PK_Note] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
) 

GO

ALTER TABLE [dbo].[Note] ADD  CONSTRAINT [DF_Note_Left]  DEFAULT ((0)) FOR [Left]
GO

ALTER TABLE [dbo].[Note] ADD  CONSTRAINT [DF_Note_Top]  DEFAULT ((0)) FOR [Top]
GO

ALTER TABLE [dbo].[Note] ADD  CONSTRAINT [DF_Note_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
---
