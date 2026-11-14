CREATE OR ALTER FUNCTION sales.fn_order_total(@order_id BIGINT)
RETURNS DECIMAL(18,2)
AS
BEGIN
    RETURN (SELECT SUM(line_total) FROM sales.order_lines WHERE order_id = @order_id);
END;
