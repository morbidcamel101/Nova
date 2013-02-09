CREATE PROCEDURE [Customers].[sp_GetCustomersByName]
	@Name NVARCHAR(500) = NULL	
AS
BEGIN
	SELECT 
		C.Id 
	FROM
		[Customers].[Customers] C
	WHERE
		C.Name LIKE '%'+@Name+'%' 
END

	