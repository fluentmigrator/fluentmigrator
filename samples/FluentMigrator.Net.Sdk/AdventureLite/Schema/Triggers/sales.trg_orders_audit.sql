CREATE OR ALTER TRIGGER sales.trg_orders_audit ON sales.orders AFTER UPDATE
AS
BEGIN
    INSERT INTO audit.order_changes (order_id, changed_at)
    SELECT order_id, SYSUTCDATETIME() FROM inserted;
END;
