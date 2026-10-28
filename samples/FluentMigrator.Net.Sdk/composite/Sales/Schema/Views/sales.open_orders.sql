CREATE OR ALTER VIEW sales.open_orders AS
SELECT o.order_id FROM sales.orders AS o WHERE o.status = 'open';
