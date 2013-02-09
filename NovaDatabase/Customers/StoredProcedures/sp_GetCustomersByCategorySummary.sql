CREATE PROCEDURE [Customers].[sp_GetCustomersByCategorySummary]
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED -- Statistics: Don't lock
	SET NOCOUNT ON

	DECLARE 
		@SumAll FLOAT
		

	DECLARE @tStats TABLE
	(
		ItemRank INT IDENTITY(1,1) NOT NULL PRIMARY KEY
		,CategoryName NVARCHAR(50)
		,CustomerCount INT NOT NULL
		,Percentage FLOAT NULL
	)


	INSERT @tStats (CategoryName, CustomerCount)
	SELECT
		CAT.Name 
		,COUNT(C.Id) CustomerCount
	FROM
		[Customers].[Customers] C
		JOIN [Customers].[Categories] CAT ON CAT.Id = C.CategoryId 
	GROUP BY
		CAT.Id 
		,CAT.Name 
	ORDER BY
		COUNT(C.Id) DESC

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
		CategoryName = CategoryName + ' ('+CAST(CAST(Percentage * 100 AS INT) AS NVARCHAR(5))+'%)'
	FROM
		@tStats


	SELECT
		CategoryName
		,CustomerCount
	FROM
		@tStats
	ORDER BY
		CategoryName

END