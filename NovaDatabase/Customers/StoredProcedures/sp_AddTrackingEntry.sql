CREATE PROCEDURE [Customers].[sp_AddTrackingEntry]
(
	@OperationType NCHAR(1) = 'U'
	,@Date DATETIME
	,@CustomerId INT
	,@SystemUserId INT = NULL

)
AS
BEGIN
	INSERT INTO [Customers].[CustomerTracking] (OperationType, Date, CustomerId, SystemUserId)
	VALUES (@OperationType, @Date, @CustomerId, @SystemUserId) 
END
	