CREATE OR ALTER VIEW sales.open_orders AS
SELECT o.order_id, o.customer_id, o.ordered_at
FROM sales.orders AS o
WHERE o.status = 'open';
