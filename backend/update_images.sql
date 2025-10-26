-- Update product images with better Unsplash URLs
UPDATE "ProductImages" SET 
  "ImageUrl" = CASE 
    WHEN "ProductId" = 1 AND "DisplayOrder" = 1 THEN 'https://images.unsplash.com/photo-1537151608828-8661f6e4c5e0?w=400&h=400&fit=crop'
    WHEN "ProductId" = 1 AND "DisplayOrder" = 2 THEN 'https://images.unsplash.com/photo-1537151608828-8661f6e4c5e0?w=400&h=400&fit=crop'
    WHEN "ProductId" = 2 THEN 'https://images.unsplash.com/photo-1596854407944-bf87f6fdd49e?w=400&h=400&fit=crop'
    WHEN "ProductId" = 3 THEN 'https://images.unsplash.com/photo-1599999810694-b5ac4dd64352?w=400&h=400&fit=crop'
    WHEN "ProductId" = 4 THEN 'https://images.unsplash.com/photo-1607872556501-b046db2f5f1f?w=400&h=400&fit=crop'
    WHEN "ProductId" = 5 THEN 'https://images.unsplash.com/photo-1558788353-f76d92427f16?w=400&h=400&fit=crop'
    WHEN "ProductId" = 6 THEN 'https://images.unsplash.com/photo-1537151608828-8661f6e4c5e0?w=400&h=400&fit=crop'
    ELSE "ImageUrl"
  END;
