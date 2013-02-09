CREATE PROCEDURE [Customers].[sp_AddUpdateLocations]
	@City NVARCHAR(150) = NULL
	,@State NVARCHAR(100) = NULL
	,@Country NVARCHAR(50) = NULL
AS
BEGIN
	
	DECLARE 
		@LocationId INT

	SET @City = LTRIM(RTRIM(@City))
	SET @State = LTRIM(RTRIM(@State))
	SET @Country = LTRIM(RTRIM(@Country))

	SELECT 
		TOP 1 
		@LocationId = L.Id
	FROM
		[Customers].[Locations] L
	WHERE
		L.City = @City
		AND L.[State] = @State
		AND L.Country = @Country

	IF (@LocationId IS NULL)
	BEGIN
		INSERT [Customers].[Locations] ([City], [State], [Country]) VALUES (@City, @State, @Country)
		SET @LocationId = SCOPE_IDENTITY()
	END
	RETURN @LocationId

END