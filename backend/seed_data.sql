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
    (1, 'https://via.placeholder.com/400x400?text=Dog+Hoodie+1', 'Blue dog hoodie front view', true, 1, NOW()),
    (1, 'https://via.placeholder.com/400x400?text=Dog+Hoodie+2', 'Blue dog hoodie side view', false, 2, NOW()),
    (2, 'https://via.placeholder.com/400x400?text=Cat+Sweater', 'Pink cat sweater', true, 1, NOW()),
    (3, 'https://via.placeholder.com/400x400?text=Dog+Rain+Jacket', 'Yellow dog rain jacket', true, 1, NOW()),
    (4, 'https://via.placeholder.com/400x400?text=Bird+Sweater', 'Red bird sweater', true, 1, NOW()),
    (5, 'https://via.placeholder.com/400x400?text=Dog+Costume', 'Green dinosaur costume for dogs', true, 1, NOW()),
    (6, 'https://via.placeholder.com/400x400?text=Cat+Collar+Sweater', 'Black cat collar sweater', true, 1, NOW());
