using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Demo.Infrastructure.Catalog;
using Demo.Infrastructure.Identity;

namespace Demo.Infrastructure.Seeding;

public static class DataSeeder
{
    private static readonly (string Name, string Description, string Type, int Priority)[] Roles =
    [
        ("Admin",  "Full administrative access", "superuser",    100),
        ("Seller", "Access to seller features", "user",       10),
        ("Buyer",  "Access to buyer features",  "user",       1),
    ];

    public static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager, ILogger logger)
    {
        foreach (var (name, description, type, priority) in Roles)
        {
            if (await roleManager.RoleExistsAsync(name))
                continue;

            var role = new ApplicationRole(name)
            {
                Id = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                Description = description,
                RoleType = type,
                IsActive = true,
                Priority = priority,
                CreatedBy = "system",
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = "system",
                UpdatedAt = DateTime.UtcNow,
            };

            var result = await roleManager.CreateAsync(role);

            if (result.Succeeded)
                logger.LogInformation("Seeded role '{Role}'", name);
            else
                logger.LogError("Failed to seed role '{Role}': {Errors}", name,
                    string.Join(", ", result.Errors.Select(e => $"{e.Code}: {e.Description}")));
        }
    }

    public static async Task SeedProductCategoriesAsync(
        IDbContextFactory<DemoCatalogDbContext> dbContextFactory,
        ILogger logger)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        if (await context.ProductCategories.AnyAsync())
        {
            logger.LogInformation("Product categories already seeded, skipping.");
            return;
        }

        var feeCategoryId = await context.FeeCategories
            .Where(f => f.Code == "final_value")
            .Select(f => f.FeeCategoryId)
            .FirstOrDefaultAsync();

        if (feeCategoryId == Guid.Empty)
        {
            logger.LogError("Cannot seed product categories: no 'final_value' fee category found. Ensure fee categories are seeded first.");
            return;
        }

        var topLevel = new (string Name, string Slug, int Sort)[]
        {
            ("Electronics",          "electronics",       1),
            ("Clothing & Apparel",   "clothing-apparel",  2),
            ("Home & Garden",        "home-garden",       3),
            ("Sports & Outdoors",    "sports-outdoors",   4),
            ("Health & Beauty",      "health-beauty",     5),
            ("Books, Music & Media", "books-music-media", 6),
            ("Toys & Baby",          "toys-baby",         7),
            ("Automotive",           "automotive",        8),
            ("Collectibles & Art",   "collectibles-art",  9),
        };

        var topEntities = topLevel.Select(c => new ProductCategoryEntity
        {
            CategoryId = Guid.NewGuid(),
            CategoryGuid = Guid.NewGuid(),
            Name = c.Name,
            Slug = c.Slug,
            IsActive = true,
            ParentCategoryId = null,
            Level = 0,
            SortOrder = c.Sort,
            FeeCategoryId = feeCategoryId
        }).ToList();

        context.ProductCategories.AddRange(topEntities);
        await context.SaveChangesAsync();

        var lookup = topEntities.ToDictionary(c => c.Name);

        var subs = new (string Name, string Slug, string Parent, int Sort)[]
        {
            ("Mobile Phones & Accessories", "mobile-phones",      "Electronics",          1),
            ("Laptops & Computers",         "laptops-computers",  "Electronics",          2),
            ("Tablets",                     "tablets",            "Electronics",          3),
            ("Audio & Headphones",          "audio-headphones",   "Electronics",          4),
            ("Cameras & Photography",       "cameras",            "Electronics",          5),
            ("TVs & Home Theatre",          "tvs-home-theatre",   "Electronics",          6),
            ("Gaming & Consoles",           "gaming-consoles",    "Electronics",          7),
            ("Wearables & Smart Watches",   "wearables",          "Electronics",          8),

            ("Men's Clothing",              "mens-clothing",      "Clothing & Apparel",   1),
            ("Women's Clothing",            "womens-clothing",    "Clothing & Apparel",   2),
            ("Kids' Clothing",              "kids-clothing",      "Clothing & Apparel",   3),
            ("Shoes & Footwear",            "shoes-footwear",     "Clothing & Apparel",   4),
            ("Bags & Luggage",              "bags-luggage",       "Clothing & Apparel",   5),
            ("Jewellery & Accessories",     "jewellery",          "Clothing & Apparel",   6),

            ("Furniture",                   "furniture",          "Home & Garden",        1),
            ("Kitchen & Dining",            "kitchen-dining",     "Home & Garden",        2),
            ("Bedding & Bath",              "bedding-bath",       "Home & Garden",        3),
            ("Garden & Outdoor",            "garden-outdoor",     "Home & Garden",        4),
            ("Home Decor",                  "home-decor",         "Home & Garden",        5),
            ("Tools & Hardware",            "tools-hardware",     "Home & Garden",        6),

            ("Exercise & Fitness",          "exercise-fitness",   "Sports & Outdoors",    1),
            ("Cycling",                     "cycling",            "Sports & Outdoors",    2),
            ("Camping & Hiking",            "camping-hiking",     "Sports & Outdoors",    3),
            ("Water Sports",                "water-sports",       "Sports & Outdoors",    4),
            ("Team Sports",                 "team-sports",        "Sports & Outdoors",    5),

            ("Skincare",                    "skincare",           "Health & Beauty",      1),
            ("Hair Care",                   "hair-care",          "Health & Beauty",      2),
            ("Vitamins & Supplements",      "vitamins",           "Health & Beauty",      3),
            ("Personal Care",               "personal-care",      "Health & Beauty",      4),

            ("Books",                       "books",              "Books, Music & Media", 1),
            ("Music",                       "music",              "Books, Music & Media", 2),
            ("Movies & TV",                 "movies-tv",          "Books, Music & Media", 3),
            ("Video Games",                 "video-games",        "Books, Music & Media", 4),

            ("Baby Gear",                   "baby-gear",          "Toys & Baby",          1),
            ("Toys & Games",                "toys-games",         "Toys & Baby",          2),
            ("Learning & Education",        "learning-education", "Toys & Baby",          3),

            ("Car Parts & Accessories",     "car-parts",          "Automotive",           1),
            ("Tyres & Wheels",              "tyres-wheels",       "Automotive",           2),
            ("Car Electronics",             "car-electronics",    "Automotive",           3),

            ("Trading Cards",               "trading-cards",      "Collectibles & Art",   1),
            ("Coins & Stamps",              "coins-stamps",       "Collectibles & Art",   2),
            ("Art & Prints",                "art-prints",         "Collectibles & Art",   3),
            ("Antiques",                    "antiques",           "Collectibles & Art",   4),
        };

        var subEntities = subs.Select(s => new ProductCategoryEntity
        {
            CategoryId = Guid.NewGuid(),
            CategoryGuid = Guid.NewGuid(),
            Name = s.Name,
            Slug = s.Slug,
            IsActive = true,
            ParentCategoryId = lookup[s.Parent].CategoryId,
            Level = 1,
            SortOrder = s.Sort,
            FeeCategoryId = feeCategoryId
        }).ToList();

        context.ProductCategories.AddRange(subEntities);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} product categories.", topEntities.Count + subEntities.Count);
    }

    public static async Task SeedFeeCategoriesAsync(
        IDbContextFactory<DemoCatalogDbContext> dbContextFactory,
        ILogger logger)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();

        if (await context.FeeCategories.AnyAsync())
        {
            logger.LogInformation("Fee categories already seeded, skipping.");
            return;
        }

        var fees = new (string Code, string Name, string Description, string Group, int Sort)[]
        {
            // Listing & Publishing
            ("listing",          "Listing Fee",              "Flat fee charged per product listed",                       "Listing & Publishing",     1),
            ("insertion",        "Insertion Fee",            "Fee per re-listing or additional listing slot",             "Listing & Publishing",     2),
            ("featured_listing", "Featured Listing Fee",     "Premium placement fee for boosted visibility",              "Listing & Publishing",     3),

            // Sales & Transactions
            ("final_value",             "Final Value Fee",            "Percentage of sale price on a completed transaction",   "Sales & Transactions", 1),
            ("transaction",             "Transaction Fee",            "Payment processing fee (e.g. 2.9% + flat rate)",        "Sales & Transactions", 2),
            ("international_transaction","International Transaction",  "Cross-border surcharge applied to overseas sales",      "Sales & Transactions", 3),
            ("currency_conversion",     "Currency Conversion Fee",    "Fee for converting foreign currency payments",          "Sales & Transactions", 4),

            // Subscriptions & Plans
            ("seller_subscription", "Seller Subscription",  "Monthly or annual seller plan (Basic, Standard, Premium)",  "Subscriptions & Plans", 1),
            ("store_subscription",  "Store Subscription",   "Storefront subscription tier fee",                          "Subscriptions & Plans", 2),

            // Advertising & Promotion
            ("promoted_listing", "Promoted Listing Fee",    "Cost-per-click or % of sale for promoted listings",         "Advertising & Promotion", 1),
            ("banner_ad",        "Banner Ad Fee",           "Fixed fee for banner ad placement on the platform",         "Advertising & Promotion", 2),

            // Fulfilment & Logistics
            ("shipping_label",  "Shipping Label Fee",       "Fee charged for purchasing a shipping label",               "Fulfilment & Logistics", 1),
            ("fulfillment",     "Fulfilment Fee",           "Pick, pack, and ship fee for platform-managed fulfilment",  "Fulfilment & Logistics", 2),
            ("storage",         "Storage Fee",              "Warehouse storage fee per unit per month",                  "Fulfilment & Logistics", 3),

            // Post-Sale & Disputes
            ("return_processing", "Return Processing Fee",  "Fee charged when a buyer returns an item",                  "Post-Sale & Disputes", 1),
            ("dispute",           "Dispute Fee",            "Fee applied for a chargeback or payment dispute",           "Post-Sale & Disputes", 2),
            ("refund_adjustment", "Refund Adjustment",      "Partial credit of final value fee when a refund is issued", "Post-Sale & Disputes", 3),

            // Payout & Finance
            ("payout",          "Payout Fee",              "Fee per withdrawal or payout to a bank account",            "Payout & Finance", 1),
            ("currency_payout", "Currency Payout Fee",     "Fee for payout issued in a foreign currency",               "Payout & Finance", 2),
        };

        var entities = fees.Select(f => new FeeCategoryEntity
        {
            FeeCategoryId = Guid.NewGuid(),
            Code = f.Code,
            Name = f.Name,
            Description = f.Description,
            Group = f.Group,
            IsActive = true,
            SortOrder = f.Sort
        }).ToList();

        context.FeeCategories.AddRange(entities);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} fee categories.", entities.Count);
    }
}
