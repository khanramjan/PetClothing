-- Reset sequences if needed
ALTER SEQUENCE "Users_Id_seq" RESTART WITH 1;
ALTER SEQUENCE "Categories_Id_seq" RESTART WITH 1;
ALTER SEQUENCE "Products_Id_seq" RESTART WITH 1;
ALTER SEQUENCE "ProductImages_Id_seq" RESTART WITH 1;

-- Delete existing data
DELETE FROM "ProductImages";
DELETE FROM "Products";
DELETE FROM "Categories";
DELETE FROM "Users";

-- Insert Admin User
INSERT INTO "Users" ("Email", "PasswordHash", "FirstName", "LastName", "Role", "IsActive", "CreatedAt")
VALUES ('admin@petshop.com', '$2a$11$lXevRVHSfKZcBHT/egSXau/FnrzQ3wqzC9O.V4TyK3Yq8NN9aLAxe', 'Admin', 'User', 'Admin', true, NOW());

-- Insert Categories
INSERT INTO "Categories" ("Name", "Description", "IsActive", "DisplayOrder", "CreatedAt")
VALUES 
    ('Dog Clothing', 'Stylish and comfortable clothing for dogs', true, 1, NOW()),
    ('Cat Clothing', 'Fashionable outfits for cats', true, 2, NOW()),
    ('Bird Accessories', 'Accessories and clothing for birds', true, 3, NOW()),
    ('Pet Costumes', 'Fun costumes for all pets', true, 4, NOW());

-- Insert Sample Products
INSERT INTO "Products" ("Name", "Description", "Price", "DiscountPrice", "SKU", "StockQuantity", "CategoryId", "PetType", "Size", "Color", "Material", "IsActive", "IsFeatured", "Rating", "ReviewCount", "CreatedAt")
VALUES 
    ('Classic Dog Hoodie', 'Comfortable and stylish hoodie for dogs', 29.99, 24.99, 'DOG-HOODIE-001', 50, 1, 'Dog', 'S,M,L,XL', 'Blue', 'Cotton Blend', true, true, 4.5, 12, NOW()),
    ('Cat Sweater', 'Warm sweater perfect for cold weather', 24.99, 19.99, 'CAT-SWEATER-001', 30, 2, 'Cat', 'XS,S,M,L', 'Pink', 'Wool Blend', true, true, 4.8, 8, NOW()),
    ('Dog Rain Jacket', 'Waterproof jacket for rainy days', 34.99, null, 'DOG-RAIN-001', 25, 1, 'Dog', 'M,L,XL,XXL', 'Yellow', 'Polyester', true, false, 4.2, 5, NOW()),
    ('Bird Sweater', 'Tiny sweater for small birds', 12.99, 9.99, 'BIRD-SWEATER-001', 100, 3, 'Bird', 'XS', 'Red', 'Acrylic', true, false, 4.0, 3, NOW()),
    ('Dog Costume Set', 'Fun dinosaur costume for dogs', 39.99, 29.99, 'DOG-COSTUME-001', 15, 4, 'Dog', 'S,M,L', 'Green', 'Polyester', true, true, 4.7, 20, NOW()),
    ('Cat Collar Sweater', 'Elegant sweater for cats', 18.99, null, 'CAT-COL-SWEATER', 40, 2, 'Cat', 'S,M', 'Black', 'Cotton', true, false, 4.3, 6, NOW());

-- Insert Product Images
INSERT INTO "ProductImages" ("ProductId", "ImageUrl", "AltText", "IsPrimary", "DisplayOrder", "CreatedAt")
VALUES 
    (1, 'https://images.unsplash.com/photo-1537151608828-8661f6e4c5e0?w=400&h=400&fit=crop', 'Blue dog hoodie front view', true, 1, NOW()),
    (1, 'https://images.unsplash.com/photo-1537151608828-8661f6e4c5e0?w=400&h=400&fit=crop', 'Blue dog hoodie side view', false, 2, NOW()),
    (2, 'https://images.unsplash.com/photo-1596854407944-bf87f6fdd49e?w=400&h=400&fit=crop', 'Pink cat sweater', true, 1, NOW()),
    (3, 'https://images.unsplash.com/photo-1599599810694-b5ac4dd64352?w=400&h=400&fit=crop', 'Yellow dog rain jacket', true, 1, NOW()),
    (4, 'https://images.unsplash.com/photo-1607872556501-b046db2f5f1f?w=400&h=400&fit=crop', 'Red bird sweater', true, 1, NOW()),
    (5, 'https://images.unsplash.com/photo-1558788353-f76d92427f16?w=400&h=400&fit=crop', 'Green dinosaur costume for dogs', true, 1, NOW()),
    (6, 'https://images.unsplash.com/photo-1537151608828-8661f6e4c5e0?w=400&h=400&fit=crop', 'Black cat collar sweater', true, 1, NOW());
