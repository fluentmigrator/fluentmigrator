CREATE OR ALTER PROCEDURE sales.usp_ship_order @order_id BIGINT
AS
BEGIN
    UPDATE sales.orders SET status = 'shipped' WHERE order_id = @order_id;
END;
