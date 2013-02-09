CREATE PROCEDURE [Customers].[sp_AddUpdateCategories]
	@CategoryId int,
	@Name NVARCHAR(50)
AS
BEGIN
	IF (@CategoryId IS NULL)
	BEGIN
		INSERT [Customers].[Categories] (Name) VALUES (@Name)
		SET @CategoryId = SCOPE_IDENTITY()
	END
	RETURN @CategoryId 
END