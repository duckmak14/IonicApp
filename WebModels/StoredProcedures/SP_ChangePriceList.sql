CREATE PROCEDURE ChangePriceList
@Percent int,
@CreatedBy int
AS
BEGIN TRY
	BEGIN TRANSACTION 
		UPDATE PricingTables SET Price = CASE
											WHEN Round(Price + (Price * 0.01 * @Percent)/1000, 0) * 1000 >= 0
											THEN Round(Price + (Price * 0.01 * @Percent)/1000, 0) * 1000
											ELSE 0
										END

		INSERT INTO PriceChangePeriod([Percent]
				   ,[CreatedBy]
				   ,[CreatedDate])
		VALUES (@Percent, @CreatedBy, GETDATE())
	COMMIT TRANSACTION
END TRY
BEGIN CATCH
	ROLLBACK
END CATCH
