CREATE PROCEDURE [Customers].[sp_AddUpdateCustomers]
	@CustomerId int
	,@Name NVARCHAR(150)
	,@Gender NCHAR(1)
	,@HouseNumber INT
	,@AddressLine1 NVARCHAR(500)
	,@DOB DATETIME
	,@City NVARCHAR(150) 
	,@State NVARCHAR(100)
	,@Country NVARCHAR(50)
	,@CategoryName NVARCHAR(100)
	,@SystemUserId INT = NULL
	
AS
BEGIN
	DECLARE 
		@CategoryId INT
		,@LocationId INT
		,@RowCount INT
		,@CreatedDate DATETIME
		,@LastModifiedDate DATETIME

	SET @CategoryName = RTRIM(LTRIM(@CategoryName))
	SET @Name = RTRIM(LTRIM(@Name))
	SET @LastModifiedDate = GETDATE()
	SET @CreatedDate = GETDATE()

	SELECT 
		@CategoryId = C.Id
	FROM
		[Customers].[Categories] C
	WHERE
		C.Name = @CategoryName

	IF (@CategoryId IS NULL)
	BEGIN
		EXEC @CategoryId = [Customers].[sp_AddUpdateCategories] @CategoryID, @CategoryName  
	END

	EXEC @LocationId = [Customers].[sp_AddUpdateLocations] @City, @State, @Country 

	IF (@LocationId IS NULL OR @CategoryId IS NULL)
	BEGIN
		RAISERROR('Meta-data for customer could not be located or inserted.', 10,1)
	END

	IF (@CustomerId IS NULL)
	BEGIN
		INSERT INTO Customers.Customers (Name, Gender, HouseNumber, AddressLine1, DOB, LocationId, CategoryId)
		VALUES (@Name, @Gender, @HouseNumber, @AddressLine1, @DOB, @LocationID, @CategoryId) 
		SET @CustomerId = SCOPE_IDENTITY() 
		EXEC [Customers].[sp_AddTrackingEntry] 'C', @CreatedDate, @CustomerId, @SystemUserId
	END
	ELSE
	BEGIN
		UPDATE C
		SET
			Name = @Name
			,Gender = @Gender
			,HouseNumber = @HouseNumber
			,AddressLine1 = @AddressLine1
			,DOB = @DOB
			,LocationId = @LocationId 
		FROM
			[Customers].[Customers] C
		WHERE
			C.Id = @CustomerId 
			AND 
			(
				Name <> @Name
				OR Gender <> @Gender
				OR HouseNumber <> @HouseNumber
				OR AddressLine1 <> @AddressLine1
				OR DOB <> @DOB
				OR LocationId <> @LocationId 
			)

		SET @RowCount = @@ROWCOUNT 

		IF (@RowCount > 0)
		BEGIN
			EXEC [Customers].[sp_AddTrackingEntry] 'U', @CreatedDate, @CustomerId, @SystemUserId -- C.<R>.U.D
		END
	END
	RETURN @CustomerId
		 
END