CREATE PROCEDURE [Customers].[sp_GetCustomerByLocationSummary]
	@LocationMode int = 0 -- Mode == City: 0, State: 1, Country: 2
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED -- Statistics: Don't lock
	SET NOCOUNT ON

	DECLARE @RestRank INT
			,@MaxRows INT
			,@SumAll FLOAT

	DECLARE @tStats TABLE
	(
		ItemRank INT IDENTITY(1,1) NOT NULL PRIMARY KEY
		,Country NVARCHAR(50) NULL
		,State NVARCHAR(100) NULL
		,City NVARCHAR(150) NULL
		,CustomerCount INT NOT NULL
		,Percentage FLOAT NULL
	)

	SET @MaxRows = 10 -- Better for pie charts

	IF (@LocationMode = 0)
	BEGIN
		INSERT @tStats
		(
			Country
			,State
			,City
			,CustomerCount
		)
		SELECT
			L.Country 
			,L.State 
			,L.City
			,COUNT(C.Id) CustomerCount
		FROM
			[Customers].[Customers] C 
			JOIN [Customers].[Locations] L ON L.Id = C.LocationId 
		GROUP BY
			L.Country -- Can't just do it by city, can be duplicate cities accross states 
			,L.State 
			,L.City
		ORDER BY
			COUNT(C.Id) DESC
	END
	ELSE IF (@LocationMode = 1)
	BEGIN
		INSERT @tStats
		(
			Country
			,State
			,CustomerCount
		)
		SELECT
			L.Country 
			,L.State 
			,COUNT(C.Id) CustomerCount
		FROM
			[Customers].[Customers] C 
			JOIN [Customers].[Locations] L ON L.Id = C.LocationId 
		GROUP BY
			L.Country 
			,L.State 
		ORDER BY
			COUNT(C.Id) DESC
	END
	ELSE IF (@LocationMode = 2)
	BEGIN
		INSERT @tStats
		(
			Country
			,CustomerCount
		)
		SELECT
			L.Country 
			,COUNT(C.Id) CustomerCount
		FROM
			[Customers].[Customers] C 
			JOIN [Customers].[Locations] L ON L.Id = C.LocationId 
		GROUP BY
			L.Country 
		ORDER BY
			COUNT(C.Id) DESC
	END
	ELSE
	BEGIN
		DECLARE @Error NVARCHAR(200)
		SET @Error = 'Invalid stored procedure usage. Invalid value for parameter @LocationMode!'
		RAISERROR (@Error, 10, 1)
	END

	-- Logic to collapse the stats
	IF EXISTS(SELECT ItemRank FROM @tStats WHERE ItemRank > @MaxRows)
	BEGIN
		INSERT @tStats
		(
			City
			,Country
			,State
			,CustomerCount
		)
		SELECT
			'Other'
			,'Other'
			,'Other'
			,SUM(CustomerCount)
		FROM
			@tStats
		WHERE
			ItemRank > @MaxRows

		SET @RestRank = SCOPE_IDENTITY()

		DELETE S
		FROM
			@tStats S
		WHERE
			S.ItemRank > @MaxRows
			AND S.ItemRank <> @RestRank
	END

	SELECT 
		@SumAll = CAST(SUM(CustomerCount) AS FLOAT)
	FROM
		@tStats


	UPDATE @tStats
	SET
		Percentage = CASE WHEN @SumAll <> 0 THEN CAST(CustomerCount AS FLOAT) / @SumAll ELSE 0 END
	FROM
		@tStats

	UPDATE @tStats
	SET
		State = State + ' ('+CAST(CAST(Percentage * 100 AS INT) AS NVARCHAR(5))+'%)'
	FROM
		@tStats

	SELECT
		Country
		,State
		,City
		,CustomerCount
	FROM
		@tStats
END